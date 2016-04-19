using System.Collections.Generic;

namespace System.Configuration.Core {

    /// <summary>
    /// 为仓库提供搜索Package的能力，在一个仓库中可能包含各种规范的Package。
    /// </summary>
    public interface IPackageProvider {

        /// <summary>
        /// 获取某个路径下所有的包名称。
        /// </summary>
        /// <param name="path">要检索的路径。</param>
        /// <returns>返回这个路径下所有可能的Package的名称。</returns>
        IEnumerable<string> GetPackageNames(string path);

        /// <summary>
        /// 读取一个指定名称的包对象。
        /// </summary>
        /// <param name="path">Package所在的仓库路径。</param>
        /// <param name="packageName">要加载的包对象名称。</param>
        /// <param name="runtime">package在工作时需要的运行时信息。</param>
        /// <param name="package">如果包含此package将返回他，否则为null.</param>
        /// <returns>一个新的包对象。如果没有此名称的包将返回false.</returns>       
        bool TryGetLocalPackage(string path, string packageName, ConfigurationRuntime runtime, out Package package);
    }
}
