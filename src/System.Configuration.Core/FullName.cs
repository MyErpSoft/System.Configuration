using System.Collections.Generic;

namespace System.Configuration.Core {

    public struct FullName {
        public readonly string Namespace;
        public readonly string Name;

        public FullName(string objNamespace,string name) {
            this.Namespace = objNamespace;
            this.Name = name;
        }

        public override string ToString() {
            return string.IsNullOrEmpty(this.Namespace) ? this.Name : this.Namespace + "." + this.Name;
        }

        public override bool Equals(object obj) {
            if (obj is FullName) {
                FullName other = (FullName)obj;
                return this.Name == other.Name && this.Namespace == other.Namespace;
            }

            return false;
        }

        public override int GetHashCode() {
            return (this.Namespace == null ? 0 : this.Namespace.GetHashCode()) ^
                    (this.Name == null ? 0 : this.Name.GetHashCode());
        }

        public static bool operator ==(FullName x, FullName y) {
            return x.Name == y.Name && x.Namespace == y.Namespace;
        }

        public static bool operator !=(FullName x, FullName y) {
            return !(x == y);
        }

        public static IEqualityComparer<FullName> Comparer {
            get {
                return new PartKeyEqualityComparer();
            }
        }

        private sealed class PartKeyEqualityComparer : IEqualityComparer<FullName> {
            public bool Equals(FullName x, FullName y) {
                return x.Name == y.Name && x.Namespace == y.Namespace;
            }

            public int GetHashCode(FullName obj) {
                return (obj.Namespace == null ? 0 : obj.Namespace.GetHashCode()) ^
                    (obj.Name == null ? 0 : obj.Name.GetHashCode());
            }
        }
    }

    
}
