using System.Collections.Generic;
using System.Text;

namespace System.Configuration.Core.Metadata {

    /// <summary>
    /// 由多个Binder合并的IConfigurationObjectBinder实现。例如默认的有CLR和接口的绑定器。
    /// </summary>
    internal sealed class CombinedConfigurationObjectBinder : IConfigurationObjectBinder {
        private readonly IConfigurationObjectBinder[] _binders;
        private readonly Dictionary<string, IConfigurationObjectBinder> _dictByName; 

        public CombinedConfigurationObjectBinder(IConfigurationObjectBinder[] binders) {
            if (binders == null) {
                Utilities.ThrowArgumentNull(nameof(binders));
            }
            if (binders.Length < 2) {
                Utilities.ThrowArgumentException("至少包含2个实例。", nameof(binders));
            }

            //以SupportedName为键（例如："clr-namespace"），具体的binder为值。
            //可能出现一个Binder实例支持多个ProviderName，也可能出现多个Binder支持同一个ProviderName，以数组靠前的优先。
            _dictByName = new Dictionary<string, IConfigurationObjectBinder>(binders.Length);
            foreach (var binder in binders) {
                if (binder == null) {
                    Utilities.ThrowArgumentNull(nameof(binders));
                }

                foreach (var name in binder.SupportedNames) {
                    var supportedName = name ?? string.Empty;

                    IConfigurationObjectBinder selectedBinder;
                    if (!_dictByName.TryGetValue(supportedName,out selectedBinder)) {
                        _dictByName.Add(supportedName, binder);
                    }
                    else {
                        //出现多个Binder支持同一个ProviderName，以数组靠前的优先。
                        ArrayBinder arrayBinder = selectedBinder as ArrayBinder;
                        if (arrayBinder == null) {
                            arrayBinder = new ArrayBinder(supportedName);
                            arrayBinder.Add(selectedBinder);
                            arrayBinder.Add(binder);
                            _dictByName[supportedName] = arrayBinder;
                        }
                        else {
                            arrayBinder.Add(selectedBinder);
                        }
                    }
                }
                
            }

            this._binders = binders;
        }

        public IEnumerable<string> SupportedNames {
            get {
                return _dictByName.Keys;
            }
        }

        public bool TryBindToType(ObjectTypeQualifiedName name, out IType type) {
            IConfigurationObjectBinder selectedBinder;
            if (_dictByName.TryGetValue(name.ProviderName, out selectedBinder)) {
                return selectedBinder.TryBindToType(name, out type);
            }

            type = null;
            return false;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach (var binder in _binders) {
                sb.AppendLine(binder.ToString() + ";");
            }
            return sb.ToString();
        }

        #region 内部用于处理同一个ProviderName有多个Binder的情况。
        private sealed class ArrayBinder : IConfigurationObjectBinder {
            public ArrayBinder(string supportedName) {
                this._supportedName = supportedName;
            }

            private readonly string _supportedName;
            public IEnumerable<string> SupportedNames {
                get { yield return _supportedName; }
            }

            private IConfigurationObjectBinder[] _binders;
            internal void Add(IConfigurationObjectBinder binder) {
                if (_binders == null) {
                    _binders = new IConfigurationObjectBinder[] { binder };
                }
                else {
                    var newArray = new IConfigurationObjectBinder[_binders.Length + 1];
                    _binders.CopyTo(newArray, 0);
                    newArray[newArray.Length - 1] = binder;
                    _binders = newArray;
                }
            }

            public bool TryBindToType(ObjectTypeQualifiedName name, out IType type) {
                foreach (var binder in _binders) {
                    if (binder.TryBindToType(name, out type)) {
                        return true;
                    }
                }

                type = null;
                return false;
            }
        } 
        #endregion
    }
}
