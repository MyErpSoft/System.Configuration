using System.Collections.Generic;

namespace System.Configuration.Core {

    /// <summary>
    /// 为仓库提供搜索Package的能力，在一个仓库中可能包含各种规范的Package。
    /// </summary>
    public interface IPackageProvider {

        /// <summary>
        /// 获取所有的包名称。
        /// </summary>
        /// <returns>返回所有可能的Package的名称，以及他们的提供者对象。</returns>
        IEnumerable<PackageItem> GetPackageItems();

        /// <summary>
        /// 读取一个指定名称的包对象。
        /// </summary>
        /// <param name="packageName">要加载的包对象名称。</param>
        /// <param name="repository">package所在的仓库对象。</param>
        /// <param name="package">如果包含此package将返回他，否则为null.</param>
        /// <returns>一个新的包对象。如果没有此名称的包将返回false.</returns>       
        bool TryGetLocalPackage(string packageName, Repository repository, out Package package);

        /// <summary>
        /// 当提供者检测到Package清单发生变动时播发此事件，以便Repository清空内部的缓存信息。
        /// </summary>
        event EventHandler Changed;
    }
}
