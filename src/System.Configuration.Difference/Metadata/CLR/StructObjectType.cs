using System;
using System.ComponentModel;

namespace System.Configuration.Difference.Metadata.CLR {

    internal struct StructObjectType : IConverter, IObjectType {

        public StructObjectType(Type structType) {
            if (structType == null) {
                Utils.ExceptionHelper.ThrowArgumentNull("structType");
            }
            if (!structType.IsValueType) {
                Utils.ExceptionHelper.ThrowNotSupported("must is struct");
            }
            this._clrType = structType;
            this._converter = TypeDescriptor.GetConverter(structType);
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
        private readonly Type _clrType;
        private readonly TypeConverter _converter;

        public string ToString(object obj) {
            System.Diagnostics.Debug.Assert(_converter != null);
            return this._converter.ConvertToInvariantString(obj);
        }

        public object FromString(string str) {
            System.Diagnostics.Debug.Assert(_converter != null);
            return this._converter.ConvertFromInvariantString(str);
        } 
        #endregion

        #region Equals
        public override string ToString() {
            return _clrType.ToString();
        }

        public override bool Equals(object obj) {
            if (obj is StructObjectType) {
                return ((StructObjectType)obj)._clrType == this._clrType;
            }
            return false;
        }

        public override int GetHashCode() {
            return _clrType.GetHashCode();
        } 
        #endregion

    }
}

