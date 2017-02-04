using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration.Core.Metadata;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace System.Configuration.Core {

    /// <summary>
    /// 类似Nuget的Repository，或者.net的Framework的概念，是Package的承载容器。
    /// </summary>
    /// <remarks>
    /// <para>此类的职责是，作为Package的承载容器，通过名称可以检索到Package。</para>
    /// <para>其次，提供库的依赖管理，允许在检索时检索到差量的包。</para>
    /// </remarks>
    public class Repository {

        #region 构造函数
        public Repository(string path) : this(Utilities.CreateDefaultPackageProvider(path), Utilities.DefaultBinder, null) {
        }

        public Repository(string path, params Repository[] dependencies) : this(Utilities.CreateDefaultPackageProvider(path), Utilities.DefaultBinder, dependencies) {
        }

        public Repository(IPackageProvider provider, IConfigurationObjectBinder binder, params Repository[] dependencies) {
            if (provider == null) {
                Utilities.ThrowArgumentNull(nameof(provider));
            }
            _packageProvider = provider;
            _packageProvider.Changed += packageProvider_Changed;

            if (binder == null) {
                Utilities.ThrowArgumentNull(nameof(binder));
            }
            this._binder = binder;

            if (dependencies == null || dependencies.Length == 0) {
                this._dependencies = _emptyRepositories;
                this._allRepositories = null;
            }
            else {
                this._dependencies = new ReadOnlyCollection<Repository>(dependencies);
                this._allRepositories = GetAllRepositories();
            }

            this._loadedPackages = new ObjectContainer<string, Package>(this.LoadPackage);
        }
        #endregion

        #region 仓库依赖管理
        private int? _depth;
        //依赖深度。用于排序
        private int Depth {
            get {
                if (_depth == null) {
                    //如果没有依赖，那么深度为0，否则是底层依赖的最大值+1
                    _depth = (this._dependencies.Count == 0) ? 0 : (from p in _dependencies select p.Depth).Max() + 1;
                }

                return _depth.Value;
            }
        }

        private static readonly ReadOnlyCollection<Repository> _emptyRepositories = new ReadOnlyCollection<Repository>(new Repository[0]);

        //仓库的依赖性被设计成只读，且只能构造传入后就不能再变更了，主要考虑：
        //1、在实际运行时，变更依赖性会遇到很多不确定性的问题；
        //2、并发环境下，变更依赖性意味着非常小心处理集合的变化；
        //3、当基础仓库的依赖性发生变化时，意味着所有依赖他的仓库都同步变化，这个给程序带来巨大的复杂性。
        //4、依赖性变化了，那么已经加载的Package和配置对象如何处理？
        private readonly ReadOnlyCollection<Repository> _dependencies;
        /// <summary>
        /// 返回此仓库直接依赖的仓库集合。
        /// </summary>
        public ReadOnlyCollection<Repository> Dependencies {
            get { return _dependencies; }
        }
        private readonly Repository[] _allRepositories;

        /// <summary>
        /// 返回指定仓库所依赖的所有仓库，这些仓库经过依赖性排序。
        /// </summary>
        private Repository[] GetAllRepositories() {
            //这里我们没有处理仓库依赖的“死循环”问题，因为我们通过构造函数传入依赖性的，所以根本写不出来“死循环”的依赖。
            //搜索所有的仓库，并且保证没有重复的项
            var list = new HashSet<Repository>();
            AddTo(this, list);

            //计算他的依赖深度，并按照此排序
            return (from p in list
                    orderby p.Depth
                    select p).ToArray();
        }

        private void AddTo(Repository repository, HashSet<Repository> list) {
            if (list.Add(repository)) {
                if (repository._binder != this._binder) {
                    Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                        "依赖的知识库 {0} 必须公用同一个运行时。", repository));
                }
            }

            if (repository._dependencies.Count > 0) {
                foreach (var item in repository._dependencies) {
                    AddTo(item, list);
                }
            }
        }

        #endregion

        #region 属性
        private readonly IPackageProvider _packageProvider;
        /// <summary>
        /// 返回当前仓库关联的Package提供者。
        /// </summary>
        public IPackageProvider PackageProvider {
            get { return _packageProvider; }
        }

        private readonly IConfigurationObjectBinder _binder;
        /// <summary>
        /// 返回当前仓库关联的类型绑定器。
        /// </summary>
        public IConfigurationObjectBinder Binder {
            get { return _binder; }
        }
        #endregion

        //缓存了packageName对应的Package的映射，
        private readonly ObjectContainer<string, Package> _loadedPackages;

        /// <summary>
        /// 加载指定名称的包对象。
        /// </summary>
        /// <param name="packageName">要加载的包的名称。</param>
        /// <returns>与此包名称对应的包对象。</returns>
        public Package GetPackage(string packageName) {
            //所有仓库已经提前按照依赖性排序，所以可以直接从头到尾检索包，检索出来的包也就自动按照依赖性排序的。
            return _loadedPackages.GetValue(packageName);
        }

        //通过给定的包名称，加载对应的包，如果找不到将返回null.
        private Package LoadPackage(string packageName) {
            if (!Utilities.VerifyNameWithNamespace(packageName)) {
                Utilities.ThrowArgumentException("packageName只能包含字母、数字或_，且不能使用数字开头。", nameof(packageName));
            }

            Package result;

            if (_allRepositories == null) {
                if (this.TryGetLocalPackage(packageName, out result)) {
                    return result;
                }
            }
            else {
                Package firstPackage = null;
                List<Package> list = null;

                foreach (var item in _allRepositories) {
                    if (item.TryGetLocalPackage(packageName, out result)) {

                        //通常情况下，没有差量包，所以我们就避免创建List<Package>对象，可以直接返回唯一的包。
                        if (firstPackage == null) {
                            firstPackage = result;
                        }
                        else {
                            //如果存在差量包，就放在一起。
                            if (list == null) {
                                list = new List<Package>(2) { firstPackage, result };
                            }
                            else {
                                list.Add(result);
                            }
                        }
                    }
                }//end foreach

                //由于允许多个仓库包含同一个名称的包，他们之间是差量的关系，我们会包装成一个虚拟的包。
                //大多数情况下是没有差量的包，所以每次总是foreach一个数组，而这个数组只有一个记录，对于计算次数极其多的方法是不合算的。
                if (list != null) {
                    return new CombinedPackage(list.ToArray(), _binder);
                }
                else if (firstPackage != null) {
                    return firstPackage;
                }
            }

            Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                "未能找到包：{0}", packageName));
            return null;
        }

        /// <summary>本仓库所有的本地Package清单</summary>
        // 注意与_loadedPackages很不同：
        // _localPackages是本地文件所有Package（可能没有加载），而_loadedPackages是已经加载过的。
        private volatile Dictionary<string, PackageItem> _localPackages;

        /// <summary>
        /// 读取一个指定名称的本地包对象。
        /// </summary>
        /// <param name="packageName">要加载的包对象名称。</param>
        /// <returns>一个新的包对象。如果没有此名称的包将</returns>       
        protected bool TryGetLocalPackage(string packageName, out Package package) {
            //检查是否存在本地package的清单缓存，这里使用到无锁设计。
            var localPackages = _localPackages;
            if (localPackages == null) {
                Interlocked.CompareExchange(ref _localPackages, GetLocalPackages(), localPackages);
                localPackages = _localPackages;
            }

            PackageItem cacheItem;
            if (localPackages.TryGetValue(packageName, out cacheItem)) {
                return cacheItem.TryGetPackage(this, out package);
            }

            package = null;
            return false;
        }

        private Dictionary<string, PackageItem> GetLocalPackages() {
            //注意，PackageName是忽略大小写的。
            var dict = new Dictionary<string, PackageItem>(StringComparer.OrdinalIgnoreCase);
            var items = _packageProvider.GetPackageItems();
            if (items != null) {
                foreach (var item in items) {
                    //当同一个仓库，存在两个相同名称的Package时，会优先保留Provider靠前的那个。
                    //例如在某个仓库中，包含Dcxml形式的目录，也包含对应的dc文件，我们将仅保留dcxml形式，这也为调试提供了方便。
                    if (!string.IsNullOrEmpty(item.Name) && !dict.ContainsKey(item.Name)) {
                        dict.Add(item.Name, item);
                    }
                }
            }

            return dict;
        }

        private void packageProvider_Changed(object sender, EventArgs e) {
            _localPackages = null;
            _loadedPackages.Clear();
        }

        /// <summary>
        /// 返回调试时需要的信息，包括库所在的路径
        /// </summary>
        /// <returns>一个描述字符串。</returns>
        public override string ToString() {
            return "Repository" + Environment.NewLine + this._packageProvider.ToString();
        }

    }
}
