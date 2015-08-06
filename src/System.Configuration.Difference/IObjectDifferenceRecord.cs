using System;
using System.Configuration.Difference.Metadata;

namespace System.Configuration.Difference {

    /// <summary>
    /// Define a object difference record.
    /// </summary>
    public interface IObjectDifferenceRecord : IDifferenceRecord {

        /// <summary>
        /// Return object record id.
        /// </summary>
        /// <remarks>
        /// <para>Why return is object ,because oid can is guid or int.</para>
        /// </remarks>
        object GetOid(IConverter converter);

        /// <summary>
        /// Return object type name if create object.
        /// </summary>
        /// <remarks>
        /// <para>object type name can using 'namespace.classname,assembly'. please see IObjectBinder.</para>
        /// </remarks>
        string ObjectTypeName { get; }
    }
}
