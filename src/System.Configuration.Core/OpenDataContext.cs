using System.Configuration.Core.Metadata;

namespace System.Configuration.Core {

    public class OpenDataContext {

        public OpenDataContext(ConfigurationObjectBinder binder, QualifiedName key) {
            this._binder = binder;
            this._key = key;
        }

        private readonly ConfigurationObjectBinder _binder;
        public ConfigurationObjectBinder Binder {
            get {
                return _binder;
            }
        }

        private readonly QualifiedName _key;
        /// <summary>
        /// 检索的对象键。
        /// </summary>
        public QualifiedName Key {
            get { return _key; }
        }

    }
}
