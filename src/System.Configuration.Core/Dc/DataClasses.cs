using System.Collections;
using System.Collections.Generic;
using System.Configuration.Core.Metadata;
using System.Linq;

namespace System.Configuration.Core.Dc {

    internal abstract class ObjectGeneratorBase<T,TValue> : IEnumerable<TValue> where T : class {
        private Dictionary<T, TValue> _dict;

        public ObjectGeneratorBase() {
            _dict = new Dictionary<T, TValue>();
        }

        protected Dictionary<T, TValue> Dictionary {
            get { return _dict; }
        }

        public IEnumerator<TValue> GetEnumerator() {
            return _dict.Values.GetEnumerator();
        }

        public TValue GetId(T data) {
            //Null 保留，使用0序号
            if (data == null) {
                return default(TValue);
            }

            TValue p;
            if (_dict.TryGetValue(data, out p)) {
                return p;
            }

            p = this.CreateValue(data);
            _dict.Add(data, p);

            return p;
        }

        protected abstract TValue CreateValue(T data);

        IEnumerator IEnumerable.GetEnumerator() {
            return _dict.Values.GetEnumerator();
        }
    }

    internal sealed class ObjectIDGenerator<T> : ObjectGeneratorBase<T,int> where T:class {
        private int _maxOrder;

        public int Count { get { return Dictionary.Count; } }

        protected override int CreateValue(T data) {
            _maxOrder++;
            return _maxOrder;
        }

        public T[] GetItems() {
            return (from p in base.Dictionary orderby p.Value select p.Key).ToArray();
        }
    }

    internal sealed class ObjectTypeIDGenerator : ObjectGeneratorBase<IType, ObjectTypeData> {
        private int _maxOrder;

        protected override ObjectTypeData CreateValue(IType data) {
            _maxOrder++;
            return new ObjectTypeData(data) { Order = _maxOrder };
        }

        public ObjectTypeData[] GetItems() {
            return (from p in base.Dictionary orderby p.Value.Order select p.Value).ToArray();
        }
    }

    internal sealed class WriterPartDataCollection : IEnumerable<PartData> {
        private int _maxOrder;
        private Dictionary<FullName, PartData> _dict = new Dictionary<FullName, PartData>();

        public PartData Register(FullName fullName, ConfigurationObjectPart part) {
            PartData partData;
            if (!_dict.TryGetValue(fullName,out partData)) {
                _maxOrder++; //注意part是从1计数的 ，虽然也可以从0计数，但考虑到与String的设计一致。
                partData = new PartData(fullName, part) { Order = _maxOrder };
                _dict.Add(fullName, partData);                
            }
            
            return partData;
        }

        public int GetId(FullName fullName) {
            return _dict[fullName].Order;
        }

        public PartData[] GetItems() {
            return (from p in _dict orderby p.Value.Order select p.Value).ToArray();
        }

        public IEnumerator<PartData> GetEnumerator() {
            return _dict.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _dict.Values.GetEnumerator();
        }
    }

    internal sealed class PackageWriteContext {
        public PackageWriteContext(Package sourcePackage) {
            this._stringDict = new ObjectIDGenerator<string>();
            this._typeDict = new ObjectTypeIDGenerator();
            this._sourcePackage = sourcePackage;
        }

        //存储所有字符串的字典
        ObjectIDGenerator<string> _stringDict;

        //存储所有使用到的类型对象
        ObjectTypeIDGenerator _typeDict;
        private readonly Package _sourcePackage;

        internal ObjectIDGenerator<string> StringDict {
            get { return _stringDict; }
        }

        internal ObjectTypeIDGenerator TypeDict {
            get { return _typeDict; }
        }

        public Package SourcePackage {
            get { return _sourcePackage; }
        }
    }

    internal sealed class PartData {
        public PartData(FullName key, ConfigurationObjectPart value) {
            this.FullName = key;
            this.Part = value;
        }
        public ConfigurationObjectPart Part { get; set; }
        
        public FullName FullName { get; set; }

        public int Order { get; set; }

        public byte[] Data { get; set; }
    }

    internal sealed class ObjectTypeData {
        public ObjectTypeData(IType type) {
            this.Type = type;
        }
        public IType Type { get; set; }
        public int Order { get; set; }


        private ObjectIDGenerator<IProperty> _properties;
        /// <summary>
        /// 所有用到的属性清单，不是所有属性哦。
        /// </summary>
        public ObjectIDGenerator<IProperty> Properties {
            get {
                if (_properties == null) {
                    _properties = new ObjectIDGenerator<IProperty>();
                }
                return _properties;
            }
        }

    }
}
