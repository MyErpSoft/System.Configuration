using System.Configuration.Core.Metadata;

namespace System.Configuration.Core {

    /// <summary>
    /// 描述Repository和Package在加载和工作时，需要的信息。例如类型绑定信息。
    /// </summary>
    public class ConfigurationRuntime {

        /// <summary>
        /// 创建ConfigurationRuntime 实例。
        /// </summary>
        public ConfigurationRuntime():
            this(new ConfigurationObjectBinder()) {
        }

        /// <summary>
        /// 创建ConfigurationRuntime 实例并指定绑定器。
        /// </summary>
        /// <param name="binder">类型绑定器，用于将配置部件转换为配置的动态实体。</param>
        public ConfigurationRuntime(ConfigurationObjectBinder binder) {
            if (binder == null) {
                Utilities.ThrowArgumentNull(nameof(binder));
            }
            this._binder = binder;
        }

        private readonly ConfigurationObjectBinder _binder;
        /// <summary>
        /// 返回类型绑定器，用于将配置部件转换为配置的动态实体。
        /// </summary>
        public ConfigurationObjectBinder Binder {
            get { return _binder; }
        }
    }
}
