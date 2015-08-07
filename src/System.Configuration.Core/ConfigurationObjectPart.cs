namespace System.Configuration.Core {

    public abstract class ConfigurationObjectPart {
        
        private bool _isOpened;

        internal void OpenData() {
            if (!_isOpened) {
                lock (this) {
                    if (!_isOpened) {
                        this.OpenDataCore();
                        _isOpened = true;
                    }
                }
            }
        }

        /// <summary>
        /// 返回此对象是否已经完成解开数据包工作。
        /// </summary>
        protected bool IsOpened {
            get {
                return this._isOpened;
            }
        }

        /// <summary>
        /// 派生类重载此方法，将原始的数据解开填充到当前数据包中。
        /// </summary>
        protected abstract void OpenDataCore();
    }
}
