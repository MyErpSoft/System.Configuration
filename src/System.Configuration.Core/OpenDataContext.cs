using System.Configuration.Core.Metadata;

namespace System.Configuration.Core {

    /// <summary>
    /// 为打开数据提供上下文数据。
    /// </summary>
    public class OpenDataContext {

        public OpenDataContext(IConfigurationObjectBinder binder, QualifiedName key) {
            this._binder = binder;
            this._key = key;
        }

        private readonly IConfigurationObjectBinder _binder;
        public IConfigurationObjectBinder Binder {
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

        private IType _type;
        /// <summary>
        /// 返回/设置当前对象的类型信息。
        /// </summary>
        public IType Type {
            get { return _type; }
            set { _type = value; }
        }
    }
}
