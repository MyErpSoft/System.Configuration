namespace System.Configuration.Core {

    /// <summary>
    /// 包提供者返回所有包清单时的单个项。
    /// </summary>
    public sealed class PackageItem {
        private readonly string _name;
        private readonly IPackageProvider _provider;
        private volatile Package _package;

        /// <summary>
        /// 实现IPackageProvider时，作为GetPackageItems的元素项。
        /// </summary>
        /// <param name="name">此包的完整名称。</param>
        /// <param name="provider">此包的提供者对象。</param>
        public PackageItem(string name, IPackageProvider provider) {
            if (string.IsNullOrEmpty(name)) {
                Utilities.ThrowArgumentNull(nameof(name));
            }
            if (provider == null) {
                Utilities.ThrowArgumentNull(nameof(provider));
            }

            this._name = name;
            this._provider = provider;
        }

        /// <summary>
        /// 此包的完整名称。
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// 此包的提供者对象。
        /// </summary>
        public IPackageProvider Provider {
            get {
                return _provider;
            }
        }

        /// <summary>
        /// Repository的内部方法，用于保证并发环境下，保证只存在一个实例。
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        internal bool TryGetPackage(Repository repository, out Package package) {
            if (_package != null) {
                package = _package;
                return true;
            }

            lock (this) {
                if (_package == null) {
                    if (_provider.TryGetLocalPackage(_name, repository, out package)) {
                        _package = package;
                    }
                }

                package = _package;
                return package != null;
            }
        }
    }
}
