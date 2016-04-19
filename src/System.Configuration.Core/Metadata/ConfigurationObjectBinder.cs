using System.Configuration.Core.Metadata.Clr;

namespace System.Configuration.Core.Metadata {

    /// <summary>
    /// 默认的配置对象绑定器。
    /// </summary>
    public class ConfigurationObjectBinder {

        public ConfigurationObjectBinder() {
            this._cachedTypes = new ObjectContainer<ObjectTypeQualifiedName, IType>(this.LoadType);
        }

        #region BindToType
        private readonly ObjectContainer<ObjectTypeQualifiedName, IType> _cachedTypes;

        public IType BindToType(ObjectTypeQualifiedName name) {
            //在缓存中寻找，
            return this._cachedTypes.GetValue(name);
        }

        private IType LoadType(ObjectTypeQualifiedName name) {
            IType result;
            if (!TryBindToTypeCore(name, out result)) {
                Utilities.ThrowApplicationException("未能识别的配置对象类型，检查providerName是否正确");
                result = null;
            }

            return result;
        }

        public const string ClrBinderProviderName = "clr-namespace";
        /// <summary>
        /// 默认实现的clr-namespace的绑定器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual bool TryBindToTypeCore(ObjectTypeQualifiedName name, out IType type) {
            if (name == null) {
                Utilities.ThrowArgumentNull(nameof(name));
            }

            if (string.IsNullOrEmpty(name.ProviderName) || name.ProviderName == ClrBinderProviderName) {
                //todo:安全性检测，不然配置文件可以任意创建一个对象并设置其属性值。
                Type dt = Type.GetType(name.QualifiedName.ToString(), true);
                type = (IType)Clr.ClrType.GetClrType(dt);
                return true;
            }

            type = null;
            return false;
        }
        #endregion

        #region 创建配置对象
        /// <summary>
        /// 允许派生类重载此方法，创建自己的配置对象。
        /// </summary>
        /// <param name="data">关联的配置对象。</param>
        /// <returns>一个新的配置对象。</returns>
        public virtual object CreateObject(ConfigurationObject data) {
            ClrType clrType = data.Part.Type as ClrType;
            if (clrType != null) {
                //组装成新的配置对象。
                return Activator.CreateInstance(clrType.MappingType, data);
            }

            Utilities.ThrowNotSupported("重载此方法用于支持新类型的创建。");
            return true;
        }
        #endregion
    }
}
