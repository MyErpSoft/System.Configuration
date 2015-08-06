using System.Collections.Generic;
using System.Globalization;

namespace System.Configuration.Core {

    /// <summary>
    /// 基础的包实现。
    /// </summary>
    public abstract class BasicPackage : Package {

        protected BasicPackage(string name, Repository repository) : base(name, repository) { }

        /// <summary>
        /// 派生类重载此方法，用于加载所有的部件。
        /// </summary>
        /// <returns>可枚举的部件集合。</returns>
        protected abstract IEnumerable<ConfigurationObjectPart> LoadPartsCore();

        private Dictionary<PartKey, ConfigurationObjectPart> GetParts() {
            if (_parts == null) {
                lock (this) {
                    if (_parts == null) {
                        Dictionary<PartKey, ConfigurationObjectPart> parts = new Dictionary<PartKey, ConfigurationObjectPart>(new PartKeyEqualityComparer());
                        var loadedParts = this.LoadPartsCore().GetEnumerator();
                        ConfigurationObjectPart current = null;
                        try {
                            while (loadedParts.MoveNext()) {
                                current = loadedParts.Current; //底层的实现要完成 参数检查
                                parts.Add(new PartKey() { Namespace = current.Namespace, Name = current.Name }, current);
                            }
                        }
                        catch (ArgumentException) {
                            Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                                "在配置文件中出现重复的名称:{0}", current));
                        }

                        this._parts = parts;
                    }
                }
            }

            return this._parts;
        }

        private Dictionary<PartKey, ConfigurationObjectPart> _parts;

        public override sealed bool TryGetPart(string objNamespace, string name, out ConfigurationObjectPart part) {
            var parts = this.GetParts();
            return parts.TryGetValue(new PartKey() { Namespace = objNamespace, Name = name }, out part);
        }

        #region PartKey
        private struct PartKey {
            public string Namespace;
            public string Name;

            public override string ToString() {
                return string.IsNullOrEmpty(this.Namespace) ? this.Name : this.Namespace + "." + this.Name;
            }
        }

        private sealed class PartKeyEqualityComparer : IEqualityComparer<PartKey> {
            public bool Equals(PartKey x, PartKey y) {
                return x.Name == y.Name && x.Namespace == y.Namespace;
            }

            public int GetHashCode(PartKey obj) {
                return (obj.Namespace == null ? 0 : obj.Namespace.GetHashCode()) ^
                    (obj.Name == null ? 0 : obj.Name.GetHashCode());
            }
        }
        #endregion
    }
}