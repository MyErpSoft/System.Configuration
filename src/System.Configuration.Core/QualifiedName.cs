namespace System.Configuration.Core {

    /// <summary>
    /// 表示一个配置对象的识别键，包含所在的组装件、命名空间和名称。
    /// </summary>
    public struct QualifiedName : IEquatable<QualifiedName> {
        /// <summary>结构不允许为null，所以这里内部使用了Empty.</summary>
        internal readonly static QualifiedName Empty = new QualifiedName(null, null, null);
         
        public QualifiedName(string objNamespace, string name, string packageName)
            : this(new FullName(objNamespace, name), packageName) {
        }

        public QualifiedName(FullName fullName, string packageName) {
            this.FullName = fullName;
            this.PackageName = packageName;
        }

        /// <summary>
        /// 返回所在的包。
        /// </summary>
        public readonly string PackageName;

        /// <summary>
        /// 返回命名空间和名称
        /// </summary>
        public readonly FullName FullName;

        /// <summary>
        /// 返回命名空间
        /// </summary>
        public string Namespace { get { return FullName.Namespace; } }

        /// <summary>
        /// 返回名称。
        /// </summary>
        public string Name { get { return FullName.Name; } }

        /// <summary>
        /// 输出调试用的字符串，类似 mynamespace.name,packageName 这样的形式。
        /// </summary>
        /// <returns>一个字符串。</returns>
        public override string ToString() {
            return string.IsNullOrEmpty(this.PackageName) ? this.FullName.ToString() :
                (this.FullName.ToString() + "," + this.PackageName);
        }

        /// <summary>
        /// 判断两个对象是否相等，QualifiedName，那么判断FullName和PackageName是否都相等。注意：PackageName不区分大小写。
        /// </summary>
        /// <param name="obj">要判断的对象。</param>
        /// <returns>如果相等返回true，否则false.</returns>
        public override bool Equals(object obj) {
            if (obj is QualifiedName) {
                return this.Equals((QualifiedName)obj);
            }

            //也支持对null的相等判断
            if (obj == null && this == Empty) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 返回FullName和PackageName组合后的HashCode。
        /// </summary>
        /// <returns>一个int32固定值。</returns>
        public override int GetHashCode() {
            return this.FullName.GetHashCode() ^ (this.PackageName == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(this.PackageName));
        }

        /// <summary>
        /// 判断两个对象是否相等，如果FullName和PackageName都相等。注意：PackageName不区分大小写。，则返回true。。
        /// </summary>
        /// <param name="other">要比较的对象。</param>
        /// <returns>返回是否相等。</returns>
        public bool Equals(QualifiedName other) {
            return this.FullName == other.FullName && string.Equals(this.PackageName, other.PackageName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断两个对象是否相等，如果FullName和PackageName都相等。注意：PackageName不区分大小写。，则返回true。。
        /// </summary>
        /// <param name="x">要比较的对象。</param>
        /// <param name="y">要比较的第二个对象。</param>
        /// <returns>返回是否相等。</returns>
        public static bool operator ==(QualifiedName x, QualifiedName y) {
            return x.Equals(y);
        }

        /// <summary>
        /// 判断两个对象是否不相等，如果FullName和PackageName任何一个不相等。注意：PackageName不区分大小写。，则返回true。。
        /// </summary>
        /// <param name="x">要比较的对象。</param>
        /// <param name="y">要比较的第二个对象。</param>
        /// <returns>返回是否不相等。</returns>
        public static bool operator !=(QualifiedName x, QualifiedName y) {
            return !x.Equals(y);
        }
    }
}
