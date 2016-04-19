using System.Collections.ObjectModel;
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
            this._providers = new PackageProviderCollection(this,
                new Dcxml.DcxmlPackageProvider(),
                new Dc.DcPackageProvider()
            );
        }

        private readonly ConfigurationObjectBinder _binder;
        /// <summary>
        /// 返回类型绑定器，用于将配置部件转换为配置的动态实体。
        /// </summary>
        public ConfigurationObjectBinder Binder {
            get { return _binder; }
        }

        private IPackageProvider[] _packageProviders;
        //内部使用，数组循环更快，但不安全。
        internal IPackageProvider[] GetProviders() {
            return _packageProviders;
        }

        private readonly PackageProviderCollection _providers;
        /// <summary>
        /// 返回当前所有可用的PackageProvider，由于搜索Package,默认初始化了内置的Dcxml和Dc两种格式。
        /// </summary>
        public Collection<IPackageProvider> PackageProviders {
            get { return _providers; }
        }
        #region PackageProviderCollection
        /// <summary>
        /// 处理runtime并发问题的代码，外界修改provider
        /// </summary>
        private sealed class PackageProviderCollection : Collection<IPackageProvider> {
            private readonly ConfigurationRuntime _runtime;

            public PackageProviderCollection(ConfigurationRuntime runtime, params IPackageProvider[] providers) {
                this._runtime = runtime;
                runtime._packageProviders = providers;
                foreach (var item in providers) {
                    Items.Add(item);
                }
            }

            protected override void InsertItem(int index, IPackageProvider item) {
                lock (this) {
                    base.InsertItem(index, item);
                    this.Update();
                }
            }

            protected override void RemoveItem(int index) {
                lock (this) {
                    base.RemoveItem(index);
                    this.Update();
                }
            }

            protected override void SetItem(int index, IPackageProvider item) {
                lock (this) {
                    base.SetItem(index, item);
                    this.Update();
                }
            }

            protected override void ClearItems() {
                lock (this) {
                    base.ClearItems();
                    this.Update();
                }
            }

            private void Update() {
                IPackageProvider[] newArray = new IPackageProvider[this.Count];
                this.CopyTo(newArray, 0);
                _runtime._packageProviders = newArray;
            }
        } 
        #endregion
    }
}
