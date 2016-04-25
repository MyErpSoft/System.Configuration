using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System.Configuration.Core.Metadata {

    /// <summary>
    /// 只读且使用键访问的集合。
    /// </summary>
    /// <typeparam name="TKey">元素键的数据类型</typeparam>
    /// <typeparam name="TItem">元素的数据类型</typeparam>
    /// <remarks>
    /// <para>本类适合仅只读、按索引或键访问的集合，需要在初始化时提供集合的所有数据。</para>
    /// </remarks>
    [DebuggerTypeProxy(typeof(ReadOnlyKeyedCollection_DebugView<,>)), DebuggerDisplay("Count = {Count}")]
    internal abstract class ReadOnlyKeyedCollection<TKey, TItem> : IReadOnlyCollection<TItem>,IReadOnlyDictionary<TKey, TItem>, IEnumerable<TItem> {
        private readonly TItem[] _items;
        private readonly Dictionary<TKey, TItem> _dict;
        
        /// <summary>
        /// 创建一个只读可用键访问的集合 ReadOnlyKeyedCollection
        /// </summary>
        /// <param name="items">用于初始化的数组，可以为null，如果是数组，将直接使用他，注意，外部不要修改这个数组。</param>
        protected ReadOnlyKeyedCollection(Dictionary<TKey,TItem> dict) {
            if (dict == null) {
                Utilities.ThrowArgumentNull(nameof(dict));
            }

            _dict = dict;
            _items = dict.Values.ToArray();
        }

        /// <summary>
        /// 返回集合类型的数组集合，尽力避免操控此数组。
        /// </summary>
        protected TItem[] Items {
            get { return _items; }
        }

        /// <summary>
        /// 返回内部的字典对象，尽力避免操控此对象。
        /// </summary>
        protected Dictionary<TKey, TItem> Dictionary {
            get { return _dict; }
        }

        /// <summary>
        /// 在派生类中实现时，将从指定元素提取键。
        /// </summary>
        /// <param name="item">从中提取键的元素。</param>
        /// <returns>指定元素的键</returns>
        protected abstract TKey GetKeyForItem(TItem item);

        /// <summary>
        /// Determines whether a sequence contains a specified element by using the default equality comparer.
        /// </summary>
        /// <param name="item">The value to locate in the sequence.</param>
        /// <returns>true if the source sequence contains an element that has the specified value; otherwise, false.</returns>
        public bool Contains(TItem item) {
            if (item == null) {
                return false;
            }

            TItem findItem;
            if (this.TryGetValue(GetKeyForItem(item), out findItem) &&
                object.Equals(findItem, item)) {
                return true;
            }

            return Array.IndexOf<TItem>(_items, item) >= 0;
        }

        /// <summary>
        /// Determines whether a sequence contains a specified name by using the default equality comparer.
        /// </summary>
        /// <param name="key">The name to locate in the sequence.</param>
        /// <returns>true if the source sequence contains name that has the specified value; otherwise, false.</returns>
        public bool ContainsKey(TKey key) {
            TItem item;
            return this.TryGetValue(key, out item);
        }

        /// <summary>
        /// Copies the elements of the IMetadataReadOnlyCollection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from IMetadataReadOnlyCollection. The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(TItem[] array, int arrayIndex) {
            _items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 复制数据到数组。
        /// </summary>
        /// <returns>新的数组</returns>
        public TItem[] ToArray() {
            var items = _items;
            TItem[] newArray = new TItem[items.Length];
            items.CopyTo(newArray, 0);

            return newArray;
        }

        /// <summary>
        /// Gets the number of elements contained in the IMetadataReadOnlyCollection.
        /// </summary>
        public int Count {
            get { return _items.Length; }
        }

        public IEnumerable<TKey> Keys {
            get { return _dict.Keys; }
        }

        public IEnumerable<TItem> Values {
            get { return _dict.Values; }
        }

        /// <summary>
        /// Determines the index of a specific item in the IMetadataReadOnlyCollection.
        /// </summary>
        /// <param name="item">The object to locate in the IMetadataReadOnlyCollection</param>
        /// <returns>The index of item if found in the list; otherwise, -1.</returns>
        public int IndexOf(TItem item) {
            return Array.IndexOf<TItem>(_items, item);
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Index is not a valid index of IMetadataReadOnlyCollection.</exception>
        public TItem this[int index] {
            get {
                return _items[index];
            }
        }

        /// <summary>
        /// Gets the element with the specified name.
        /// </summary>
        /// <param name="name">To obtain the distinguished name of the element.</param>
        /// <returns>An element with the specified name.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Retrieves the property name was not found.</exception>
        public TItem this[TKey key] {
            get { return _dict[key]; }
        }
        
        /// <summary>
        /// Gets the value associated with the specified name.
        /// </summary>
        /// <param name="name">To get the name of the value.</param>
        /// <param name="value">When this method returns, if the specified key is found, returns the value associated with the key; otherwise, it returns the type of the value parameter's default value. The parameter is not initialized is passed.</param>
        /// <returns>If the list element contains an element with the specified name, or true; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out TItem value) {
            return this._dict.TryGetValue(key, out value);
        }

        #region ICollection<T>

        public IEnumerator<TItem> GetEnumerator() {
            return new ArrayEnumerator(_items);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new ArrayEnumerator(_items);
        }
        
        IEnumerator<KeyValuePair<TKey, TItem>> IEnumerable<KeyValuePair<TKey, TItem>>.GetEnumerator() {
            return _dict.GetEnumerator();
        }

        #endregion

        #region The enumerator
        private struct ArrayEnumerator : IEnumerator<TItem> {
            private TItem[] _array;
            private int _endIndex;
            private int _index;
            private TItem _current;

            internal ArrayEnumerator(TItem[] array) {
                this._array = array;
                this._index = -1;
                this._endIndex = array.Length;
                this._current = default(TItem);
            }

            public bool MoveNext() {
                this._index++;
                if (this._index < this._endIndex) {
                    _current = _array[_index];
                    return true;
                }
                return false;
            }

            public void Reset() {
                this._index = -1;
            }

            public TItem Current {
                get {
                    return _current;
                }
            }

            object IEnumerator.Current {
                get { return _current; }
            }

            public void Dispose() {
                //
            }
        }
        #endregion
    }

    internal sealed class ReadOnlyKeyedCollection_DebugView<TKey, TValue> {
        private ReadOnlyKeyedCollection<TKey, TValue> collection;

        public ReadOnlyKeyedCollection_DebugView(ReadOnlyKeyedCollection<TKey, TValue> collection) {
            if (null == collection) {
                throw new ArgumentNullException("collection");
            }
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items {
            get {
                return this.collection.ToArray();
            }
        }
    }
}
