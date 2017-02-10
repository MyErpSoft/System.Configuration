using System.Collections.Generic;
using System.Configuration.Core.Metadata;
using System.IO;
using System.Linq;

namespace System.Configuration.Core.Dc {

    partial class DcPackageWriter {
        internal class DcPartWriter {
            private DcPackageWriter _dcPackageWriter;
            private PartData _partData;
            private DcBinaryWriter _writer;
            private ObjectTypeData _typeData;

            public DcPartWriter(DcPackageWriter dcPackageWriter, PartData partData) {
                this._dcPackageWriter = dcPackageWriter;
                this._partData = partData;

                var stream = new MemoryStream();
                _writer = new DcBinaryWriter(stream);
            }

            public void WriteObjectType() {

                //类型信息。
                _typeData = _dcPackageWriter._typeDict.GetId(_partData.Part.Type);
                _writer.Write7BitInt(_typeData.Order);

                //注意这里，基类信息没有在这里写入，而是作为property写入，即在下面的values循环中。
                //_writer.Write(part.Base, ctx);

                //todo:action='edit'
            }

            public void WriteProperties() {
                var values = _partData.Part.GetLocalValues().ToArray();
                //值总数。
                _writer.Write7BitInt(values.Length);

                foreach (var pair in values) {
                    //属性位置。注意，这里的位置是此属性在Type上的位置（使用编码器创建的位置，非顺序位置），不是属性字符串在列表中的位置。
                    _writer.Write7BitInt(_typeData.Properties.GetId(pair.Key));
                    WriteValue(pair.Key.PropertyType, pair.Value);
                }
            }


            private void WriteValue(IType valueType, object value) {
                if (value == null) {
                    //NULL
                    _writer.Write((byte)0); //0 Null, 1 简单值 ，2 指针 ，3 集合
                }
                else if (value is ObjectPtr) {
                    //对象指针。
                    this.WriteObjectPtr((ObjectPtr)value);
                }
                else {
                    IEnumerable<ListDifferenceItem> list = value as IEnumerable<ListDifferenceItem>;
                    if (list != null) {
                        _writer.Write((byte)4);
                        //集合
                        _writer.Write7BitInt(list.Count());
                        foreach (var item in list) {
                            this.WriteListDifferenceAction(item.Action);
                            this.WriteValue(null, item.Item); //目前设计上的缺陷，不能给出集合的参考数据类型。
                        }
                    }
                    else {
                        _writer.Write((byte)1);
                        //普通值。
                        if (valueType == null) {
                            Utilities.ThrowNotSupported("");
                        }
                        var convert = valueType.GetConverter();
                        var str = convert.ConvertToInvariantString(value);
                        _writer.Write(str ?? string.Empty);
                    }
                }
            }

            private void WriteObjectPtr(ObjectPtr ptr) {
                if (ptr == ObjectPtr.None) {
                    _writer.Write((byte)0);
                }
                else {
                    var name = ptr.Name;
                    //区分外部指针还是内部指针
                    if (string.Equals(name.PackageName, _dcPackageWriter._sourcePackageName, StringComparison.OrdinalIgnoreCase)) {
                        _writer.Write((byte)3);
                        _writer.Write7BitInt(_dcPackageWriter._partDatas.GetId(name.FullName));
                    }
                    else {
                        _writer.Write((byte)2);
                        _writer.Write7BitInt(_dcPackageWriter._stringDict.GetId(name.Namespace));
                        _writer.Write(name.Name);
                        _writer.Write7BitInt(_dcPackageWriter._stringDict.GetId(name.PackageName));
                    }
                }
            }

            private void WriteListDifferenceAction(ListDifferenceAction action) {
                if (action == ListDifferenceAction.Add) {
                    _writer.Write((byte)0);
                }
                else if (action == ListDifferenceAction.Remove) {
                    _writer.Write((byte)1);
                }
                else {
                    Utilities.ThrowNotSupported("错误的列表差量动作值。");
                }
            }

            public byte[] ToArray() {
                var data = ((MemoryStream)_writer.BaseStream).ToArray();
                _writer.BaseStream.Close();
                _writer.Close();

                return data;
            }
        }
    }
}