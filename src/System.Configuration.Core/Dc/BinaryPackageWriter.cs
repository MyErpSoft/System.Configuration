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
        internal const string version = "2.0";
        internal const string fileHead = "Difference Configuration File " + version;
        
        public BinaryPackageWriter(Stream output):base(output) {
        }

        public static void ConvertToDc(string filePath, Package sourcePackage) {
            using (var stream = PlatformUtilities.Current.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write)) {
                using (var writer = new BinaryPackageWriter(stream)) {
                    writer.WritePackage(sourcePackage);
                    stream.Flush();
                }
            }
        }

        public void WritePackage(Package sourcePackage) {
            if (sourcePackage == null) {
                Utilities.ThrowArgumentNull(nameof(sourcePackage));
            }

            PackageWriteContext ctx = new PackageWriteContext(sourcePackage);
            List<PartData> partDatas = new List<PartData>();
            
            foreach (var part in sourcePackage.GetParts()) {
                partDatas.Add(new PartData(part.Key,part.Value,ctx));
            }

            //1
            this.WriteFileHead();

            //2 仅仅namespace和name需要的字符串
            var priorityStrings = ctx.StringDict.GetItems();
            this.WriteStrings(priorityStrings,0);

            //3 对象标识清单
            this.WriteObjectIds(partDatas);

            //为了方便读取程序，写入的顺序实际是比较别扭的。
            using (var ms = new MemoryStream()) {
                using (var writer = new BinaryPackageWriter(ms)) {
                    //4 值数据块
                    writer.WriteObjects(partDatas, ctx);

                    var typeStartIndex = ms.Position;
                    //5 类型
                    writer.WriteObjectTypes(ctx.TypeDict.GetItems(), ctx);

                    var stringStartIndex = ms.Position;
                    //6 剩余的字符串
                    writer.WriteStrings(ctx.StringDict.GetItems(), priorityStrings.Length);

                    //现在，需要将数据倒过来写入文件流，以便读取时比较方便。
                    CopyStream(ms, this.BaseStream, stringStartIndex, (ms.Position - stringStartIndex));
                    CopyStream(ms, this.BaseStream, typeStartIndex, (stringStartIndex - typeStartIndex));
                    CopyStream(ms, this.BaseStream, 0, typeStartIndex);
                }
            }
            
        }

        private static void CopyStream(MemoryStream source, Stream target, long startIndex, long size) {
            target.Write(source.GetBuffer(), (int)startIndex, (int)size);
        }


        //写入文件头
        private void WriteFileHead() {
            foreach (var item in BinaryPackageWriter.fileHead) {
                this.Write(item);
            }
        }

        //写入字符串清单表
        // int              命名空间总数
        // string[]         顺序填写的字符串
        private void WriteStrings(string[] strings,int startIndex) {
            //写入总共多少，以便读取时知道什么时候结束了。
            this.Write7BitEncodedInt(strings.Length - startIndex);

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
            foreach (var item in partDatas) {
                this.WriteObject(item.Part,ctx);
            }
        }

        private void WriteObject(ConfigurationObjectPart part, PackageWriteContext ctx) {
            //记录当前的位置，先先入数据块大小（0），然后等写完毕后就知道实际多大，然后重新写入正确的值。
            this.Write(0); //不能使用Write7BitEncodedInt，那个是动态大小的。
            var startPosition = this.OutStream.Position;

            //类型信息。
            var typeData = ctx.TypeDict.GetId(part.Type);
            this.Write7BitEncodedInt(typeData.Order);

            //注意这里，基类信息没有在这里写入，而是作为property写入，即在下面的values循环中。
            //this.Write(part.Base, ctx);

            //todo:action='edit'

            var values = part.GetLocalValues();
            //值总数。
            this.Write7BitEncodedInt(values.Count());

            foreach (var pair in values) {
                //属性位置。注意，这里的位置是此属性在Type上的位置（使用编码器创建的位置，非顺序位置），不是属性字符串在列表中的位置。
                this.Write7BitEncodedInt(typeData.Properties.GetId(pair.Key));
                WriteValue(ctx, pair.Key.PropertyType, pair.Value);
            }

            //重新补入数据块大小
            var endPosition = this.OutStream.Position;
            //注意这里减去4，因为size是先写入再记录startPosition，所以实际位置要偏左（int32是4字节），大概像下面这个样子：
            //07 01 0D 28 00 00 00 01     04 01 01 02 4F 4B 02 01
            //         ^           ^
            //         大小描述     数据块开始
            this.OutStream.Position = startPosition - 4; 
            this.Write((int)(endPosition - startPosition));
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
                this.Write((char)0);
            }
            else {
                this.Write((char)1);
                this.Write7BitEncodedInt(ctx.StringDict.GetId(name.FullName.Namespace));
                this.Write7BitEncodedInt(ctx.StringDict.GetId(name.FullName.Name));

                //需要删除包信息，因为包的名称来自文件名（外部），所以对本包的引用应该使用0表示。
                if (name.PackageName == ctx.SourcePackage.Name) {
                    this.Write7BitEncodedInt(0);
                }
                else {
                    this.Write7BitEncodedInt(ctx.StringDict.GetId(name.PackageName));
                }
            }
        }

        private void Write(ObjectTypeQualifiedName name, PackageWriteContext ctx) {
            this.Write(name.QualifiedName, ctx);

            if (name != null) {
                this.Write7BitEncodedInt(ctx.StringDict.GetId(name.ProviderName));
            }
        }
    }
}
