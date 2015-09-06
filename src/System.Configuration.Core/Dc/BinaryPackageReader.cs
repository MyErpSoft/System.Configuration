using System.Collections.Generic;
using System.Configuration.Core.Collections;
using System.Configuration.Core.Metadata;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace System.Configuration.Core.Dc {

    internal sealed class BinaryPackageReader : BinaryReader {

        public BinaryPackageReader(Stream input, DcPackage sourcePackage, PackageReadContext ctx) : base(input) {
            if (sourcePackage == null) {
                Utilities.ThrowArgumentNull(nameof(sourcePackage));
            }
            if (ctx == null) {
                Utilities.ThrowArgumentNull(nameof(ctx));
            }

            this._sourcePackage = sourcePackage;
            this._readContext = ctx;
        }

        private DcPackage _sourcePackage;
        private PackageReadContext _readContext;
        internal KeyValuePair<FullName, ConfigurationObjectPart>[] _allPairs;

        public KeyValuePair<FullName, ConfigurationObjectPart>[] ReadPackage() {

            try {
                //1 
                this.ReadFileHead();

                //2
                this.ReadStrings();

                //3
                _allPairs = this.ReadObjectIds();

                //至此，当前线程的读取任务完成，可以返回了。我们将在另外一个线程下继续读取数据。
                Thread readOtherThread = new Thread(this.ReadOther);
                readOtherThread.Name = "Read Package " + _sourcePackage.Name;
                readOtherThread.Start();

                return _allPairs;
            }
            catch (EndOfStreamException) {
                Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                            "不是有效的dc文件格式。", _sourcePackage.Name));
                return null;
            }
        }

        private void ReadOther() {
            try {
                //4
                this.ReadStrings();

                //5
                this.ReadObjectTypes();

                //6
                this.ReadObjects();
            }
            catch (Exception ex) {
                _sourcePackage.ReadDataException = ex;
                //当出现异常时，由于是后台线程，如果立即抛出异常，
                // * 可能没有人捕获，造成程序崩溃；
                // * 造成DcPart.OpenData永远等待或超时。
                // 所以这里将数据填充一个空的。
                byte[] emptyData = new byte[0];
                foreach (var item in _allPairs) {
                    DcPart part = (DcPart)item.Value;
                    if (part._data == null) {
                        part._data = emptyData;
                    }
                }

                //异常将被OpenDataCore抛出。
            }
            finally {
                this.BaseStream.Close();
                this.Close();
            }

            ////后台线程仍然继续，将数据解包。
            //try {
            //    foreach (var item in _allPairs) {
            //        item.Value.OpenData();怎么办？没有上下文需要的Binder
            //    }
            //}
            //catch(Exception ex) {
            //    Trace.Write(ex.Message);
            //    //任何数据的解包失败，都将造成此线程的停止，但不会出现异常。
            //    //由于IsOpened未设置为true，所以在首次使用时仍然会出现异常。
            //}
        }

        private void ReadObjectTypes() {
            var count = this.Read7BitEncodedInt();

            for (int i = 0; i < count; i++) {
                var typeData = new ObjectTypeReadData() {
                    Name = ReadObjectTypeQualifiedName()
                };

                var propertyCount = this.Read7BitEncodedInt();
                for (int j = 0; j < propertyCount; j++) {
                    typeData.Properties.Add(
                        new ObjectPropertyReadData() {
                            //这里Name是属性的名称，注意有个特殊的属性，即Base
                            Name = _readContext.Strings[this.Read7BitEncodedInt()]
                        });
                }

                _readContext.Types.Add(typeData);
            }
        }

        private ObjectTypeQualifiedName ReadObjectTypeQualifiedName() {
            string objNamespace, name, packageName, providerName;
            if (ReadQualifiedName(out objNamespace, out name, out packageName)) {
                providerName = this._readContext.Strings[this.Read7BitEncodedInt()];
                return new ObjectTypeQualifiedName(providerName, objNamespace, name, packageName);
            }
            else {
                return null;
            }
        }

        private bool ReadQualifiedName(out string objNamespace, out string name, out string packageName) {
            if (IsNull()) {
                objNamespace = null;
                name = null;
                packageName = null;
                return false;
            }
            else {
                objNamespace = this._readContext.Strings[this.Read7BitEncodedInt()];
                name = this._readContext.Strings[this.Read7BitEncodedInt()];
                packageName = this._readContext.Strings[this.Read7BitEncodedInt()];
                return true;
            }
        }

        private bool IsNull() {
            var isNull = this.ReadChar();
            return (isNull == 0);
        }

        private void ReadObjects() {
            var count = _allPairs.Length;
            int dataSize;
            DcPart part;

            for (int i = 0; i < count; i++) {
                dataSize = this.ReadInt32();
                part = (DcPart)_allPairs[i].Value;
                //注意在OpenData时判断_data是否为null
                part._data = this.ReadBytes(dataSize);
            }
        }

        private void ReadFileHead() {
            foreach (var item in BinaryPackageWriter.fileHead) {
                var currentChar = this.ReadChar();
                if (currentChar != item) {
                    Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                        "不是有效的dc文件格式。", _sourcePackage.Name));
                }
            }
        }

        private void ReadStrings() {
            var count = this.Read7BitEncodedInt();
            var strs = _readContext.Strings;

            for (int i = 0; i < count; i++) {
                strs.Add(string.Intern(this.ReadString()));
            }
        }

        private KeyValuePair<FullName, ConfigurationObjectPart>[] ReadObjectIds() {
            var count = this.Read7BitEncodedInt();
            string objNamespace;
            string name;
            string packageName = _sourcePackage.Name;
            var strings = _readContext.Strings;
            KeyValuePair<FullName, ConfigurationObjectPart>[] pairs = new KeyValuePair<FullName, ConfigurationObjectPart>[count];

            for (int i = 0; i < count; i++) {
                objNamespace = strings[this.Read7BitEncodedInt()];
                name = strings[this.Read7BitEncodedInt()];
                pairs[i] = new KeyValuePair<FullName, ConfigurationObjectPart>(
                    new FullName(objNamespace, name),
                    new DcPart(_sourcePackage));
            }

            return pairs;
        }


        internal ObjectTypeReadData ReadTypeData() {
            return _readContext.Types[this.Read7BitEncodedInt()];
        }

        internal ObjectPtr ReadObjectPtr() {
            string objNamespace, name, packageName;
            if (ReadQualifiedName(out objNamespace, out name, out packageName)) {
                return new ObjectPtr(new QualifiedName(objNamespace, name, packageName));
            }
            else {
                return ObjectPtr.None;
            }
        }

        internal IEnumerable<KeyValuePair<IProperty,object>> ReadProperties(ObjectTypeReadData data) {
            var count = this.Read7BitEncodedInt();

            //
            for (int i = 0; i < count; i++) {
                var propertyData = data.Properties[i];
                //property
                if (propertyData.Property == null) {
                    if (propertyData.Name == ConfigurationObjectPart.BasePropertyInstance.Name) {
                        propertyData.Property = ConfigurationObjectPart.BasePropertyInstance;
                    }
                    else {
                        propertyData.Property = data.Type.GetProperty(propertyData.Name);
                    }
                }
                
                yield return new KeyValuePair<IProperty, object>(propertyData.Property, ReadValue(propertyData.Property.PropertyType));
            }
        }

        //0 Null, 1 简单值 ，2 指针 ，3 IEnumerable<ListDifferenceItem>集合
        private object ReadValue(IType valueType) {
            var flag = this.ReadChar();
            switch (flag) {
                case (char)0:
                    return null;
                case (char)1:
                    var converter = valueType.GetConverter();
                    var str = this.ReadString();
                    return converter.ConvertFromInvariantString(str);
                case (char)2:
                    return this.ReadObjectPtr();
                case (char)3:
                    {
                        var count = this.Read7BitEncodedInt();
                        OnlyNextList<ListDifferenceItem> list = new OnlyNextList<ListDifferenceItem>();
                        for (int i = 0; i < count; i++) {
                            var action = this.ReadChar();
                            list.Add(new ListDifferenceItem(action == 0 ? ListDifferenceAction.Add : ListDifferenceAction.Remove,
                                this.ReadValue(null)));
                        }
                        return list;
                    }
                default:
                    Utilities.ThrowArgumentException("数据格式不正确，不能识别的flag", "flag");
                    break;
            }

            return null;
        }
    }
}
