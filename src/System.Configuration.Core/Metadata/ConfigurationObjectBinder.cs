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

        public readonly static string ClrBinderProviderName = "clr-namespace";
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
                Type dt = Type.GetType(name.ToString(), true);
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
        /// <param name="key">要创建对象的键。</param>
        /// <param name="part">关联的部件。</param>
        /// <returns>一个新的配置对象。</returns>
        public virtual object CreateObject(QualifiedName key, ConfigurationObjectPart part) {
            //组装成新的配置对象。
            return new ConfigurationObject(part);
        }
        #endregion
    }
}
