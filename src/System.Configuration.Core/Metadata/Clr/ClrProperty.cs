using System.Reflection;
using System.Threading;

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
                    Interlocked.CompareExchange(ref _propertyType, ClrType.GetClrType(this.ClrMapping.PropertyType), null);
                }

                return this._propertyType;
            }
        }
    }
}
