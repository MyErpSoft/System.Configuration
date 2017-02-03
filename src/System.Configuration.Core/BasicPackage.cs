using System.Collections.Generic;
using System.Configuration.Core.Collections;
using System.Configuration.Core.Metadata;
using System.Globalization;

namespace System.Configuration.Core {

    /// <summary>
    /// 基础的包实现。
    /// </summary>
    public abstract class BasicPackage : Package {

        protected BasicPackage(string name, IConfigurationObjectBinder binder) : base(name, binder) { }

        /// <summary>
        /// 派生类重载此方法，用于加载所有的部件。
        /// </summary>
        /// <returns>可枚举的部件集合，所有部件必须完成命名的检查，以及让所有命名字符串公用字符串引用。</returns>
        protected abstract IEnumerable<KeyValuePair<FullName, ConfigurationObjectPart>> LoadPartsCore();

        /// <summary>
        /// 用于返回所有的部件。
        /// </summary>
        /// <returns>可枚举的部件集合，所有部件必须完成命名的检查，以及让所有命名字符串公用字符串引用。</returns>
        public override IEnumerable<KeyValuePair<FullName, ConfigurationObjectPart>> GetParts() {
            var parts = _parts ?? this.LoadParts();
            foreach (var part in parts) {
                OpenData(part.Value);
            }

            return new ReadOnlyEnumerable<KeyValuePair<FullName, ConfigurationObjectPart>>(parts);
        }

        private Dictionary<FullName, ConfigurationObjectPart> LoadParts() {
            lock (this) {
                if (_parts == null) {
                    Dictionary<FullName, ConfigurationObjectPart> parts = new Dictionary<FullName, ConfigurationObjectPart>();
                    var loadedParts = this.LoadPartsCore().GetEnumerator();
                    KeyValuePair<FullName, ConfigurationObjectPart> current;
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
                            "在同一个包配置文件 {0} 中出现重复的名称:{1}", this.Name, loadedParts.Current.Key));
                    }
                    finally {
                        if (loadedParts != null) {
                            loadedParts.Dispose();
                        }
                    } 

                    this._parts = parts;
                }
            }

            return this._parts;
        }

        private Dictionary<FullName, ConfigurationObjectPart> _parts;

        public sealed override bool TryGetPart(FullName fullName, out ConfigurationObjectPart part) {
            var parts = _parts ?? this.LoadParts();
            if (parts.TryGetValue(fullName, out part)) {
                OpenData(part);

                return true;
            }

            return false;
        }

        private void OpenData(ConfigurationObjectPart part) {
            var basicPart = part as BasicPart;
            if (basicPart != null && !basicPart.IsOpened) {
                basicPart.OpenData(this.Binder);
            }
        }
    }
}