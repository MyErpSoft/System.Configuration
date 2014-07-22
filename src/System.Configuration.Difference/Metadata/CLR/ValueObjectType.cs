using System;
using System.ComponentModel;

namespace System.Configuration.Difference.Metadata.CLR {

    internal struct ValueObjectType<T> : IConverter, IObjectType {

        public ValueObjectType(Func<T, string> toStringFunc, Func<string, T> fromStringFunc) {
            System.Diagnostics.Debug.Assert(toStringFunc != null);
            System.Diagnostics.Debug.Assert(fromStringFunc != null);
            this._toStringFunc = toStringFunc;
            this._fromStringFunc = fromStringFunc;
        }

        #region IObjectType
        public bool IsValueType {
            get { return true; }
        }

        public IConverter Converter {
            get {
                return this;
            }
        }

        public object CreateInstance() {
            Utils.ExceptionHelper.ThrowNotSupported("value type not supported.");
            return null;
        }

        public IObjectProperty GetProperty(string propertyName) {
            Utils.ExceptionHelper.ThrowNotSupported("value type not supported.");
            return null;
        } 
        #endregion

        #region Convert
        private readonly Func<T, string> _toStringFunc;
        private readonly Func<string, T> _fromStringFunc;

        public string ToString(object obj) {
            System.Diagnostics.Debug.Assert(_toStringFunc != null);
            return this._toStringFunc((T)obj);
        }

        public object FromString(string str) {
            System.Diagnostics.Debug.Assert(_fromStringFunc != null);
            return this._fromStringFunc(str);
        } 
        #endregion

        #region Equals
        public override string ToString() {
            return typeof(T).ToString();
        }

        public override bool Equals(object obj) {
            if (obj is ValueObjectType<T>) {
                return true;
            }
            return false;
        }

        public override int GetHashCode() {
            return typeof(T).GetHashCode();
        } 
        #endregion
    }
}
