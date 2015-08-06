namespace System.Configuration.Core.Dcxml {

    /// <summary>
    /// 一种为调试时使用的仓库，他直接使用目录作为Repository，然后一级子目录作为Package。
    /// </summary>
    public class DcxmlRepository : Repository {
        private string _path;

        public DcxmlRepository(string path)  {
            if (string.IsNullOrEmpty(path)) {
                Utilities.ThrowArgumentNull(nameof(path));
            }
            this._path = path;
        }

        /// <summary>
        /// 返回仓库所在的路径。
        /// </summary>
        public string RootPath {
            get { return this._path; }
        }

        protected override bool TryGetPackageCore(string packageName, out Package package) {
            //拼接这个字符串，变成子路径，例如在磁盘系统下：
            // _path = "c:\myRepository";
            // packageName = "DemoCompany.DemoPackage"
            // 那么包所在的路径就是 "c:\myRepository\DemoCompany.DemoPackage"
            // 基类由于已经对packageName进行了检查，所以可以避免出现： c:\myRepository\..\windows\system32 这样的安全漏洞。
            // 不支持网络路径，例如： https://app.demo.com/DemoCompany.DemoPackage
            string packagePath = GetPackagePath(packageName);

            //尝试获取此路径下的 *.dcxml 或通过清单文件获取。
            var files = GetFiles(packagePath);

            //仓库里面很可能没有这个库。
            if (files == null || files.Length == 0) {
                package = null;
                return false;
            }
            else {
                package = new DcxmlPackage(packageName, this, files);
                return true;
            }
        }

        protected virtual string[] GetFiles(string packagePath) {
            //尝试获取此路径下所有*.dcxml文件，包括子目录
            return PlatformUtilities.Current.GetFiles(this._path, "*.dcxml", true);
        }
        
        /// <summary>
        /// 返回一个包对应的路径
        /// </summary>
        /// <param name="packageName">要检测的包名称。</param>
        /// <returns>一个地址，指向了包所在的位置。</returns>
        protected virtual string GetPackagePath(string packageName) {
            return PlatformUtilities.Current.CombinePath(this._path, packageName);
        } 
    }
}
