using System.Collections.Generic;
using System.Text;

namespace System.Configuration.Core {

    /// <summary>
    /// 由多个IPackageProvider组合的提供者。例如在用一个文件路径下存在*.dc和*.dcxml两种形式的配置文件。
    /// </summary>
    internal sealed class CombinedPackageProvider : IPackageProvider {

        /// <summary>
        /// 创建由多个IPackageProvider组合的提供者。例如在用一个文件路径下存在*.dc和*.dcxml两种形式的配置文件。
        /// </summary>
        /// <param name="p1">第一个提供者，不能为null</param>
        /// <param name="p2">第二个提供者，不能为null</param>
        /// <param name="providers">更多的提供者。</param>
        public CombinedPackageProvider(IPackageProvider p1, IPackageProvider p2, params IPackageProvider[] providers)
            : this(ConvertToArray(p1, p2, providers)) { }

        private static IPackageProvider[] ConvertToArray(IPackageProvider p1, IPackageProvider p2, IPackageProvider[] providers) {
            if (p1 == null) {
                Utilities.ThrowArgumentNull(nameof(p1));
            }

            if (p2 == null) {
                Utilities.ThrowArgumentNull(nameof(p2));
            }

            var count = 2 + (providers == null ? 0 : providers.Length);
            var newProviders = new IPackageProvider[count];
            newProviders[0] = p1;
            newProviders[1] = p2;
            if (providers != null && providers.Length > 0) {
                Array.Copy(providers, 0, newProviders, 2, providers.Length);
            }

            return newProviders;
        }

        /// <summary>
        /// 传入多个提供者的数组，至少包含两个及其以上的元素。构建一个合并的提供者。
        /// </summary>
        /// <param name="providers">至少包含两个及其以上的元素的数组。</param>
        public CombinedPackageProvider(IPackageProvider[] providers) {
            if (providers == null) {
                Utilities.ThrowArgumentNull(nameof(providers));
            }
            if (providers.Length < 2) {
                Utilities.ThrowArgumentException("至少包含2个实例。", nameof(providers));
            }
            
            foreach (var provider in providers) {
                if (provider == null) {
                    Utilities.ThrowArgumentNull(nameof(providers));
                }
                provider.Changed += provider_Changed;
            }
            _providers = providers;
        }

        private void provider_Changed(object sender, EventArgs e) {
            var changed = this.Changed;
            if (changed != null) {
                changed(sender, e);
            }
        }

        private readonly IPackageProvider[] _providers;

        public IEnumerable<PackageItem> GetPackageItems() {
            foreach (var privider in _providers) {
                foreach (var item in privider.GetPackageItems()) {
                    yield return item;
                }
            }
        }

        public bool TryGetLocalPackage(string packageName, Repository repository, out Package package) {
            //这个方法实际根本不会被调用，因为GetPackageNames已经直接返回了原始的提供者。    
            foreach (var provider in _providers) {
                if (provider.TryGetLocalPackage(packageName,repository,out package)) {
                    return true;
                }
            }

            package = null;
            return false;
        }

        /// <summary>
        /// 当提供者的内部文件或配置发生改变后播发此事件。
        /// </summary>
        public event EventHandler Changed;
        
        /// <summary>
        /// 输出调试信息，包含内部提供者的输出。
        /// </summary>
        /// <returns>使用多行描述提供者的信息</returns>
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach (var provider in _providers) {
                sb.AppendLine(provider.ToString());
            }

            return sb.ToString();
        }
    }
}
