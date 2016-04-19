namespace System.Configuration.Core {

    /// <summary>
    /// 表示一个对象的指针，比如一个复杂属性指向一个对象用此描述指向。
    /// </summary>
    /// <remarks>
    /// <para>为什么不直接使用QualifiedName呢？因为如果程序赋值了QualifiedName，我如何知道他就是要这个值还是要指向一个对象呢？</para>
    /// <para>所以我们使用了这个类，只要是这个类的实例，就认为是指针。</para>
    /// </remarks>
    internal struct ObjectPtr {
        public static readonly ObjectPtr None = new ObjectPtr();

        public ObjectPtr(QualifiedName name) {
            this._name = name;
        }

        private readonly QualifiedName _name;
        /// <summary>
        /// 返回对象的指向。
        /// </summary>
        public QualifiedName Name {
            get { return this._name; }
        }
        
        public override string ToString() {
            return this._name == QualifiedName.Empty ? "<None>" : this._name.ToString();
        }

        public override bool Equals(object obj) {
            if (obj is ObjectPtr) {
                ObjectPtr other = (ObjectPtr)obj;
                return other._name == this._name;
            }

            if (obj == null && this._name == QualifiedName.Empty) {
                return true;
            }

            return false;
        }

        public override int GetHashCode() {
            return this._name == QualifiedName.Empty ? 0 : this._name.GetHashCode();
        }

        public static bool operator ==(ObjectPtr x, ObjectPtr y) {
            return x._name == y._name;
        }

        public static bool operator !=(ObjectPtr x, ObjectPtr y) {
            return x._name != y._name;
        }
    }
}
