using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Configuration.Core.Metadata;

namespace System.Configuration.Core.Dc {

    /// <summary>
    /// 二进制格式的Package写入器。
    /// </summary>
    internal sealed class BinaryPackageWriter : BinaryWriter {
        const string version = "2.0";
        const string fileHead = "Difference Configuration File " + version;

        public BinaryPackageWriter(Stream output):base(output) {
        }

        public void WritePackage(Package package,OpenDataContext openDataContext) {
            if (package == null) {
                Utilities.ThrowArgumentNull(nameof(package));
            }
            if (openDataContext == null) {
                Utilities.ThrowArgumentNull(nameof(openDataContext));
            }

            PackageWriteContext ctx = new PackageWriteContext();
            List<PartData> partDatas = new List<PartData>();
            
            foreach (var part in package.GetParts()) {
                part.Value.OpenData(openDataContext);
                partDatas.Add(new PartData(part.Key,part.Value,ctx));
            }

            //1
            this.WriteFileHead();

            //2 仅仅namespace和name需要的字符串
            var priorityStrings = ctx.StringDict.GetItems();
            this.WriteStrings(priorityStrings,0);

            //3 对象标识清单
            this.WriteObjectIds(partDatas);

            //4 值数据块
            this.WriteObjects(partDatas, ctx);

            //5 类型
            this.WriteObjectTypes(ctx.TypeDict.GetItems(), ctx);

            //6 剩余的字符串
            this.WriteStrings(ctx.StringDict.GetItems(), priorityStrings.Length);
        }

        //写入文件头
        private void WriteFileHead() {
            this.Write(fileHead);
        }

        //写入字符串清单表
        // int              命名空间总数
        // string[]         顺序填写的字符串
        private void WriteStrings(string[] strings,int startIndex) {
            //写入总共多少，以便读取时知道什么时候结束了。
            this.Write7BitEncodedInt(strings.Length);

            for (int i = startIndex; i < strings.Length; i++) {
                this.Write(strings[i]);//不能出现null.
            }
        }

        //写入对象标识清单
        // {int,int}    {命名空间序号，名称序号}
        private void WriteObjectIds(List<PartData> partDatas) {
            this.Write7BitEncodedInt(partDatas.Count);
            
            //为加快读取时的速度，读取程序先将对象标识清单全部读取到内存，直接返回，
            //后台程序异步完成数据块的加载和解码（而不是像Dcxml那样读取所有数据 和 用到的时候才解码）。
            foreach (var item in partDatas) {

                //对象的编号
                this.Write7BitEncodedInt(item.Namespace);
                this.Write7BitEncodedInt(item.Name);
            }
        }

        // 写入对象类型清单
        // int                  总数
        // {int,int,int,int}[]      {命名空间序号，名称序号，库序号,提供者}
        private void WriteObjectTypes(ObjectTypeData[] types, PackageWriteContext ctx) {
            this.Write7BitEncodedInt(types.Length);

            foreach (var item in types) {
                this.Write(item.Type.QualifiedName,ctx);

                //使用到的属性清单。
                var properties = item.Properties.GetItems();
                this.Write7BitEncodedInt(properties.Length);
                foreach (var property in properties) {
                    this.Write7BitEncodedInt(ctx.StringDict.GetId(property.Name));//注意，这里字符串位置信息。
                }
            }
        }

        private void WriteObjects(List<PartData> partDatas, PackageWriteContext ctx) {
            this.Write7BitEncodedInt(partDatas.Count);

            foreach (var item in partDatas) {
                this.WriteObject(item.Part,ctx);
            }
        }

        private void WriteObject(ConfigurationObjectPart part, PackageWriteContext ctx) {
            //记录当前的位置，先先入数据块大小（0），然后等写完毕后就知道实际多大，然后重新写入正确的值。
            var startPosition = this.OutStream.Position;
            this.Write(0); //不能使用Write7BitEncodedInt，那个是动态大小的。

            //类型信息。
            var typeData = ctx.TypeDict.GetId(part.Type);
            this.Write7BitEncodedInt(typeData.Order);

            //基类
            this.Write(part.Base, ctx);

            var values = part.GetLocalValues();
            //值总数。
            foreach (var pair in values) {
                //属性位置。注意，这里的位置是此属性在Type上的位置（使用编码器创建的位置，非顺序位置），不是属性字符串在列表中的位置。
                this.Write7BitEncodedInt(typeData.Properties.GetId(pair.Key));
                WriteValue(ctx, pair.Key.PropertyType, pair.Value);
            }

            //重新补入数据块大小
            var endPosition = this.OutStream.Position;
            this.OutStream.Position = startPosition;
            this.Write(endPosition - startPosition);
            this.OutStream.Position = endPosition;
        }

        private void WriteValue(PackageWriteContext ctx,IType valueType, object value) {
            if (value == null) {
                //NULL
                this.Write((byte)0); //0 Null, 1 简单值 ，2 指针 ，3 集合
            }
            else if (value is ObjectPtr) {
                this.Write((byte)2);

                ObjectPtr ptr = (ObjectPtr)value;
                //对象指针。
                this.Write(ptr, ctx);
            }
            else {
                IEnumerable<ListDifferenceItem> list = value as IEnumerable<ListDifferenceItem>;
                if (list != null) {
                    this.Write((byte)3);
                    //集合
                    this.Write7BitEncodedInt(list.Count());
                    foreach (var item in list) {
                        if (item.Action == ListDifferenceAction.Add) {
                            this.Write((byte)0);
                        }
                        else {
                            this.Write((byte)1);
                        }
                        this.WriteValue(ctx, null, item.Item); //目前设计上的缺陷，不能给出集合的参考数据类型。
                    }
                }
                else {
                    this.Write((byte)1);
                    //普通值。
                    if (valueType == null) {
                        Utilities.ThrowNotSupported("");
                    }
                    var convert = valueType.GetConverter();
                    var str = convert.ConvertToInvariantString(value);
                    this.Write(str ?? string.Empty);
                }
            }
        }

        private void Write(ObjectPtr ptr, PackageWriteContext ctx) {
            this.Write(ptr.Name, ctx);
        }

        private void Write(QualifiedName name, PackageWriteContext ctx) {
            if (name == null) {
                this.Write7BitEncodedInt(0);
            }
            else {
                this.Write7BitEncodedInt(ctx.StringDict.GetId(name.FullName.Namespace));
                this.Write7BitEncodedInt(ctx.StringDict.GetId(name.FullName.Name));
                this.Write7BitEncodedInt(ctx.StringDict.GetId(name.PackageName));
            }
        }

        private void Write(ObjectTypeQualifiedName name, PackageWriteContext ctx) {
            this.Write((QualifiedName)name, ctx);

            if (name != null) {
                this.Write7BitEncodedInt(ctx.StringDict.GetId(name.ProviderName));
            }
        }
    }
}
