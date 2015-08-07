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
        /// <returns>可枚举的部件集合，所有部件必须完成命名的检查，以及让所有命名字符串公用字符串引用。</returns>
        protected abstract IEnumerable<KeyValuePair<PcakagePartKey, ConfigurationObjectPart>> LoadPartsCore();

        private Dictionary<PcakagePartKey, ConfigurationObjectPart> GetParts() {
            lock (this) {
                if (_parts == null) {
                    Dictionary<PcakagePartKey, ConfigurationObjectPart> parts = new Dictionary<PcakagePartKey, ConfigurationObjectPart>(PcakagePartKey.Comparer);
                    var loadedParts = this.LoadPartsCore().GetEnumerator();
                    KeyValuePair<PcakagePartKey, ConfigurationObjectPart> current;
                    try {
                        while (loadedParts.MoveNext()) {
                            current = loadedParts.Current;
                            //派生类要在LoadPartsCore对Namespace和Name的检查，因为派生类检查效率更高，例如Namespace在Dcxml中是一个文件定义一个，也就只用检查一次。
                            //同时，也需要让Namespace和Name字符串驻留，这样就可以在检查字符串相等时使用引用判断。
                            parts.Add(current.Key, current.Value);
                        }
                    }
                    catch (ArgumentException) {
                        Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                            "在配置文件中出现重复的名称:{0}", loadedParts.Current.Key));
                    }

                    this._parts = parts;
                }
            }

            return this._parts;
        }

        private Dictionary<PcakagePartKey, ConfigurationObjectPart> _parts;

        public override sealed bool TryGetPart(string objNamespace, string name, out ConfigurationObjectPart part) {
            var parts = _parts ?? this.GetParts();
            var isOk = parts.TryGetValue(new PcakagePartKey(objNamespace, name), out part);
            if (isOk) {
                part.OpenData();
            }

            return isOk;
        }
    }
}