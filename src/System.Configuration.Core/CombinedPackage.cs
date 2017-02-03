using System.Collections.Generic;
using System.Configuration.Core.Collections;
using System.Configuration.Core.Metadata;
using System.Linq;
using System.Text;

namespace System.Configuration.Core {

    internal class CombinedPackage : Package
    {
        //一组差量化的包，索引越小的其深度Depth越小，意味着在检索时优先级越低。
        private readonly Package[] _packages;
        public CombinedPackage(Package[] packages, IConfigurationObjectBinder binder) :
            base(packages[0].Name, binder) {
            this._packages = packages;
        }

        public override bool TryGetPart(FullName fullName, out ConfigurationObjectPart part) {
            OnlyNextList<ConfigurationObjectPart> list = new OnlyNextList<ConfigurationObjectPart>();
            Package package;

            //优先级越高的，排在前面。
            for (int i = _packages.Length - 1; i >= 0; i--) {
                package = _packages[i];
                if (package.TryGetPart(fullName, out part)) {
                    list.Add(part);
                }
            }

            //如果仅仅找到一个零件，没有必要输出CombinedPart，毕竟他的性能不如原生的对象(多一层调用)。
            var first = list.First;
            if (first != null) {
                part = (first.Next == null) ? first.Value : new CombinedPart(first);
            }
            else {
                part = null;
            }

            //在结束时，没有调用OpenData方法，因为这是一个虚拟的包，在上面的TryGetPart时真实的包已经完成调用。
            return part != null;
        }

        public override IEnumerable<KeyValuePair<FullName, ConfigurationObjectPart>> GetParts() {
            HashSet<FullName> allPairKeys = new HashSet<FullName>();
            foreach (var package in _packages) {
                foreach (var pair in package.GetParts()) {
                    allPairKeys.Add(pair.Key);
                }
            }

            return from p in allPairKeys select new KeyValuePair<FullName, ConfigurationObjectPart>(p, GetPart(p));
        }

        private ConfigurationObjectPart GetPart(FullName fullName) {
            ConfigurationObjectPart result;
            if (this.TryGetPart(fullName,out result)) {
                return result;
            }

            Utilities.ThrowNotSupported("内部错误，GetPart失败。");
            return null;
        }

        /// <summary>
        /// 输出调试信息，包含内部提供者的输出。
        /// </summary>
        /// <returns>使用多行描述提供者的信息</returns>
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach (var package in _packages) {
                sb.AppendLine(package.ToString());
            }

            return sb.ToString();
        }
    }
}
