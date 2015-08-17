using System.Configuration.Core.Metadata;
using System.Globalization;

namespace System.Configuration.Core {

    /// <summary>
    /// 配置工作区，通过此获取配置对象。
    /// </summary>
    public class ConfigurationWorkspace {

        /// <summary>
        /// 创建工作区实例，他用于创建具体的配置对象。
        /// </summary>
        /// <param name="repository">此工作区工作时依赖的仓库。</param>
        public ConfigurationWorkspace(Repository repository) : this(repository, new ConfigurationObjectBinder()) {
        }

        /// <summary>
        /// 创建工作区实例，他用于创建具体的配置对象。
        /// </summary>
        /// <param name="repository">此工作区工作时依赖的仓库。</param>
        /// <param name="binder">类型绑定器，用于将配置部件转换为配置的动态实体。</param>
        public ConfigurationWorkspace(Repository repository,ConfigurationObjectBinder binder) {
            if (repository == null) {
                Utilities.ThrowArgumentNull(nameof(repository));
            }
            if (binder == null) {
                Utilities.ThrowArgumentNull(nameof(binder));
            }

            this._objectContainer = new ObjectContainer<QualifiedName, ObjectPart>(this.LoadObject);
            this._repository = repository;
            this._binder = binder;
        }

        private readonly Repository _repository;
        /// <summary>
        /// 返回当前工作区依赖的仓库。
        /// </summary>
        public Repository Repository {
            get { return _repository; }
        }

        private readonly ConfigurationObjectBinder _binder;
        /// <summary>
        /// 返回类型绑定器，用于将配置部件转换为配置的动态实体。
        /// </summary>
        public ConfigurationObjectBinder Binder {
            get { return _binder; }
        }

        private readonly ObjectContainer<QualifiedName, ObjectPart> _objectContainer;
        /// <summary>
        /// 检索一个指定键的对象，如果对象尚未创建将自动创建一个，容器保证同一个key返回相同的实例。
        /// </summary>
        /// <param name="key">要查询的对象标识</param>
        /// <returns>此键关联的一个配置对象</returns>
        public ConfigurationObject GetConfigurationObject(QualifiedName key) {
            if ((object)key == null) {
                return null;
            }

            //尝试在ObjectContainer检查是否存在缓存，如果有直接返回，否则ObjectContainer的回调将
            //调用LoadObject方法。
            return this._objectContainer.GetValue(key).ConfigObject;
        }

        /// <summary>
        /// 检索一个指定键的对象，如果对象尚未创建将自动创建一个，容器保证同一个key返回相同的实例。
        /// </summary>
        /// <param name="key">要查询的对象标识</param>
        /// <returns>此键关联的一个CLR对象</returns>
        public object GetObject(QualifiedName key) {
            if ((object)key == null) {
                return null;
            }

            return this._objectContainer.GetValue(key).GetClrObject(this);
        }

        private ObjectPart LoadObject(QualifiedName key) {
            //从仓库获取Package，当出现多个包时仓库会包装成一个虚拟的包
            var package = this._repository.GetPackage(key.PackageName);

            //从Package中检索此key对应的配置数据
            ConfigurationObjectPart part;
            if (!package.TryGetPart(key.FullName, out part)) {
                Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                    "未能找到指定的配置对象：{0}", key));
            }

            //解开数据
            if (!part.IsOpened) {
                var ctx = new OpenDataContext(this._binder, key);
                part.OpenData(ctx);
            }

            return new ObjectPart(new ConfigurationObject(this, key, part));
        }

        private sealed class ObjectPart {
            public ObjectPart(ConfigurationObject cobj) {
                this.ConfigObject = cobj;
            }

            public object ClrObject;
            public readonly ConfigurationObject ConfigObject;

            public object GetClrObject(ConfigurationWorkspace workspace) {
                if (ClrObject != null) {
                    return this.ClrObject;
                }

                lock (this) {
                    if (ClrObject == null) {
                        this.ClrObject = workspace.Binder.CreateObject(this.ConfigObject);
                    }

                    return this.ClrObject;
                }
            }
        }
    }
}
