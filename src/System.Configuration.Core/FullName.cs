using System.Collections.Generic;

namespace System.Configuration.Core {

    /// <summary>
    /// 描述命名空间和名称。
    /// </summary>
    public struct FullName : IEquatable<FullName> {

        /// <summary>
        /// 返回命名空间
        /// </summary>
        public readonly string Namespace;

        /// <summary>
        /// 返回名称。
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// 创建 FullName ，但注意此构造并不检查合法性。
        /// </summary>
        /// <param name="objNamespace">命名空间</param>
        /// <param name="name">名称</param>
        public FullName(string objNamespace,string name) {
            this.Namespace = objNamespace;
            this.Name = name;
        }

        /// <summary>
        /// 输出调试用的字符串，类似 mynamespace.name 这样的形式。
        /// </summary>
        /// <returns>一个字符串。</returns>
        public override string ToString() {
            return string.IsNullOrEmpty(this.Namespace) ? this.Name : 
                (string.IsNullOrEmpty(this.Name) ? this.Name : this.Namespace + "." + this.Name);
        }

        /// <summary>
        /// 判断两个对象是否相等，如果是FullName对象，那么判断Namespace和Name是否都相等。区分大小写。
        /// </summary>
        /// <param name="obj">要判断的对象。</param>
        /// <returns>如果相等返回true，否则false.</returns>
        public override bool Equals(object obj) {
            if (obj is FullName) {
                FullName other = (FullName)obj;
                return Equals((FullName)obj);
            }

            return false;
        }

        /// <summary>
        /// 返回Namespace和Name组合后的HashCode。
        /// </summary>
        /// <returns>一个int32固定值。</returns>
        public override int GetHashCode() {
            return (this.Namespace == null ? 0 : this.Namespace.GetHashCode()) ^
                    (this.Name == null ? 0 : this.Name.GetHashCode());
        }

        /// <summary>
        /// 判断两个对象是否相等，如果是FullName对象，那么判断Namespace和Name是否都相等。区分大小写。
        /// </summary>
        /// <param name="other">要判断的对象。</param>
        /// <returns>如果相等返回true，否则false.</returns>
        public bool Equals(FullName other) {
            return this.Name == other.Name && this.Namespace == other.Namespace;
        }

        /// <summary>
        /// 判断两个对象是否相等，如果Namespace和Name都相等（区分大小写），则返回true。。
        /// </summary>
        /// <param name="x">要比较的对象。</param>
        /// <param name="y">要比较的第二个对象。</param>
        /// <returns>返回是否相等。</returns>
        public static bool operator ==(FullName x, FullName y) {
            return x.Name == y.Name && x.Namespace == y.Namespace;
        }

        /// <summary>
        /// 判断两个对象是否不相等，如果Namespace和Name任何有一个不相等（区分大小写），则返回true。。
        /// </summary>
        /// <param name="x">要比较的对象。</param>
        /// <param name="y">要比较的第二个对象。</param>
        /// <returns>返回是否不相等。</returns>
        public static bool operator !=(FullName x, FullName y) {
            return x.Name != y.Name || x.Namespace != y.Namespace;
        }
        
    }

    
}
