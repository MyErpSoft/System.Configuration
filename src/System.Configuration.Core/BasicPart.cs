using System.Collections.Generic;
using System.Configuration.Core.Metadata;

namespace System.Configuration.Core {

    internal abstract class BasicPart : ConfigurationObjectPart {
        private ObjectTypeQualifiedName _typeName;

        protected BasicPart(ObjectTypeQualifiedName typeName) {
            this._typeName = typeName;
            this._values = new Dictionary<IProperty, object>(ReferenceEqualityComparer<IProperty>.Default);
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

        #region TypeInfo

        private IType _type;
        /// <summary>
        /// 返回当前对象的类型信息。
        /// </summary>
        public override IType Type {
            get { return _type; }
        }
        #endregion

        #region OpenData
        protected override void OpenDataCore(OpenDataContext ctx) {
            if (ctx.Type != null) {
                this._type = ctx.Type;
            }
            else {
                if ((object)this._typeName != null) {
                    this._type = ctx.Binder.BindToType(_typeName);
                    ctx.Type = this._type;
                }
            }
        }
        #endregion
        
    }
}
