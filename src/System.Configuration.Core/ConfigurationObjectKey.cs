using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Configuration.Core {

    /// <summary>
    /// 表示一个配置对象的识别键，包含所在的组装件、命名空间和名称。
    /// </summary>
    public class ConfigurationObjectKey {

        private string _package;
        public string PackageName {
            get { return _package; }
            set { _package = value; }
        }

        private string _namespace;

        public string Namespace {
            get { return _namespace; }
            set { _namespace = value; }
        }

        private string _name;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public override string ToString() {
            return (string.IsNullOrEmpty(this.Namespace) ? this.Name : this.Namespace + "." + this.Name) +
                "," + this.PackageName;
        }
    }
}
