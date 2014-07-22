using System;

namespace System.Configuration.Difference.Metadata {

    public interface IConverter {

        string ToString(object value);

        object FromString(string str);
    }
}
