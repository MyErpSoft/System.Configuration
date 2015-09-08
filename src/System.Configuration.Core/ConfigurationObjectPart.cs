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
        public static IProperty BasePropertyInstance = new BaseProperty();
        #endregion

        /// <summary>
        /// 返回此部件定义的所有本地值清单。
        /// </summary>
        /// <returns>一个枚举的清单，用于返回</returns>
        public abstract IEnumerable<KeyValuePair<IProperty, object>> GetLocalValues();

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
    }
}
