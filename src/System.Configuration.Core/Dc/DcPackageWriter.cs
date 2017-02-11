using System.Collections.Generic;
using System.IO;

namespace System.Configuration.Core.Dc {
    internal partial class DcPackageWriter {
        internal const string Version = "2.1";
        internal const string FileHead = "Difference Configuration File " + Version;

        public static void ConvertToDc(string filePath, Package sourcePackage) {
            using (var stream = PlatformUtilities.Current.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write)) {
                var writer = new DcPackageWriter(stream);
                writer.WriteParts(sourcePackage);
                stream.Flush();
            }
        }

        public DcPackageWriter(Stream output) {
            _writer = new DcBinaryWriter(output);
        }

        private readonly DcBinaryWriter _writer;

        public void WriteParts(Package sourcePackage) {
            if (sourcePackage == null) {
                Utilities.ThrowArgumentNull(nameof(sourcePackage));
            }
            this._stringDict = new ObjectIDGenerator<string>();
            this._typeDict = new ObjectTypeIDGenerator();

            this._sourcePackageName = sourcePackage.Name;
            this.WriteParts(sourcePackage.GetParts());
        }

        private ObjectIDGenerator<string> _stringDict;          //存储所有字符串的字典        
        private ObjectTypeIDGenerator _typeDict;                //存储所有使用到的类型对象
        private WriterPartDataCollection _partDatas;            //存储所有的Part对应的数据
        private string _sourcePackageName;

        private void WriteParts(IEnumerable<KeyValuePair<FullName, ConfigurationObjectPart>> parts) {
            ScanningParts(parts);
            ScanningObjectTypes();
            WritePartDatas();

            this.WriteFileHead();

            var strs = _stringDict.GetItems();
            WriteInternStrings(strs);

            WriteObjectTypes(strs);
            var partDataArray = _partDatas.GetItems();
            WriteObjectIds(partDataArray);
            WriteData(partDataArray);
        }
        
        private void ScanningParts(IEnumerable<KeyValuePair<FullName, ConfigurationObjectPart>> parts) {
            if (_partDatas != null) {
                Utilities.ThrowNotSupported("只能调用一次WriteParts方法。");
            }

            _partDatas = new WriterPartDataCollection();
            PartData partData;

            foreach (var pair in parts) {
                //为每个零件编号，以便后续的引用值指向当前库时，使用数组索引而不是完整的指针描述字符串。
                //参考：DcPartReader.ReadThisPackageObjectPtr()
                partData = _partDatas.Register(pair.Key, pair.Value);

                //将关键性的字符串登记，这些字符串在读取时需要Intern处理的。包括:

                // ObjectFullName的namespace，大多数命名空间相同，且能够在运行时查询对象时基于对象的查找。
                // 当前Package的对象是不描述PackageName的，所以没有必要登记。
                _stringDict.GetId(pair.Key.Namespace);

                //将零件的类型也登记，这样每个类型都有唯一的序号。也方便ScanningObjectTypeProviders直接循环用到的类型
                var typeData = _typeDict.GetId(pair.Value.Type);
                
                // 同理，实际值中如果存在对象指针，其namespace,packageName也基于对象的查找
                foreach (var valuePair in pair.Value.GetLocalValues()) {
                    //每个Type用到的属性也会编号
                    typeData.Properties.GetId(valuePair.Key);

                    if (!RegisterObjectPtr(valuePair.Value)) {
                        IEnumerable<ListDifferenceItem> list = valuePair.Value as IEnumerable<ListDifferenceItem>;
                        if (list != null) {
                            foreach (var diffItem in list) {
                                RegisterObjectPtr(diffItem.Item);
                            }
                        }
                    }
                }
            }

            foreach (var typeDataItem in _typeDict) {
                // 对象类型（IType）的ProviderName，例如：clr-namespace，
                //    但是IType上的其他属性不做此处理，因为读取时根据名称先转换为ClrType，此时用不到InternString的特性。                
                _stringDict.GetId(typeDataItem.Type.QualifiedName.ProviderName);
            }

            //记住InternString的大小，后面还要添加字符串，最后写入文件时容易处理。
            _internStringSize = _stringDict.Count;
        }//end ScanningParts

        private bool RegisterObjectPtr(object value) {
            if (value is ObjectPtr) {
                var ptr = (ObjectPtr)value;
                //本Package的对象指针不用处理，因为最终会转换为 本地对象指针，不用描述PackageName，namespace等。
                if (!string.Equals(ptr.Name.PackageName, _sourcePackageName, StringComparison.OrdinalIgnoreCase)) {
                    _stringDict.GetId(ptr.Name.PackageName);
                    _stringDict.GetId(ptr.Name.Namespace);
                }

                return true;
            }

            return false;
        }

        //将类型用到的字符串登记。没有在ScanningParts一并处理是因为ScanningParts扫描的是Intern处理的字符串。
        private void ScanningObjectTypes() {
            _objectTypeStringOffset = _stringDict.Count;

            foreach (var objectTypeData in _typeDict) {
                var objectTypeName = objectTypeData.Type.QualifiedName;

                //ProviderName已经作为InternString处理了。
                _stringDict.GetId(objectTypeName.Namespace);
                _stringDict.GetId(objectTypeName.Name);
                _stringDict.GetId(objectTypeName.PackageName);

                //注意是`用到`的属性清单，而不是完整的属性清单
                var properties = objectTypeData.Properties.GetItems(); 
                foreach (var property in properties) {
                    _stringDict.GetId(property.Name);
                }
            }

            _objectTypeStringSize = _stringDict.Count - _objectTypeStringOffset;
        }

        private void WritePartDatas() {
            foreach (var partData in _partDatas) {
                var partWriter = new DcPartWriter(this, partData);
                partWriter.WriteObjectType();
                partWriter.WriteProperties();
                partData.Data = partWriter.ToArray();

            }
        }

        //写入文件头
        private void WriteFileHead() {
            _writer.Write7BitInt(FileHead.Length);
            foreach (var item in FileHead) {
                _writer.Write(item);
            }

            //todo:写入文件的数据版本号和自定义元数据信息。
        }

        private int _internStringSize;                          //InternString处理的字符串大小。
        private void WriteInternStrings(string[] priorityStrings) {
            _writer.WriteStrings(priorityStrings, 0, _internStringSize);
        }
        
        private int _objectTypeStringOffset;
        private int _objectTypeStringSize;                      //ObjectTypeProviders用到的字符串大小
        private void WriteObjectTypes(string[] strs) {
            _writer.WriteStrings(strs, _objectTypeStringOffset, _objectTypeStringSize);

            var typeDatas = _typeDict.GetItems(); //按序号排列的数组。
            _writer.Write7BitInt(typeDatas.Length);

            foreach (var typeData in typeDatas) {

                var tName = typeData.Type.QualifiedName;
                _writer.Write7BitInt(_stringDict.GetId(tName.ProviderName));
                _writer.Write7BitInt(_stringDict.GetId(tName.Namespace));
                _writer.Write7BitInt(_stringDict.GetId(tName.Name));
                _writer.Write7BitInt(_stringDict.GetId(tName.PackageName));

                //使用到的属性清单。
                var properties = typeData.Properties.GetItems();
                _writer.Write7BitInt(properties.Length);
                foreach (var property in properties) {
                    _writer.Write7BitInt(_stringDict.GetId(property.Name));
                }
            }
        }
        
        private void WriteObjectIds(PartData[] partDataArray) {
            _writer.Write7BitInt(partDataArray.Length);
            int offset = 0;

            foreach (var partData in partDataArray) {
                _writer.Write7BitInt(_stringDict.GetId(partData.FullName.Namespace));
                _writer.Write(partData.FullName.Name);
                offset += partData.Data.Length;
                _writer.Write7BitInt(offset);
            }
        }

        private void WriteData(PartData[] partDataArray) {
            foreach (var partData in partDataArray) {
                _writer.Write(partData.Data, 0, partData.Data.Length);
            }            
        }
    }//end class
}//end namespace