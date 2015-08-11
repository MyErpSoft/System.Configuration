namespace System.Configuration.Core {

    /// <summary>
    /// 表示一个配置对象的识别键，包含所在的组装件、命名空间和名称。
    /// </summary>
    public class QualifiedName {

        public QualifiedName(string objNamespace, string name, string packageName)
            : this(new FullName(objNamespace, name), packageName) {
        }

        public QualifiedName(FullName fullName, string packageName) {
            this._fullName = fullName;
            this._packageName = packageName;
        }

        private readonly string _packageName;
        /// <summary>
        /// 返回所在的包。
        /// </summary>
        public string PackageName {
            get { return _packageName; }
        }

        private readonly FullName _fullName;
        /// <summary>
        /// 返回命名空间和名称
        /// </summary>
        public FullName FullName {
            get { return _fullName; }
        }

        public override string ToString() {
            return this._fullName.ToString() + "," + this.PackageName;
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(this,obj)) {
                return true;
            }

            QualifiedName other = obj as QualifiedName;
            if (other != null) {
                return this._fullName == other._fullName && this._packageName == other._packageName;
            }

            return false;
        }

        public override int GetHashCode() {
            return this._fullName.GetHashCode() ^ (this._packageName == null ? 0 : this._packageName.GetHashCode());
        }

        public static bool operator ==(QualifiedName x, QualifiedName y) {
            if (object.ReferenceEquals(x,y)) {
                return true;
            }

            if ((object)x == null || (object)y == null) {
                return false;
            }

            return x._fullName == y._fullName && x._packageName == y._packageName;
        }

        public static bool operator !=(QualifiedName x, QualifiedName y) {
            return !(x == y);
        }
    }
}
