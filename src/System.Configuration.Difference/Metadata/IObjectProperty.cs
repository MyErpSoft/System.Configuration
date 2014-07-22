using System;

namespace System.Configuration.Difference.Metadata {

    public interface IObjectProperty {

        IObjectType PropertyType { get; }

        void SetValue(object obj, object value);
    }
}
