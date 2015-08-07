using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Configuration.Core
{
    internal class CombinedPackage : Package
    {
        //一组差量化的包，索引越小的其深度Depth越小，意味着在检索时优先级越低。
        private readonly Package[] _packages;
        public CombinedPackage(Package[] packages) :
            base(packages[0].Name,packages[0].Repository){
            this._packages = packages;
        }

        public override bool TryGetPart(string objNamespace, string name, out ConfigurationObjectPart part) {
            CombinedPart first = null;
            CombinedPart last = null;
            Package package;

            //优先级越高的，排在前面。
            for (int i = _packages.Length - 1; i >= 0; i--) {
                package = _packages[i];
                if (package.TryGetPart(objNamespace, name, out part)) {
                    last = new CombinedPart(part, last);
                    if (first == null) {
                        first = last;
                    }
                }
            }

            //如果仅仅找到一个零件，没有必要输出CombinedPart，毕竟他的性能不如原生的对象(多一层调用)。
            if (first != null) {
                part = (first.Next == null) ? first.Value : first;
            }
            else {
                part = null;
            }

            //在结束时，没有调用OpenData方法，因为这是一个虚拟的包，在上面的TryGetPart时真实的包已经完成调用。
            return part != null;
        }
    }
}
