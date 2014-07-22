using System;

namespace System.Configuration.Difference {

    /// <summary>
    /// Define a difference record.
    /// </summary>
    public interface IDifferenceRecord {

        /// <summary>
        /// Return record type kind value.
        /// </summary>
        int TypeKind { get; }
    }
}
