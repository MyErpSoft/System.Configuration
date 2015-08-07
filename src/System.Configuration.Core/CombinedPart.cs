using System.Diagnostics;

namespace System.Configuration.Core {

    internal sealed class CombinedPart : ConfigurationObjectPart {

        public CombinedPart(ConfigurationObjectPart value,CombinedPart preNode) {
            Debug.Assert(value != null);
            this._value = value;

            if (preNode != null) {
                Debug.Assert(preNode._next == null);
                preNode._next = this;
            }
        }

        private CombinedPart _next;
        public CombinedPart Next {
            get { return this._next; }
        }

        private readonly ConfigurationObjectPart _value;
        /// <summary>
        /// 当前结点的值。
        /// </summary>
        public ConfigurationObjectPart Value {
            get { return _value; }
        }

        protected override void OpenDataCore() {
            var current = this;
            do {
                current.OpenData();//可能已经被其他workspace解包了，所以调用OpenData而不是OpenDataCore
                current = current.Next;
            } while (current != null);
        }
        
    }
}
