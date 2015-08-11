using System.Data.Metadata.DataEntities.Dynamic;

namespace System.Configuration.Core {

    /// <summary>
    /// 默认的配置对象绑定器。
    /// </summary>
    public class ConfigurationObjectBinder {

        public ConfigurationObjectBinder() {
            this._cachedTypes = new ObjectContainer<ObjectTypeQualifiedName, DynamicEntityType>(this.LoadType);
        }

        private readonly ObjectContainer<ObjectTypeQualifiedName, DynamicEntityType> _cachedTypes;

        public DynamicEntityType BindToType(ObjectTypeQualifiedName name) {
            //在缓存中寻找，
            return this._cachedTypes.GetValue(name);
        }

        private DynamicEntityType LoadType(ObjectTypeQualifiedName name) {
            DynamicEntityType result;
            if (!TryBindToTypeCore(name,out result)) {
                Utilities.ThrowApplicationException("未能识别的配置对象类型，检查providerName是否正确");
                result = null;
            }

            return result;
        }

        public readonly static string ClrBinderProviderName = "clr-namespace";
        /// <summary>
        /// 默认实现的clr-namespace的绑定器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual bool TryBindToTypeCore(ObjectTypeQualifiedName name, out DynamicEntityType type) {
            if (name == null) {
                Utilities.ThrowArgumentNull(nameof(name));
            }

            if (string.IsNullOrEmpty(name.ProviderName) || name.ProviderName == ClrBinderProviderName) {
                Type dt = Type.GetType(name.ToString(), true);
                type = (DynamicEntityType)Activator.CreateInstance(dt);
                return true;
            }

            type = null;
            return false;
        }
    }
}
