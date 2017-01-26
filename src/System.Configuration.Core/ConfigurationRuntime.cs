using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration.Core.Metadata;
using System.Threading;

namespace System.Configuration.Core {

    /// <summary>
    /// 描述Repository和Package在加载和工作时，需要的信息。例如类型绑定信息。
    /// </summary>
    public class ConfigurationRuntime {

        private static ConfigurationRuntime _defaultRuntime = new ConfigurationRuntime();
        /// <summary>
        /// 返回缺省的运行时对象。
        /// </summary>
        public static ConfigurationRuntime Default {
            get { return _defaultRuntime; }
            set { _defaultRuntime = value; }
        }

        /// <summary>
        /// 创建ConfigurationRuntime 实例并使用默认的绑定器和提供者。
        /// </summary>
        public ConfigurationRuntime() {
            this._binders = new Core.ConfigurationRuntime.ConfigurationObjectBinderCollection(this,
                new ClrConfigurationObjectBinder(),
                new ClrInterfaceConfigurationObjectBinder()
                );

            this._providers = new PackageProviderCollection(this,
                new Dcxml.DcxmlPackageProvider(),
                new Dc.DcPackageProvider()
            );
        }

        private readonly ConfigurationObjectBinderCollection _binders;
        /// <summary>
        /// 返回类型绑定器，用于将字符串描述的名称转换为实际的对象类型。
        /// </summary>
        public IConfigurationObjectBinderCollection Binders {
            get { return _binders; }
        }

        //内部使用，数组循环更快，但不安全。
        internal IPackageProvider[] _providerArray;
        private readonly PackageProviderCollection _providers;
        /// <summary>
        /// 返回当前所有可用的PackageProvider，由于搜索Package,默认初始化了内置的Dcxml和Dc两种格式。
        /// </summary>
        public IPackageProviderCollection PackageProviders {
            get { return _providers; }
        }

        #region Collections
        /// <summary>
        /// 访问所有IPackageProvider的接口。
        /// </summary>
        public interface IPackageProviderCollection : ICollection<IPackageProvider> { }

        /// <summary>
        /// 访问所有IConfigurationObjectBinder的接口。
        /// </summary>
        public interface IConfigurationObjectBinderCollection : ICollection<ConfigurationObjectBinder> {
            
            /// <summary>
            /// 提供类型的唯一名称，然后查询对象的类型。
            /// </summary>
            /// <param name="name">要检索的唯一名称。</param>
            /// <returns>对应的唯一名称。</returns>
            IType BindToType(ObjectTypeQualifiedName name);

            /// <summary>
            /// 返回指定名称的Binder。
            /// </summary>
            /// <param name="providerName">provider的识别名称。</param>
            /// <param name="binder">如果找到将返回实例，否则返回null.</param>
            /// <returns>如果找到将返回true，否则返回false.</returns>
            bool TryGetBinder(string providerName, out ConfigurationObjectBinder binder);
        }

        private sealed class PackageProviderCollection : ItemCollection<IPackageProvider>,IPackageProviderCollection {
            public PackageProviderCollection(ConfigurationRuntime runtime, params IPackageProvider[] providers)
                :base(runtime,providers){
                runtime._providerArray = providers;
            }

            protected override void Update(ConfigurationRuntime runtime, IPackageProvider[] items) {
                Interlocked.Exchange(ref runtime._providerArray, items);
            }
        }

        private sealed class ConfigurationObjectBinderCollection : ItemCollection<ConfigurationObjectBinder>, IConfigurationObjectBinderCollection {
            public ConfigurationObjectBinderCollection(ConfigurationRuntime runtime, params ConfigurationObjectBinder[] items)
                : base(runtime, items) {
                this._binderArray = items;
                this._cachedTypes = new ObjectContainer<ObjectTypeQualifiedName, IType>(this.LoadType);
            }

            protected override void Update(ConfigurationRuntime runtime, ConfigurationObjectBinder[] items) {
                Interlocked.Exchange(ref _binderArray, items); //?有必要吗?
            }

            #region BindToType
            private ConfigurationObjectBinder[] _binderArray;
            private readonly ObjectContainer<ObjectTypeQualifiedName, IType> _cachedTypes;

            /// <summary>
            /// 提供类型的唯一名称，然后查询对象的类型。
            /// </summary>
            /// <param name="name">要检索的唯一名称。</param>
            /// <returns>对应的唯一名称。</returns>
            public IType BindToType(ObjectTypeQualifiedName name) {
                if (name == null) {
                    Utilities.ThrowArgumentNull(nameof(name));
                }

                //在缓存中寻找，
                return this._cachedTypes.GetValue(name);
            }

            private IType LoadType(ObjectTypeQualifiedName name) {
                IType result;
                var binders = _binderArray;
                foreach (var item in binders) {
                    if (item.Name == name.ProviderName) {
                        if (item.TryBindToType(name, out result)) {
                            return result;
                        }
                        Utilities.ThrowApplicationException("未能识别的配置对象类型，检查providerName、namespace、packageName、Name是否正确");
                    }
                }

                Utilities.ThrowApplicationException("未能识别的配置对象类型，检查providerName是否正确");
                return null;
            }

            /// <summary>
            /// 返回指定名称的Binder。
            /// </summary>
            /// <param name="providerName">provider的识别名称。</param>
            /// <param name="binder">如果找到将返回实例，否则返回null.</param>
            /// <returns>如果找到将返回true，否则返回false.</returns>
            public bool TryGetBinder(string providerName,out ConfigurationObjectBinder binder) {
                var binders = _binderArray;
                foreach (var item in binders) {
                    if (item.Name == providerName) {
                        binder = item;
                        return true;
                    }
                }

                binder = null;
                return false;
            }
            #endregion
        }

        /// <summary>
        /// 处理runtime并发问题的代码，外界修改项同步更新
        /// </summary>
        private abstract class ItemCollection<T> : Collection<T> {
            private readonly ConfigurationRuntime _runtime;

            public ItemCollection(ConfigurationRuntime runtime, params T[] providers) {
                this._runtime = runtime;
                foreach (var item in providers) {
                    Items.Add(item); 
                }
            }

            protected override void InsertItem(int index, T item) {
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

            protected override void SetItem(int index, T item) {
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
                T[] newArray = new T[this.Count];
                this.CopyTo(newArray, 0);
                this.Update(_runtime, newArray);
            }

            protected abstract void Update(ConfigurationRuntime runtime, T[] items);
        } 
        #endregion
    }
}
