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
        /// <param name="objNamespace">部件的命名空间</param>
        /// <param name="name">部件的名称</param>
        /// <param name="part">如果找到返回一个部件对象</param>
        /// <returns>如果找到返回true。</returns>
        public abstract bool TryGetPart(string objNamespace, string name, out ConfigurationObjectPart part);

        public override string ToString() {
            return this.Name;
        }
    }
}
