using System.Collections.Generic;
using System.Configuration.Core.Collections;
using System.Configuration.Core.Metadata;
using System.Diagnostics;

namespace System.Configuration.Core {

    internal sealed class CombinedPart : ConfigurationObjectPart {

        internal CombinedPart(OnlyNextNode<ConfigurationObjectPart> first) {
            Debug.Assert(first != null);
            this._first = first;
        }

        //优先级较高的部件，即Depth较大的仓库中找到的部件。
        private OnlyNextNode<ConfigurationObjectPart> _first;

        /// <summary>
        /// 尝试获取属性的单个值。
        /// </summary>
        /// <param name="property">要获取的属性。</param>
        /// <param name="value">如果找到有效的定义返回值，否则返回null.</param>
        /// <returns>如果找到有效的定义返回true，否则返回false.</returns>
        public override bool TryGetLocalValue(IProperty property, out object value) {
            //程序的策略是，只要发现第一个有效值，就直接退出。
            //例如 二次开发 定义了新的值，如果检测到了就没有必要去检索基础开发的值了，反正只会采纳第一个。
            if (_first.Value.TryGetLocalValue(property, out value)) {
                return true;
            }

            var current = _first.Next;
            while (current != null) {
                if (current.Value.TryGetLocalValue(property,out value)) {
                    return true;
                }
                current = current.Next;
            } 

            return false;
        }
        
        /// <summary>
        /// 返回部件内部存储的集合属性值。
        /// </summary>
        /// <param name="property">要检索的属性</param>
        /// <param name="list">如果检索到有效的定义将返回他，否则返回null</param>
        /// <returns>如果检索到有效的定义将返回true</returns>
        public override bool TryGetLocalListValue(IProperty property, out IEnumerable<ListDifferenceItem> list) {

            //集合属性的值，是从底层逐步提交到顶层的，所以需要反转当前的可用部件。
            //但这需要建立另外一个数据链表，这里利用了递归调用实现反转搜索。
            List<ListDifferenceItem> result = null;
            IEnumerable<ListDifferenceItem> firstItems = null;

            CollectListValue(_first, property, ref result, ref firstItems);

            list = result ?? firstItems;
            return list != null;
        }

        //利用递归调用实现 反转 搜索。
        private static void CollectListValue(OnlyNextNode<ConfigurationObjectPart> current, IProperty property, ref List<ListDifferenceItem> result, ref IEnumerable<ListDifferenceItem> firstItems) {
            var next = current.Next;
            if (next != null) {
                CollectListValue(next, property, ref result, ref firstItems);
            }

            IEnumerable<ListDifferenceItem> items;
            if (current.Value.TryGetLocalListValue(property, out items)) {
                //大多数情况下，只有一个仓库会提供集合的部件，所以不到万不得已，不会创建新的List存储数据。
                if (firstItems == null) {
                    firstItems = items;
                }
                else {
                    //如果确实存在多个部件，那么回按顺序添加到集合（越底层的仓库添加的位置越靠前）。
                    if (result == null) {
                        result = new List<ListDifferenceItem>(firstItems);
                    }
                    result.AddRange(items);
                }
            }
        }

        public override IEnumerable<KeyValuePair<IProperty, object>> GetLocalValues() {
            throw new NotImplementedException();
        }

        public override IType Type {
            get {
                return _first.Value.Type;
            }
        }
    }
}
