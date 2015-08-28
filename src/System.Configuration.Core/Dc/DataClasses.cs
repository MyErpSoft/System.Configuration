using System;
using System.Collections.Generic;
using System.Configuration.Core.Metadata;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Core.Dc {

    internal abstract class ObjectGeneratorBase<T,TValue> where T : class {
        private Dictionary<T, TValue> _dict;

        public ObjectGeneratorBase() {
            _dict = new Dictionary<T, TValue>();
        }

        protected Dictionary<T, TValue> Dictionary {
            get { return _dict; }
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
    }

    internal sealed class ObjectIDGenerator<T> : ObjectGeneratorBase<T,int> where T:class {
        private int _maxOrder;
        
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

    internal sealed class PackageWriteContext {
        public PackageWriteContext() {
            this._stringDict = new ObjectIDGenerator<string>();
            this._typeDict = new ObjectTypeIDGenerator();
        }

        //存储所有字符串的字典
        ObjectIDGenerator<string> _stringDict;

        //存储所有使用到的类型对象
        ObjectTypeIDGenerator _typeDict;

        internal ObjectIDGenerator<string> StringDict {
            get { return _stringDict; }
        }

        internal ObjectTypeIDGenerator TypeDict {
            get { return _typeDict; }
        }
    }

    internal sealed class PartData {
        public PartData(FullName key, ConfigurationObjectPart value, PackageWriteContext ctx) {
            var stringDict = ctx.StringDict;

            Namespace = stringDict.GetId(key.Namespace);
            Name = stringDict.GetId(key.Name);

            this.Part = value;
        }
        public ConfigurationObjectPart Part { get; set; }
        
        public int Namespace { get; set; }
        public int Name { get; set; }
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
