using System.Collections;
using System.Collections.Generic;
using System.Configuration.Core.Metadata;

namespace System.Configuration.Core {
    /// <summary>
    /// 一个实际可以使用的配置对象。
    /// </summary>
    public class ConfigurationObject
    {
        /// <summary>
        /// 创建一个新的配置对象。
        /// </summary>
        /// <param name="workspace">对象的工作空间。</param>
        /// <param name="name">要创建对象的键。</param>
        /// <param name="part">配置需要的零件信息</param>
        public ConfigurationObject(ConfigurationWorkspace workspace, QualifiedName name, ConfigurationObjectPart part) {
            this._workspace = workspace;
            this._name = name;
            this._part = part;
        }

        private ConfigurationObject() { }

        private readonly ConfigurationWorkspace _workspace;
        /// <summary>
        /// 返回对象的工作空间。
        /// </summary>
        public ConfigurationWorkspace Workspace {
            get { return _workspace; }
        }

        private readonly QualifiedName _name;
        /// <summary>
        /// 要创建对象的键。
        /// </summary>
        public QualifiedName Name {
            get { return _name; }
        }

        private ConfigurationObjectPart _part;
        /// <summary>
        /// 配置需要的零件信息
        /// </summary>
        public ConfigurationObjectPart Part {
            get { return _part; }
        }

        /// <summary>
        /// 返回指定属性的值。
        /// </summary>
        /// <param name="property">要检索的属性</param>
        /// <returns>此属性的值。</returns>
        public object GetSimpleValue(IProperty property) {
            return this.GetValueCore(property);
        }

        public object GetReferenceValue(IProperty property) {
            //负责引用指针的转换
            var value = this.GetValueCore(property);

            if (value is ObjectPtr) {
                ObjectPtr refValue = (ObjectPtr)value;
                value = _workspace.GetObject(refValue.Name);
            }

            return value;
        }

        public IList GetListValue(IProperty property) {
            var value = this.GetListValueCore(property);
            IEnumerable<ListDifferenceItem> items = value as IEnumerable<ListDifferenceItem>;
            if (items != null) {
                return DifferenceList.GetList(this._workspace, items);
            }

            return null;
        }

        protected virtual object GetValueCore(IProperty property) {
            if (property == null) {
                Utilities.ThrowArgumentNull(nameof(property));
            }

            object value;
            //重复的代码是希望少执行一些代码
            if (_part.TryGetLocalValue(property, out value)) {
                return value;
            }

            var current = this.Base;
            while (current != null) {
                if (current._part.TryGetLocalValue(property, out value)) {
                    return value;
                }

                //从基类搜索
                current = current.Base;
            }

            return property.DefaultValue;
        }

        protected virtual IEnumerable<ListDifferenceItem> GetListValueCore(IProperty property) {
            if (property == null) {
                Utilities.ThrowArgumentNull(nameof(property));
            }

            List<ListDifferenceItem> result = null;
            IEnumerable<ListDifferenceItem> firstItems = null;

            CollectListValue(this, property, ref result, ref firstItems);

            return result ?? (firstItems ?? new OnlyNextList<ListDifferenceItem>());
        }

        //利用递归调用实现 反转 搜索。
        private static void CollectListValue(ConfigurationObject current, IProperty property, ref List<ListDifferenceItem> result, ref IEnumerable<ListDifferenceItem> firstItems) {
            var next = current.Base;
            if (next != null) {
                CollectListValue(next, property, ref result, ref firstItems);
            }

            IEnumerable<ListDifferenceItem> items;
            if (current._part.TryGetLocalListValue(property, out items)) {
                if (firstItems == null) {
                    firstItems = items;
                }
                else {
                    if (result == null) {
                        result = new List<ListDifferenceItem>(firstItems);
                    }
                    result.AddRange(items);
                }
            }
        }

        private ConfigurationObject _base;
        /// <summary>
        /// 返回此对象的基类信息。
        /// </summary>
        protected ConfigurationObject Base {
            get {
                if (ReferenceEquals(_base, null)) {
                    var baseName = _part.Base.Name;
                    _base = _workspace.GetConfigurationObject(baseName);

                    if (ReferenceEquals(_base, null)) {
                        _base = ConfigurationObject.Null; //给_base加入已缓存标志。
                    }
                }

                return ReferenceEquals(_base, ConfigurationObject.Null) ? null : _base;
            }
        }

        private static readonly ConfigurationObject Null = new ConfigurationObject();
    }
}
