using System;
using System.Collections.Concurrent;
using System.Threading;

namespace System.Configuration.Difference.Metadata {

    /// <summary>
    /// Default object type container.
    /// </summary>
    public abstract class ObjectTypeContainer<SourceT> {
        private readonly ConcurrentDictionary<SourceT, IObjectType> _items;

        public ObjectTypeContainer() {
            this._items = new ConcurrentDictionary<SourceT, IObjectType>();
        }

        public IObjectType GetObjectType(SourceT sourceType) {
            return this._items.GetOrAdd(sourceType, this.CreateObjectTypeCore);
        }

        protected abstract IObjectType CreateObjectTypeCore(SourceT sourceType);

        /// <summary>
        /// Get all IObjectType.
        /// </summary>
        /// <returns></returns>
        public IObjectType[] ToArray() {
            var items = this._items.ToArray();
            var array = new IObjectType[items.Length];
            for (int i = 0; i < items.Length; i++) {
                array[i] = items[i].Value;
            }
            return array;
        }

        protected ConcurrentDictionary<SourceT, IObjectType> Items {
            get { return this._items; }
        }
    }
}
