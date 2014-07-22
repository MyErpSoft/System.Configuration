using System;
using System.Configuration.Difference.Metadata;

namespace System.Configuration.Difference {

    //TODO:not imp
    public class ReadContext {

        public ReadContext(IObjectContainer objectContainer) {

            if (objectContainer == null) {
                this._objectContainer = new ObjectContainer();
            }
            else {
                this._objectContainer = objectContainer;
            }

            this._refConverter = this._objectContainer.ObjectReferenceType.Converter;
            System.Diagnostics.Debug.Assert(this._refConverter != null);

            this._objectManager = new ObjectManager();
        }

        private readonly IObjectContainer _objectContainer;
        private readonly IConverter _refConverter; //cache;
        public IObjectContainer ObjectContainer {
            get { return this._objectContainer; }
        }

        public IConverter ReferenceConverter {
            get { return this._refConverter; }
        }

        private readonly ObjectManager _objectManager;
        public ObjectManager ObjectManager {
            get { return this._objectManager; }
        }

        #region Current info
       
        private object _currentObject;
        public object CurrentObject {
            get { return this._currentObject; }
            internal set {
                if (object.ReferenceEquals(this._currentObject,value)) {
                    this._currentObject = value;
                    this.OnCurrentObjectChanged();
                }
            }
        }

        /// <summary>
        /// You can override this method,Invoke currentObject RaiseOnDeserializing
        /// </summary>
        protected virtual void OnCurrentObjectChanged() {
        }

        private IObjectType _currentObjectType;
        public IObjectType CurrentObjectType {
            get { return this._currentObjectType; }
            internal set { this._currentObjectType = value; }
        }

        private IObjectProperty _currentProperty;
        public IObjectProperty CurrentProperty {
            get { return this._currentProperty; }
            internal set { this._currentProperty = value; }
        }
        #endregion
    }
}
