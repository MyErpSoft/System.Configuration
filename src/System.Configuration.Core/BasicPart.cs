using System.Collections.Generic;
using System.Data.Metadata.DataEntities;

namespace System.Configuration.Core {

    internal abstract class BasicPart : ConfigurationObjectPart {

        protected BasicPart() {
            this._values = new Dictionary<IEntityProperty, object>();
        }
        
        private Dictionary<IEntityProperty, object> _values;

        #region TryGetValue
        
        /// <summary>
        /// 返回部件内部存储的值，如果内部有定义值将返回他。
        /// </summary>
        /// <param name="property">要检索的属性</param>
        /// <param name="value">如果检索到有效的定义将返回他，否则返回null</param>
        /// <returns>如果检索到有效的定义将返回true</returns>
        public override bool TryGetLocaleValue(IEntityProperty property, out object value) {
            return _values.TryGetValue(property, out value);
        }

        protected void SetLocalValue(IEntityProperty property, object value) {
            _values[property] = value;
        }

        protected void ResetLocalValue(IEntityProperty property) {

        }
        #endregion
    }
}
