using System;

namespace System.Configuration.Difference {

    public class DifferenceSerializer {

        /// <summary>
        /// Create a DifferenceSerializer object.
        /// </summary>
        /// <param name="objectBinder">object binder.</param>
        public DifferenceSerializer(IObjectBinder objectBinder) {
            if (objectBinder == null) {
                Utils.ExceptionHelper.ThrowArgumentNull("objectBinder");
            }

            this._objectBinder = objectBinder;
        }

        private readonly IObjectBinder _objectBinder;
        /// <summary>
        /// Return object binder.
        /// </summary>
        public IObjectBinder ObjectBinder {
            get { return _objectBinder; }
        } 

        public void Read(ReadContext context, IDifferenceRecordReader reader) {
            if (context == null) {
                Utils.ExceptionHelper.ThrowArgumentNull("context");
            }
            if (reader == null) {
                Utils.ExceptionHelper.ThrowArgumentNull("reader");
            }

            var record = reader.Read();
            while (record != null) {
                if (!ReadRecord(context, record)) {
                    Utils.ExceptionHelper.ThrowNotSupported("record type kind not supported." + record.TypeKind);
                }

                //read next record.
                record = reader.Read();
            }
        }

        /// <summary>
        /// Read a record, you can override this method support other record type.
        /// </summary>
        /// <param name="context">read context</param>
        /// <param name="record">a record.</param>
        protected virtual bool ReadRecord(ReadContext context, IDifferenceRecord record) {
            switch (record.TypeKind) {
                case RecordTypeKind.Object: {
                    this.ReadObjectRecord(context, (IObjectDifferenceRecord)record);
                    return true;
                    }
                case RecordTypeKind.SimpleProperty: {
                    this.ReadSimplePropertyRecord(context, (IPropertyDifferenceRecord)record);
                    return true;
                    }
                case RecordTypeKind.ReferenceProperty: {
                    this.ReadReferencePropertyRecord(context, (IPropertyDifferenceRecord)record);
                    return true;
                    }
                default:
                    return false;
            }
        }

        private void ReadObjectRecord(ReadContext context, IObjectDifferenceRecord objectRecord) {
            object current;
            IObjectType dt;
            var oid = objectRecord.GetOid(context.ReferenceConverter);

            if (context.ObjectContainer.TryGet(oid, out current)) {
                //Object already exists to allow object.objectRecord.ObjectTypeName is null,so using GetObjectType.
                dt = this._objectBinder.GetObjectType(current);
            }
            else {
                //create object
                dt = this._objectBinder.BindToType(objectRecord.ObjectTypeName);
                if (dt == null) {
                    if (string.IsNullOrEmpty(objectRecord.ObjectTypeName)) {
                        Utils.ExceptionHelper.ThrowArgumentNull("objectReocord.ObjectTypeName");
                    }
                    else {
                        Utils.ExceptionHelper.ThrowNotSupported(objectRecord.ObjectTypeName);
                    }
                }
                current = dt.CreateInstance();
                context.ObjectContainer.Register(oid, current);
            }

            context.CurrentObject = current;
            context.CurrentObjectType = dt;
            context.CurrentProperty = null;
        }

        private void ReadSimplePropertyRecord(ReadContext context, IPropertyDifferenceRecord propertyRecord) {
            var property = GetProperty(context, propertyRecord);
            var value = propertyRecord.GetValue(property.PropertyType.Converter);
            property.SetValue(context.CurrentObject, value);
        }

        private void ReadReferencePropertyRecord(ReadContext context, IPropertyDifferenceRecord propertyRecord) {
            var property = GetProperty(context, propertyRecord);
            var refValue = propertyRecord.GetValue(context.ReferenceConverter); //only id
            context.ObjectManager.RecordDelayedFixup(context.CurrentObject, context.CurrentProperty, refValue);
        }

        #region Utils
        private IObjectProperty GetProperty(ReadContext context, IPropertyDifferenceRecord propertyRecord) {
            System.Diagnostics.Debug.Assert(context.CurrentObject != null && context.CurrentObjectType != null);

            var property = context.CurrentObjectType.GetProperty(propertyRecord.PropertyName);
            if (property == null) {
                if (string.IsNullOrEmpty(propertyRecord.PropertyName)) {
                    Utils.ExceptionHelper.ThrowArgumentNull("propertyRecord.PropertyName");
                }
                else {
                    Utils.ExceptionHelper.ThrowNotSupported(propertyRecord.PropertyName);
                }
            }

            context.CurrentProperty = property;
            return property;
        }

        #endregion
    }
}
