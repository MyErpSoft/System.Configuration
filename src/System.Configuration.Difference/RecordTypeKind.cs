using System;

namespace System.Configuration.Difference {

    /// <summary>
    /// Record's type enum.
    /// </summary>
    /// <remarks>
    /// <para>Why not define enum,because it can be extended.</para>
    /// </remarks>
    public static class RecordTypeKind {

        /// <summary>
        /// Record is object.
        /// </summary>
        public const int Object = 1;

        /// <summary>
        /// Record is simple property, value is a struct.
        /// </summary>
        public const int SimpleProperty = 2;

        /// <summary>
        /// Record is reference property.
        /// </summary>
        public const int ReferenceProperty = 3;

        /// <summary>
        /// Record is array/collection property.
        /// </summary>
        public const int ArrayProperty = 4;

        /// <summary>
        /// Record is array item property.
        /// </summary>
        public const int ArrayItemProperty = 5;

    }
}
