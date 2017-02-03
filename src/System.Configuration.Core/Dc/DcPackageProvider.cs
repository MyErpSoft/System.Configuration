using System.Linq;
using System.Collections.Generic;

namespace System.Configuration.Core.Dc {
    /// <summary>
    /// 运行时使用的仓库提供者，指定一个目录将使用二进制的*.dc文件作为仓库。具有性能好的优点。
    /// </summary>
    internal sealed class DcPackageProvider : IPackageProvider {
        public DcPackageProvider(string path) {
            if (string.IsNullOrEmpty(path)) {
                Utilities.ThrowArgumentNull(nameof(path));
            }
            this._path = path;
        }

        private string _path;
        /// <summary>
        /// 返回仓库所在的路径。
        /// </summary>
        public string Path {
            get { return this._path; }
        }
        
        /// <summary>
        /// 尚未实现文件变动跟踪。
        /// </summary>
        public event EventHandler Changed;

        public IEnumerable<PackageItem> GetPackageItems() {
            return from p in PlatformUtilities.Current.GetFiles(_path, "*.dc", true)
                   select new PackageItem(PlatformUtilities.Current.GetFileNameWithoutExtension(p), this);
        }
        
        public bool TryGetLocalPackage(string packageName, Repository repository, out Package package) {
            //返回一个包对应的文件路径
            var packagePath = PlatformUtilities.Current.CombinePath(_path, packageName + ".dc");
            package = new DcPackage(packagePath, repository.Binder);
            return true;
        }

        /// <summary>
        /// 返回调试输出字符串，包含路径
        /// </summary>
        /// <returns>一个类似：DcPackageProvider:path 的字符串</returns>
        public override string ToString() {
            return "DcPackageProvider:" + this._path;
        }
    }
}
