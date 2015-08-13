using System.Reflection;

namespace System.Configuration.Core.Metadata.Clr {

    internal sealed class ClrProperty : MemberMetadataBase<PropertyInfo>, IProperty {

        public ClrProperty(PropertyInfo property)
            : base(property) {
        }
    }
}
