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

    }
}
