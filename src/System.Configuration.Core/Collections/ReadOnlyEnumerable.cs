using System.Collections;
using System.Collections.Generic;

namespace System.Configuration.Core.Collections {

    //如果需要公开Parts，简单的方法是直接返回内部字典，但是外部代码可以强转然后添加数据，感觉不好，所以建立了这个结构。
    internal struct ReadOnlyEnumerable<T> : IEnumerable<T> {
        private IEnumerable<T> _parent;
        public ReadOnlyEnumerable(IEnumerable<T> parent) {
            _parent = parent;
        }

        public IEnumerator<T> GetEnumerator() {
            return _parent.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _parent.GetEnumerator();
        }
    }
}
