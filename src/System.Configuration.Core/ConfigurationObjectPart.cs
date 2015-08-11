using System.Collections.Generic;
using System.Data.Metadata.DataEntities;

namespace System.Configuration.Core {

    public abstract class ConfigurationObjectPart {

        #region TryGetValue
        //todo:此实现应该放到 配置对象存储区 实现，因为base需要延迟的加载，且不能缓存到当前对象中。
        public bool TryGetValue(IEntityProperty property, out object value) {
            //首先在本地搜索数据，如果找到直接返回；
            //否则，看是否包含Base引用，如果有，从Base获取。
            if (TryGetLocaleValue(property, out value)) {
                return true;
            }

            var basePart = this.Base;
            if (basePart != null) {
                //注意Base可能还有Base
                return basePart.TryGetValue(property, out value);
            }

            return false;
        }

        /// <summary>
        /// 返回部件内部存储的值，如果内部有定义值将返回他。
        /// </summary>
        /// <param name="property">要检索的属性</param>
        /// <param name="value">如果检索到有效的定义将返回他，否则返回null</param>
        /// <returns>如果检索到有效的定义将返回true</returns>
        public abstract bool TryGetLocaleValue(IEntityProperty property, out object value);

        public ConfigurationObjectPart Base {
            get {
                object value;
                if (TryGetLocaleValue(BaseProperty.Default, out value)){
                    return value as ConfigurationObjectPart;
                }

                return null;
            }
        }

        private sealed class BaseProperty : IEntityProperty {
            public readonly static BaseProperty Default = new BaseProperty();
            public bool IsReadOnly {
                get { return true; }
            }

            public string Name {
                get { return "Base"; }
            }

            public IEntityType PropertyType {
                get { return null; }
            }

            public object GetValue(object entity) {
                throw new NotSupportedException();
            }

            public void ResetValue(object entity) {
                throw new NotSupportedException();
            }

            public void SetValue(object entity, object newValue) {
                throw new NotSupportedException();
            }
        }
        #endregion

        #region Open
        private bool _isOpened;

        internal void OpenData(GetPartContext ctx) {
            if (!_isOpened) {
                lock (this) {
                    if (!_isOpened) {
                        this.OpenDataCore(ctx);
                        _isOpened = true;
                    }
                }
            }
        }

        /// <summary>
        /// 返回此对象是否已经完成解开数据包工作。
        /// </summary>
        protected bool IsOpened {
            get {
                return this._isOpened;
            }
        }

        /// <summary>
        /// 派生类重载此方法，将原始的数据解开填充到当前数据包中。
        /// </summary>
        protected abstract void OpenDataCore(GetPartContext ctx);

        #endregion
    }
}
