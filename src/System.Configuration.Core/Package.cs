using System.Collections.Generic;

namespace System.Configuration.Core {

    /// <summary>
    /// 描述了一个包对象。
    /// </summary>
    public abstract class Package {
        private readonly string _name;
        private readonly Repository _repository;

        protected Package(string name, Repository repository) {
            if (string.IsNullOrEmpty(name)) {
                Utilities.ThrowArgumentNull(nameof(name));
            }

            if (repository == null) {
                Utilities.ThrowArgumentNull(nameof(repository));
            }

            this._name = name;
            this._repository = repository;
        }

        /// <summary>
        /// 返回包的识别名称。
        /// </summary>
        public string Name { get { return this._name; } }

        /// <summary>
        /// 返回包所在的仓库。
        /// </summary>
        public Repository Repository { get { return this._repository; } }

        /// <summary>
        /// 尝试获取指定命名空间和名称的部件
        /// </summary>
        /// <param name="fullName">检索对象的全名称</param>
        /// <param name="part">如果找到返回一个部件对象</param>
        /// <returns>如果找到返回true。</returns>
        public abstract bool TryGetPart(FullName fullName, out ConfigurationObjectPart part);

        /// <summary>
        /// 派生类重载此方法，用于返回所有的部件。
        /// </summary>
        /// <returns>可枚举的部件集合，所有部件必须完成命名的检查，以及让所有命名字符串公用字符串引用。</returns>
        public abstract IEnumerable<KeyValuePair<FullName, ConfigurationObjectPart>> GetParts();

        public override string ToString() {
            return this.Name;
        }
    }
}
