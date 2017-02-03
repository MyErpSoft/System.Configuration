using System.Linq;
using System.Collections.Generic;

namespace System.Configuration.Core.Dcxml {

    /// <summary>
    /// 一种为调试时使用的仓库，他直接使用目录作为Repository，然后一级子目录作为Package。
    /// </summary>
    internal sealed class DcxmlPackageProvider : IPackageProvider {
        public DcxmlPackageProvider(string path) {
            if (string.IsNullOrEmpty(path)) {
                Utilities.ThrowArgumentNull(nameof(path));
            }
            this._path = path;
        }

        /// <summary>
        /// 尚未实现文件变动跟踪。
        /// </summary>
        public event EventHandler Changed;

         private string _path;
       /// <summary>
        /// 返回仓库所在的路径。
        /// </summary>
        public string Path {
            get { return this._path; }
        }
        
        public IEnumerable<PackageItem> GetPackageItems() {
            return from p in PlatformUtilities.Current.GetDirectories(_path, "*", true)
                   select new PackageItem(PlatformUtilities.Current.GetFileNameWithoutExtension(p), this);
        }

        public bool TryGetLocalPackage(string packageName, Repository repository, out Package package) {
            //拼接这个字符串，变成子路径，例如在磁盘系统下：
            // _path = "c:\myRepository";
            // packageName = "DemoCompany.DemoPackage"
            // 那么包所在的路径就是 "c:\myRepository\DemoCompany.DemoPackage"
            // 基类由于已经对packageName进行了检查，所以可以避免出现： c:\myRepository\..\windows\system32 这样的安全漏洞。
            // 不支持网络路径，例如： https://app.demo.com/DemoCompany.DemoPackage
            string packagePath = PlatformUtilities.Current.CombinePath(_path, packageName);

            //尝试获取此路径下的 *.dcxml。
            var files = PlatformUtilities.Current.GetFiles(packagePath, "*.dcxml", true);

            //仓库里面很可能没有这个库。
            if (files == null || files.Length == 0) {
                package = null;
                return false;
            }
            else {
                package = new DcxmlPackage(packageName, repository.Binder, files);
                return true;
            }
        }

        /// <summary>
        /// 返回调试输出字符串，包含路径
        /// </summary>
        /// <returns>一个类似：DcxmlPackageProvider:path 的字符串</returns>
        public override string ToString() {
            return "DcxmlPackageProvider:" + this._path;
        }
    }
}
