namespace System.Configuration.Core {

    /// <summary>
    /// 列表中描述一个记录项。包含动作和值。
    /// </summary>
    public struct ListDifferenceItem {

        /// <summary>
        /// 创建一个 ListDifferenceItem
        /// </summary>
        /// <param name="action">一个集合记录项的动作</param>
        /// <param name="item">此记录项的值</param>
        public ListDifferenceItem(ListDifferenceAction action,object item) {
            this.Action = action;
            this.Item = item;
        }

        /// <summary>
        /// 描述了一个集合记录项的动作。
        /// </summary>
        public readonly ListDifferenceAction Action;

        /// <summary>
        /// 此记录项的值。
        /// </summary>
        public readonly object Item;
    }

    /// <summary>
    /// 描述了一个集合记录项的动作。
    /// </summary>
    public enum ListDifferenceAction {
        /// <summary>表示动作是：新增记录项，默认。</summary>
        Add = 0,

        /// <summary>表示动作是：删除记录项。</summary>
        Remove = 1
    }
}
