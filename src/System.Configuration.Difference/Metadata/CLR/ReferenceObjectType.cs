using System;

namespace System.Configuration.Difference.Metadata.CLR {

    internal sealed class ReferenceObjectType : IObjectType {
        private readonly Type _clrType;

        public ReferenceObjectType(Type clrType) {
            if (clrType == null) {
                Utils.ExceptionHelper.ThrowArgumentNull("clrType");
            }
            this._clrType = clrType;
        }

        #region Equals
        public override string ToString() {
            return _clrType.ToString();
        }

        public override bool Equals(object obj) {
            var other = obj as ReferenceObjectType;
            if (other != null) {
                return other._clrType == this._clrType;
            }
            return false;
        }

        public override int GetHashCode() {
            return _clrType.GetHashCode();
        }
        #endregion

        #region IObjectType 成员

        public bool IsValueType {
            get { return false; }
        }

        public IConverter Converter {
            get { return null; }
        }

        public object CreateInstance() {
            return Activator.CreateInstance(this._clrType);
        }

        public IObjectProperty GetProperty(string propertyName) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
