using System;
using System.Configuration.Difference.Metadata;

namespace System.Configuration.Difference {
    
    //TODO:not imp
    public interface IObjectBinder {

        IObjectType GetObjectType(object obj);

        IObjectType BindToType(string typeName);
    }
}
