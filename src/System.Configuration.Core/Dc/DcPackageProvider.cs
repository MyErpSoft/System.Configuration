using System.Linq;
using System.Collections.Generic;

namespace System.Configuration.Core.Dc {
    /// <summary>
    /// 运行时使用的仓库提供者，指定一个目录将使用二进制的*.dc文件作为仓库。具有性能好的优点。
    /// </summary>
    internal sealed class DcPackageProvider : IPackageProvider {
        
        public IEnumerable<string> GetPackageNames(string path) {
            return from p in PlatformUtilities.Current.GetFiles(path, "*.dc", true) select PlatformUtilities.Current.GetFileNameWithoutExtension(p);
        }

        public bool TryGetLocalPackage(string path, string packageName, ConfigurationRuntime runtime, out Package package) {
            //返回一个包对应的文件路径
            var packagePath = PlatformUtilities.Current.CombinePath(path, packageName + ".dc");
            package = new DcPackage(packagePath, runtime);
            return true;
        }
    }
}
