using System.Collections.Generic;
using System.Configuration.Core.Metadata;

namespace System.Configuration.Core.Dc {

    internal sealed class ObjectTypeReadData {
        public ObjectTypeReadData() {
            this.Properties = new List<ObjectPropertyReadData>();
            this.Properties.Add(null);
        }

        public IType Type { get; set; }
        public ObjectTypeQualifiedName Name { get; set; }

        public List<ObjectPropertyReadData> Properties { get; }
    }

    internal sealed class ObjectPropertyReadData {
        public string Name { get; set; }

        public IProperty Property { get; set; }
    }
}
