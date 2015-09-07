using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Core.Dc {
    /// <summary>
    /// 运行时使用的仓库，指定一个目录将使用二进制的*.dc文件作为仓库。具有性能好的优点。
    /// </summary>
    public class DcRepository : Repository {

        public DcRepository(string path) : this(path, null) {
        }

        public DcRepository(string path, params Repository[] dependencies) : base(dependencies) {
            if (string.IsNullOrEmpty(path)) {
                Utilities.ThrowArgumentNull(nameof(path));
            }
            this._path = path;
        }

        private string _path;
        /// <summary>
        /// 返回仓库所在的路径。
        /// </summary>
        public string RootPath {
            get { return this._path; }
        }

        protected override bool TryGetPackageCore(string packageName, out Package package) {
            var packagePath = GetPackagePath(packageName);
            if (GetPackagePathList().Contains(packagePath)) {
                package = new DcPackage(packagePath, this);
                return true;
            }

            package = null;
            return false;
        }

        private HashSet<string> _packagePathList;
        /// <summary>
        /// 返回此仓库下的所有包文件清单，用于快速比对是否存在包。
        /// </summary>
        /// <returns>一组包含所有包文件的文件路径清单</returns>
        protected virtual HashSet<string> GetPackagePathList() {
            if (_packagePathList == null) {
                _packagePathList = new HashSet<string>(PlatformUtilities.Current.GetFiles(_path, "*.dc", true),StringComparer.OrdinalIgnoreCase);
            }
            return _packagePathList;
        }

        /// <summary>
        /// 返回一个包对应的文件路径
        /// </summary>
        /// <param name="packageName">要检测的包名称。</param>
        /// <returns>一个地址，指向了包所在的位置。</returns>
        protected virtual string GetPackagePath(string packageName) {
            return PlatformUtilities.Current.CombinePath(this._path, packageName + ".dc");
        }
    }
}
