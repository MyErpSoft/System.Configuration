using System.Configuration.Core.Metadata;

namespace System.Configuration.Core {
    /// <summary>
    /// 一个实际可以使用的配置对象。
    /// </summary>
    public class ConfigurationObject
    {
        /// <summary>
        /// 创建一个新的配置对象。
        /// </summary>
        /// <param name="workspace">对象的工作空间。</param>
        /// <param name="name">要创建对象的键。</param>
        /// <param name="part">配置需要的零件信息</param>
        public ConfigurationObject(ConfigurationWorkspace workspace, QualifiedName name, ConfigurationObjectPart part) {
            this._workspace = workspace;
            this._name = name;
            this._part = part;
        }

        private readonly ConfigurationWorkspace _workspace;
        /// <summary>
        /// 返回对象的工作空间。
        /// </summary>
        public ConfigurationWorkspace Workspace {
            get { return _workspace; }
        }

        private readonly QualifiedName _name;
        /// <summary>
        /// 要创建对象的键。
        /// </summary>
        public QualifiedName Name {
            get { return _name; }
        }

        private ConfigurationObjectPart _part;
        /// <summary>
        /// 配置需要的零件信息
        /// </summary>
        public ConfigurationObjectPart Part {
            get { return _part; }
        }

        public virtual object GetValue(IProperty property) {
            object value;
            if (_part.TryGetLocaleValue(property, out value)) {
                return value;
            }

            //从基类搜索
            var baseObj = this.Base;
            if (baseObj != null) {
                return baseObj.GetValue(property);
            }
            else {
                return property.DefaultValue;
            }
        }

        private ConfigurationObject _base;
        public ConfigurationObject Base {
            get {
                if (_base == null) {
                    var baseName = _part.Base.Name;
                    if ((object)baseName == null) {
                        return null; //没有办法给_base加入已缓存标志。
                    }

                    lock (this) {
                        if (baseName != null) {
                            _base = _workspace.GetConfigurationObject(baseName);
                        }
                    }
                }

                return _base;
            }
        }
    }
}
