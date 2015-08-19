using System.Collections.Generic;
using System.Configuration.Core.Metadata;

namespace System.Configuration.Core {

    /// <summary>
    /// 在Package中检索到的配置部件，存储了单个对象的属性值信息。
    /// </summary>
    public abstract class ConfigurationObjectPart {

        #region TryGetLocaleValue Base

        /// <summary>
        /// 返回部件内部存储的值，如果内部有定义值将返回他。
        /// </summary>
        /// <param name="property">要检索的属性</param>
        /// <param name="value">如果检索到有效的定义将返回他，否则返回null</param>
        /// <returns>如果检索到有效的定义将返回true</returns>
        public abstract bool TryGetLocalValue(IProperty property, out object value);

        /// <summary>
        /// 返回部件内部存储的集合属性值。
        /// </summary>
        /// <param name="property">要检索的属性</param>
        /// <param name="list">如果检索到有效的定义将返回他，否则返回null</param>
        /// <returns>如果检索到有效的定义将返回true</returns>
        public abstract bool TryGetLocalListValue(IProperty property, out IEnumerable<ListDifferenceItem> list);

        /// <summary>
        /// 返回基类的指针。
        /// </summary>
        internal ObjectPtr Base {
            get {
                object value;
                if (TryGetLocalValue(BasePropertyInstance, out value)) {
                    return value == null ? ObjectPtr.None : (ObjectPtr)value;
                }

                return ObjectPtr.None;
            }
        }

        /// <summary>
        /// 能够访问到基类信息。
        /// </summary>
        protected static IProperty BasePropertyInstance = new BaseProperty();
        #endregion

        #region TypeInfo
        /// <summary>
        /// 返回当前部件对应的类型
        /// </summary>
        public abstract IType Type {
            get;
        }
        #endregion

        #region BaseProperty
        private sealed class BaseProperty : IProperty {
            public object DefaultValue {
                get { return null; }
            }

            public bool IsReadOnly {
                get { return true; }
            }

            public string Name {
                get { return "__Base__"; }
            }

            public IType PropertyType {
                get { return null; }
            }
        }
        #endregion

        #region Open
        private bool _isOpened;

        internal void OpenData(OpenDataContext ctx) {
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
        internal bool IsOpened {
            get {
                return this._isOpened;
            }
        }

        /// <summary>
        /// 派生类重载此方法，将原始的数据解开填充到当前数据包中。
        /// </summary>
        protected abstract void OpenDataCore(OpenDataContext ctx);

        #endregion
    }
}
