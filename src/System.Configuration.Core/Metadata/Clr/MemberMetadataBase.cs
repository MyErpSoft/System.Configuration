using System.Reflection;

namespace System.Configuration.Core.Metadata.Clr {

    public abstract class MemberMetadataBase<T> where T : MemberInfo {
        /// <summary>
        /// Return CLR member.
        /// </summary>
        protected readonly T ClrMapping;

        /// <summary>
        /// Create MemberMetadataBase instance.
        /// </summary>
        /// <param name="memberInfo">CLR member object.</param>
        protected MemberMetadataBase(T memberInfo) {
            if (null == memberInfo) {
                Utilities.ThrowArgumentNull("memberInfo");
            }

            this.ClrMapping = memberInfo;
        }

        public string Name {
            get { return ClrMapping.Name; }
        }

        #region Equals
        /// <summary>
        /// 如果比较的对象是一个Clr实例，那么他们将相等，否则不等。
        /// </summary>
        /// <param name="obj">要判断的对象。</param>
        /// <returns>是否关联了同一个Clr对象。</returns>
        public override bool Equals(object obj) {
            MemberMetadataBase<T> other = obj as MemberMetadataBase<T>;
            if (other != null) {
                return other.ClrMapping == this.ClrMapping;
            }
            return false;
        }

        /// <summary>
        /// 返回实例关联的Clr的HashCode.
        /// </summary>
        /// <returns>一个int的值。</returns>
        public override int GetHashCode() {
            return this.ClrMapping.GetHashCode();
        }

        /// <summary>
        /// 返回clr对象的输出
        /// </summary>
        /// <returns>描述关联clr对象的字符串。</returns>
        public override string ToString() {
            return this.ClrMapping.ToString();
        }
        #endregion
    }
}
