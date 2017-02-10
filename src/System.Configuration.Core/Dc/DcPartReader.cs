using System.Collections.Generic;
using System.Configuration.Core.Collections;
using System.Configuration.Core.Metadata;
using System.IO;
using System.Threading;

namespace System.Configuration.Core.Dc {

    partial class DcPackageReader {

        private DcPartReader _cachedDcPartReader;

        //使用复用技术，但需要考虑并发问题
        internal DcPartReader CreateDcPartReader(int offset) {
            var cachedDcPartReader = Interlocked.Exchange(ref _cachedDcPartReader, null);
            if (cachedDcPartReader == null) {
                cachedDcPartReader = new DcPartReader(this);
            }
            cachedDcPartReader.Seek(offset);

            return cachedDcPartReader;
        }

        /// <summary>
        /// 单个零件在展开数据时需要的读取器。
        /// </summary>
        internal class DcPartReader : IDisposable {
            private readonly DcBinaryReader _reader;
            private readonly DcPackageReader _dcReader;

            public DcPartReader(DcPackageReader dcReader) {
                var input = new MemoryStream(dcReader._data, false);
                _reader = new DcBinaryReader(input);
                _dcReader = dcReader;
            }

            internal void Seek(int Offset) {
                _reader.BaseStream.Position = Offset;
            }

            private ObjectTypeReadData _objectTypeData;
            /// <summary>
            /// 读取类型信息。
            /// </summary>
            /// <returns></returns>
            public IType ReadObjectType() {
                //第一个数据是类型的索引位置
                _objectTypeData = GetObjectTypeData(_reader.Read7BitInt());
                return _objectTypeData.Type;
            }

            internal IEnumerable<KeyValuePair<IProperty, object>> ReadProperties() {
                //存储的“属性-值 对” 的个数。
                var count = _reader.Read7BitInt();

                for (int i = 0; i < count; i++) {
                    var propertyIndex = _reader.Read7BitInt();
                    var propertyData = _objectTypeData.Properties[propertyIndex];
                    //property
                    if (propertyData.Property == null) {
                        if (propertyData.Name == ConfigurationObjectPart.BasePropertyInstance.Name) {
                            propertyData.Property = ConfigurationObjectPart.BasePropertyInstance;
                        }
                        else {
                            propertyData.Property = _objectTypeData.Type.GetProperty(propertyData.Name);
                        }
                    }

                    yield return new KeyValuePair<IProperty, object>(propertyData.Property, ReadValue(propertyData.Property.PropertyType));
                }
            }

            //0 Null, 1 简单值 ，2 指针 ，3 IEnumerable<ListDifferenceItem>集合
            private object ReadValue(IType valueType) {
                var flag = _reader.ReadChar();
                switch (flag) {
                    case (char)0:
                        //null
                        return null;
                    case (char)1:
                        //简单值
                        var converter = valueType.GetConverter();
                        var str = _reader.ReadString();
                        return converter.ConvertFromInvariantString(str);
                    case (char)2:
                        //外部的对象指针。
                        return this.ReadObjectPtr();
                    case (char)3:
                        //内部指针，指向当前包的对象清单的索引。
                        return this.ReadThisPackageObjectPtr();
                    case (char)4: {
                            var count = _reader.Read7BitInt();
                            OnlyNextList<ListDifferenceItem> list = new OnlyNextList<ListDifferenceItem>();
                            for (int i = 0; i < count; i++) {
                                var action = ReadListDifferenceAction();
                                list.Add(new ListDifferenceItem(action, this.ReadValue(null)));
                            }
                            return list;
                        }
                    default:
                        Utilities.ThrowArgumentException("数据格式不正确，不能识别的flag", "flag");
                        break;
                }

                return null;
            }

            protected ObjectPtr ReadObjectPtr() {
                string objNamespace, name, packageName;
                //名称是直接存储字符串，主要因为重复率低，而命名空间和packageName重复率高，实际放在InternString中
                objNamespace = GetString(_reader.Read7BitInt());
                name = _reader.ReadString();
                packageName = GetString(_reader.Read7BitInt());

                //packageName 不会等于 null ，内部对象的指针走的是ReadThisPackageObjectPtr流程。
                //name也不会为null，如果是空指针，其flag为0，已经处理。
                return new ObjectPtr(new QualifiedName(objNamespace, name, packageName));
            }

            protected ListDifferenceAction ReadListDifferenceAction() {
                var action =_reader.ReadChar();
                if (action == 0) {
                    return ListDifferenceAction.Add;
                }else if (action == 1) {
                    return ListDifferenceAction.Remove;
                }
                else {
                    Utilities.ThrowNotSupported("错误的列表差量动作值。");
                    return ListDifferenceAction.Add;
                }
            }

            protected ObjectPtr ReadThisPackageObjectPtr() {
                //注意本地指针 不会为null，这个在flag时已经处理。
                var index = _reader.Read7BitInt();
                return new ObjectPtr(new QualifiedName(GetObjectId(index), _dcReader._sourcePackage.Name));
            }
            
            /// <summary>
            /// 读取类型信息。
            /// </summary>
            /// <returns></returns>
            internal ObjectTypeReadData GetObjectTypeData(int typeIndex) {
                var typeData = _dcReader._types[typeIndex];
                //通过类型信息，完成绑定，这里会存在多个线程同时检查Type是否为null，同时去绑定，当没有副作用
                if (typeData.Type == null) {
                    typeData.Type = _dcReader._sourcePackage.Binder.BindToType(typeData.Name);
                }

                return typeData;
            }

            internal FullName GetObjectId(int index) {
                return _dcReader._objectIds[index - 1];
            }

            internal string GetString(int index) {
                return _dcReader._strings[index];
            }

            public void Dispose() {
                //并不释放资源，而是回收资源
                var oldValue = Interlocked.CompareExchange(ref _dcReader._cachedDcPartReader, this, null);
                if (oldValue != null) {
                    //如果_cachedDcPartReader不为null，我们不会替换掉缓存，那么当前实例就没有人使用了。
                    _reader.BaseStream.Close();
                    _reader.Close();
                }
            }
        }//end class DcPartReader

    }//end class DcPackageReader
}//end namespace
