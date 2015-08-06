using System.Collections.Concurrent;

namespace System.Configuration.Core {

    /// <summary>
    /// 存储对象的容器，通过此容器获取一个配置对象。
    /// </summary>
    /// <typeparam name="TKey">用于定位对象的唯一标识</typeparam>
    /// <typeparam name="TValue">对象的类型</typeparam>
    internal class ObjectContainer<TKey, TValue> : ConcurrentDictionary<TKey, TValue> {
        private readonly Func<TKey, TValue> _createObjectFunc;

        /// <summary>
        /// 创建默认的ObjectContainer 实例。
        /// </summary>
        /// <param name="createObjectFunc">创建对象的委托</param>
        public ObjectContainer(Func<TKey,TValue> createObjectFunc) {
            if (createObjectFunc == null) {
                Utilities.ThrowArgumentNull(nameof(createObjectFunc));  
            }

            this._createObjectFunc = createObjectFunc;
        }

        /// <summary>
        /// 检索一个指定键的对象，如果对象尚未创建将自动创建一个，容器保证同一个key返回相同的实例。
        /// </summary>
        /// <param name="key">要查询的对象标识</param>
        /// <returns>此键关联的一个对象</returns>
        public TValue GetValue(TKey key) {
            if (key == null) {
                Utilities.ThrowArgumentNull(nameof(key));
            }

            TValue value;
            //先尝试在现有对象中查询，如果找到将直接返回。
            if (this.TryGetValue(key, out value)) {
                return value;
            }

            //尝试创建一个新的对象，注意，可能造成同一个key在两个并发线程中创建两次，但我们只会采用第一个创建的。
            value = this._createObjectFunc(key);

            if (!this.TryAdd(key, value)) {
                var isOk = this.TryGetValue(key, out value);
                System.Diagnostics.Debug.Assert(isOk); //既然TryAdd失败说明已经有其他线程抢先添加了，理论上一定检索的到。
            }

            return value;
        }
    }
}
