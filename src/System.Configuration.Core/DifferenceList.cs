using System.Collections;
using System.Collections.Generic;

namespace System.Configuration.Core {

    internal class DifferenceList {

        public static IList GetList(ConfigurationWorkspace workspace, IEnumerable<ListDifferenceItem> items) {
            List<object> list = new List<object>();
            HashSet<object> hashSet = null;
            int listSearchCount = 0;

            //我们假设很少出现Remove动作，这样一次性构建集合即可。
            //当存在较多次数的Remove,会先存储到HashSet，用于快速的删除。
            //items中动作的顺序，已经在差量化(CombinedPart)、派生关系（ConfigurationObject）中排序完毕。

            foreach (var item in items) {
                if (item.Action == ListDifferenceAction.Add) {
                    if (list != null) {
                        list.Add(item.Item);
                    }
                    else {
                        hashSet.Add(item.Item);
                    }
                }
                else if(item.Action == ListDifferenceAction.Remove) {
                    if (listSearchCount < 2) {
                        //如果未发生较多的查询，使用较慢的Remove
                        list.Remove(item.Item);
                        listSearchCount++;
                    }
                    else {
                        if (hashSet == null) {
                            hashSet = new HashSet<object>(list);
                            list = null;
                        }

                        hashSet.Remove(item.Item);
                    }
                }
                else {
                    Utilities.ThrowNotSupported("action is error");
                }
            }

            if (hashSet != null) {
                list = new List<object>(hashSet.Count);
                foreach (var item in hashSet) {
                    list.Add(item);
                }
            }

            //现有列表中的项，可能还是对象指针，需要转换为实际的对象
            for (int i = 0; i < list.Count; i++) {
                list[i] = GetListItem(workspace, list[i]);
            }

            return list;
        }

        private static object GetListItem(ConfigurationWorkspace workspace, object item) {
            if (item is ObjectPtr) {
                ObjectPtr refValue = (ObjectPtr)item;
                return workspace.GetObject(refValue.Name);
            }

            return item;
        }
    }
}
