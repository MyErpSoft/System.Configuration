using System;

namespace System.Configuration.Difference.Metadata {

    public interface IObjectType {

        /// <summary>
        /// return true if object is struct,else return false.
        /// </summary>
        /// <remarks>
        /// <para>ex,int type return true,complex type is return false.</para>
        /// </remarks>
        bool IsValueType { get; }

        /// <summary>
        /// Return a IConverter instance if IsValueType is true,else return null.
        /// </summary>
        IConverter Converter { get; }

        object CreateInstance();

        IObjectProperty GetProperty(string propertyName);
    }
}
