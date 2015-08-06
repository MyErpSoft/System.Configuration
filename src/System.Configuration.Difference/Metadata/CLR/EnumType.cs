using System;

namespace System.Configuration.Difference.Metadata.CLR {

    /// <summary>
    /// Enum converter/Type. using struct.
    /// </summary>
    internal struct EnumType : IObjectType, IConverter {
        private readonly Type _enumType;

        public EnumType(Type enumType) {
            if (!enumType.IsEnum) {
                Utils.ExceptionHelper.ThrowNotSupported("must is enum");
            }
            this._enumType = enumType;
        }

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

        public string ToString(object value) {
            System.Diagnostics.Debug.Assert(this._enumType != null);
            return _enumType.GetEnumName(value);
        }

        public object FromString(string str) {
            return Enum.Parse(this._enumType, str, false);
        }

        #region Equals
        public override string ToString() {
            return this._enumType.ToString();
        }

        public override bool Equals(object obj) {
            if (obj is EnumType) {
                return ((EnumType)obj)._enumType == this._enumType;
            }
            return false;
        }

        public override int GetHashCode() {
            return this._enumType.GetHashCode();
        } 
        #endregion
    }
}
