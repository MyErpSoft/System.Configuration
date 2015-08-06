using System;
using System.Configuration.Difference.Metadata;

namespace System.Configuration.Difference {
    
    internal sealed class ObjectContainer : IObjectContainer {

        public bool TryGet(object oid, out object obj) {
            throw new NotImplementedException();
        }
        
        public IObjectType ObjectReferenceType {
            get { throw new NotImplementedException(); }
        }

        public void Register(object oid, object obj) {
            throw new NotImplementedException();
        }

    }
}
