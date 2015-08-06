using System.Collections.ObjectModel;

namespace System.Configuration.Core {

    /// <summary>
    /// 配置工作区，通过此获取配置对象。
    /// </summary>
    public class ConfigurationWorkspace {

        /// <summary>
        /// 创建工作区实例，他用于创建具体的配置对象。
        /// </summary>
        /// <param name="repository">此工作区工作时依赖的仓库。</param>
        public ConfigurationWorkspace(Repository repository) {
            if (repository == null) {
                Utilities.ThrowArgumentNull(nameof(repository));
            }

            this._objectContainer = new ObjectContainer<ConfigurationObjectKey, ConfigurationObject>(this.LoadObject);
            this._repository = repository;
        }

        private readonly Repository _repository;
        /// <summary>
        /// 返回当前工作区依赖的仓库。
        /// </summary>
        public Repository Repository {
            get { return _repository; }
        }

        private readonly ObjectContainer<ConfigurationObjectKey, ConfigurationObject> _objectContainer;
        /// <summary>
        /// 检索一个指定键的对象，如果对象尚未创建将自动创建一个，容器保证同一个key返回相同的实例。
        /// </summary>
        /// <param name="key">要查询的对象标识</param>
        /// <returns>此键关联的一个对象</returns>
        public ConfigurationObject GetObject(ConfigurationObjectKey key) {
            if (key == null) {
                Utilities.ThrowArgumentNull(nameof(key));
            }

            //尝试在ObjectContainer检查是否存在缓存，如果有直接返回，否则ObjectContainer的回调将
            //调用LoadObject方法。
            return this._objectContainer.GetValue(key);
        }

        private ConfigurationObject LoadObject(ConfigurationObjectKey key) {
            //从仓库获取Package，当出现多个包时仓库会包装成一个虚拟的包
            var package = this._repository.GetPackage(key.PackageName);

            //从Package中检索此key对应的配置数据
            ConfigurationObjectPart part;
            if (package.TryGetPart(key.Namespace, key.Name, out part)) {
                Utilities.ThrowApplicationException(string.Format(Globalization.CultureInfo.CurrentCulture,
                    "未能找到指定的配置对象：{0}", key));
            }

            //组装成新的配置对象。
            return new ConfigurationObject(part);
        }

    }
}
