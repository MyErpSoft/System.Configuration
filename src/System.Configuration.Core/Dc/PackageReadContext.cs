using System;
using System.Collections.Generic;
using System.Configuration.Core.Metadata;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Core.Dc {

    /// <summary>
    /// 为读取Package提供上下文数据
    /// </summary>
    internal sealed class PackageReadContext {

        public PackageReadContext() {
            this._strings = new List<string>();
            this.Types = new List<ObjectTypeReadData>();

            //List<T>的索引是从0开始的，而我们的字符串索引是从1开始的，0预留给null.
            this._strings.Add(null);
            this.Types.Add(null);
        }

        private List<string> _strings;
        /// <summary>
        /// 所有可用的字符串清单
        /// </summary>
        public List<string> Strings {
            get { return _strings; }
        }

        public List<ObjectTypeReadData> Types { get; private set; }
    }

    internal sealed class ObjectTypeReadData {
        public ObjectTypeReadData() {
            this.Properties = new List<ObjectPropertyReadData>();
            this.Properties.Add(null);
        }

        public IType Type { get; set; }
        public ObjectTypeQualifiedName Name { get; set; }

        public List<ObjectPropertyReadData> Properties { get; }
    }

    internal sealed class ObjectPropertyReadData {
        public string Name { get; set; }
        public IProperty Property { get; set; }
    }
}
