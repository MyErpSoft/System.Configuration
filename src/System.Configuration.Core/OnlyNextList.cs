using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Configuration.Core {

    /// <summary>
    /// 一种仅仅能向前检索的列表，开销更小。
    /// </summary>
    internal struct OnlyNextList<T> : IEnumerable<T> {
        OnlyNextNode<T> _first;
        OnlyNextNode<T> _last;

        public OnlyNextNode<T> First {
            get { return _first; }
        }

        public OnlyNextNode<T> Last {
            get { return _last; }
        }

        public void Add(T value) {
            _last = new OnlyNextNode<T>(value, _last);
            if (_first == null) {
                _first = _last;
            }
        }

        public void AddRange(IEnumerable<T> items) {
            if (items != null) {
                foreach (var item in items) {
                    this.Add(item);
                }
            }
        }

        #region IEnumerable
        public IEnumerator<T> GetEnumerator() {
            return new Enumerator(_first);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(_first);
        }

        internal struct Enumerator : IEnumerator<T> {
            private OnlyNextNode<T> _node;
            private T _current;

            public Enumerator(OnlyNextNode<T> first) {
                this._node = first;
                this._current = default(T);
            }

            public T Current {
                get { return _current; }
            }

            object IEnumerator.Current {
                get { return _current; }
            }

            public void Dispose() { }

            public bool MoveNext() {
                if (_node == null) {
                    return false;
                }

                _current = _node.Value;
                _node = _node.Next;

                return true;
            }

            public void Reset() {
                Utilities.ThrowNotSupported("not supported");
            }
        } 
        #endregion
    }

    internal sealed class OnlyNextNode<T> {
        internal OnlyNextNode(T value, OnlyNextNode<T> preNode) {
            this.Value = value;

            if (preNode != null) {
                Debug.Assert(preNode._next == null);
                preNode._next = this;
            }
        }

        private OnlyNextNode(T value) {
            this.Value = value;
        }

        /// <summary>
        /// 返回关联的值。
        /// </summary>
        public readonly T Value;

        private OnlyNextNode<T> _next;
        /// <summary>
        /// 返回下一个节点。
        /// </summary>
        public OnlyNextNode<T> Next {
            get { return _next; }
        }

        /// <summary>
        /// 反转当前结点以及后续节点，得到一个新的列表。
        /// </summary>
        /// <returns>假设原先节点是：1>2>3，那么反转后得到的列表是：3>2>1</returns>
        public OnlyNextNode<T> Reverse() {
            var current = this;
            OnlyNextNode<T> first = null;

            do {
                //第一次处理时，示例中是1，所以创建的节点其Next指向null.
                //第二次处理是，实例中是2，所以创建的节点的Next指向1；
                OnlyNextNode<T> newNode = new OnlyNextNode<T>(current.Value) { _next = first };
                first = newNode;

                current = current._next;
            } while (current != null);

            return first;
        }
    }
}
