using System;
using System.Configuration.Difference.Metadata;

namespace System.Configuration.Difference {

    public interface IPropertyDifferenceRecord : IDifferenceRecord {
        
        /// <summary>
        /// Return a property name.
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Return a value from record.
        /// </summary>
        /// <param name="converter">a converter instance.</param>
        /// <returns>value object.</returns>
        object GetValue(IConverter converter);
    }
}
