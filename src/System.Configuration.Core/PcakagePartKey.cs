using System.Collections.Generic;

namespace System.Configuration.Core {

    public struct PcakagePartKey {
        public readonly string Namespace;
        public readonly string Name;

        public PcakagePartKey(string objNamespace,string name) {
            this.Namespace = objNamespace;
            this.Name = name;
        }

        public override string ToString() {
            return string.IsNullOrEmpty(this.Namespace) ? this.Name : this.Namespace + "." + this.Name;
        }

        public static IEqualityComparer<PcakagePartKey> Comparer {
            get {
                return new PartKeyEqualityComparer();
            }
        }

        private sealed class PartKeyEqualityComparer : IEqualityComparer<PcakagePartKey> {
            public bool Equals(PcakagePartKey x, PcakagePartKey y) {
                return x.Name == y.Name && x.Namespace == y.Namespace;
            }

            public int GetHashCode(PcakagePartKey obj) {
                return (obj.Namespace == null ? 0 : obj.Namespace.GetHashCode()) ^
                    (obj.Name == null ? 0 : obj.Name.GetHashCode());
            }
        }
    }

    
}
