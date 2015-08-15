using System.Reflection;

namespace System.Configuration.Core.Metadata.Clr {

    public sealed class ClrProperty : MemberMetadataBase<PropertyInfo>, IProperty {

        private object _defaultValue;
        public ClrProperty(PropertyInfo property)
            : base(property) {

            if (property.PropertyType.IsValueType) {
                _defaultValue = Activator.CreateInstance(property.PropertyType);
            }
            else {
                _defaultValue = null;
            }
        }

        public object DefaultValue {
            get { return _defaultValue; }
        }

        private IType _propertyType;
        public IType PropertyType {
            get {
                if (_propertyType == null) {
                    _propertyType = ClrType.GetClrType(this.ClrMapping.PropertyType);
                }

                return this._propertyType;
            }
        }
    }
}
