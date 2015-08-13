using System.Collections.Generic;
using System.Configuration.Core.Metadata;

namespace System.Configuration.Core {

    internal abstract class BasicPart : ConfigurationObjectPart {

        protected BasicPart() {
            this._values = new Dictionary<IProperty, object>();
        }
        
        private Dictionary<IProperty, object> _values;

        #region TryGetValue
        
        /// <summary>
        /// 返回部件内部存储的值，如果内部有定义值将返回他。
        /// </summary>
        /// <param name="property">要检索的属性</param>
        /// <param name="value">如果检索到有效的定义将返回他，否则返回null</param>
        /// <returns>如果检索到有效的定义将返回true</returns>
        public override bool TryGetLocaleValue(IProperty property, out object value) {
            return _values.TryGetValue(property, out value);
        }

        protected void SetLocalValue(IProperty property, object value) {
            _values[property] = value;
        }

        protected void ResetLocalValue(IProperty property) {
            _values.Remove(property);
        }
        
        #endregion

    }
}
