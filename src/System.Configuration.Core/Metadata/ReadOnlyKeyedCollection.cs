using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace System.Configuration.Core.Metadata {

    /// <summary>
    /// 只读且使用键访问的集合。
    /// </summary>
    /// <typeparam name="TKey">元素键的数据类型</typeparam>
    /// <typeparam name="TItem">元素的数据类型</typeparam>
    /// <remarks>
    /// <para>本类适合访问频繁，存在并发读取，但极少改动的集合，例如系统一些元数据的描述对象。</para>
    /// <para>虽然此类支持常见的索引访问，但在并发情况下可能存在问题，例如在循环的过程中又添加了新的项目。但是因为此集合只会最后添加且只能添加元素，
    /// 所以使用索引访问不会出现索引超出范围的异常。</para>
    /// <para>此类对外提供只读的访问，但派生类支持对集合的修改。</para>
    /// </remarks>
    [DebuggerTypeProxy(typeof(ReadOnlyKeyedCollection_DebugView<,>)), DebuggerDisplay("Count = {Count}")]
    internal abstract class ReadOnlyKeyedCollection<TKey, TItem> : IEnumerable<TItem> {
        private TItem[] _items;
        private Dictionary<TKey, TItem> _dict;
        private static readonly TItem[] _emptyArray = new TItem[0];

        /// <summary>
        /// 创建一个只读可用键访问的集合 ReadOnlyKeyedCollection
        /// </summary>
        protected ReadOnlyKeyedCollection() : this(null, null) { }

        /// <summary>
        /// 创建一个只读可用键访问的集合 ReadOnlyKeyedCollection
        /// </summary>
        /// <param name = "items" > 用于初始化的数组，可以为null，如果是数组，将直接使用他，注意，外部不要修改这个数组。</param>
        protected ReadOnlyKeyedCollection(IEnumerable<TItem> items) : this(items, null) { }

        /// <summary>
        /// 创建一个只读可用键访问的集合 ReadOnlyKeyedCollection
        /// </summary>
        /// <param name="items">用于初始化的数组，可以为null，如果是数组，将直接使用他，注意，外部不要修改这个数组。</param>
        protected ReadOnlyKeyedCollection(IEnumerable<TItem> items, IEqualityComparer<TKey> comparer) {
            if (comparer == null) {
                comparer = EqualityComparer<TKey>.Default;
            }
            this._comparer = comparer;

            if (items == null) {
                _items = _emptyArray;
            }
            else {
                InsertItems(items);
            }
        }

        private readonly IEqualityComparer<TKey> _comparer;
        /// <summary>
        /// 获取用于确定集合中的键是否相等的泛型相等比较器。
        /// </summary>
        public IEqualityComparer<TKey> Comparer {
            get { return this._comparer; }
        }

        /// <summary>
        /// 在派生类中实现时，将从指定元素提取键。
        /// </summary>
        /// <param name="item">从中提取键的元素。</param>
        /// <returns>指定元素的键</returns>
        protected abstract TKey GetKeyForItem(TItem item);

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
            get {
                if (_dict == null && (_items.Length > 0)) {
                    var spin = new SpinWait();

                    TItem[] items;
                    while (true) {
                        items = _items;
                        Interlocked.CompareExchange(ref _dict, this.CreateDictionary(items), null);

                        //可能在CreateDictionary()时，并发又替换了数组，需要重新创建字典。
                        if (items == _items) {
                            break;
                        }

                        spin.SpinOnce();
                    }

                }

                return _dict;
            }
        }

        //根据一批数据创建其对应的字典。
        private Dictionary<TKey, TItem> CreateDictionary(IEnumerable<TItem> items) {
            var dict = new Dictionary<TKey, TItem>(this._comparer);
            foreach (var item in items) {
                TKey key = GetKeyForItem(item);
                if (key != null) {
                    dict.Add(key, item);
                }
            }

            return dict;
        }

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
        public bool Contains(TKey key) {
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
            get {
                TItem item;
                if (!this.TryGetValue(key, out item)) {
                    ThrowKeyNotFoundException(key);
                }

                return item;
            }
        }

        protected virtual void ThrowKeyNotFoundException(TKey key) {
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Gets the value associated with the specified name.
        /// </summary>
        /// <param name="name">To get the name of the value.</param>
        /// <param name="value">When this method returns, if the specified key is found, returns the value associated with the key; otherwise, it returns the type of the value parameter's default value. The parameter is not initialized is passed.</param>
        /// <returns>If the list element contains an element with the specified name, or true; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out TItem value) {
            var dict = this.Dictionary;
            if (dict == null) {
                value = default(TItem);
                return false;
            }

            return dict.TryGetValue(key, out value);
        }

        protected virtual void InsertItems(IEnumerable<TItem> items) {
            if (null == items) {
                throw new ArgumentNullException("items");
            }

            var spin = new SpinWait();
            var currentDict = CreateDictionary(items);
            TItem[] oldItems;

            while (true) {
                oldItems = _items;

                int count = 0;
                TKey key;

                foreach (var item in items) {
                    if (null == item) {
                        throw new ArgumentNullException("items[" + count.ToString() + "]");
                    }

                    key = GetKeyForItem(item);
                    if (_comparer.Equals(key, default(TKey))) {
                        throw new ArgumentNullException("Add an element name cannot be empty.");
                    }

                    if (Contains(key) || currentDict.ContainsKey(key)) {
                        throw new ArgumentNullException(
                            string.Format(CultureInfo.CurrentCulture, "The element name {0} already exists in the collection.", key));
                    }

                    currentDict.Add(key, item);
                    count++;
                }

                TItem[] newItems;
                if (oldItems == null) {
                    newItems = oldItems;
                }
                else {
                    var startIndex = oldItems.Length;
                    newItems = new TItem[count + startIndex];
                    oldItems.CopyTo(newItems, 0);

                    foreach (var item in items) {
                        newItems[startIndex++] = item;
                    }
                }

                Interlocked.CompareExchange(ref _items, newItems, oldItems);
                if (_items == newItems) {
                    _dict = null;
                    break;
                }

                spin.SpinOnce();
            }
        }

        /// <summary>
        /// Allows a derived class to add data.
        /// </summary>
        /// <param name="item">The element to be added.</param>
        protected virtual void InsertItem(TItem item) {
            if (null == item) {
                throw new ArgumentNullException("item");
            }

            var key = GetKeyForItem(item);
            if (_comparer.Equals(key, default(TKey))) {
                throw new ArgumentNullException("Add an element name cannot be empty.");
            }

            var spin = new SpinWait();
            TItem[] oldItems;

            while (true) {
                oldItems = _items;
                if (Contains(key)) {
                    throw new ArgumentNullException(
                        string.Format(CultureInfo.CurrentCulture, "The element name {0} already exists in the collection.", key));
                }

                TItem[] newItems = new TItem[1 + oldItems.Length];
                oldItems.CopyTo(newItems, 0);
                newItems[oldItems.Length] = item;
                Interlocked.CompareExchange(ref _items, newItems, oldItems);
                if (_items == newItems) {
                    _dict = null;
                    break;
                }

                spin.SpinOnce();
            }
        }

        #region ICollection<T>

        public IEnumerator<TItem> GetEnumerator() {
            return new ArrayEnumerator(_items);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new ArrayEnumerator(_items);
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
