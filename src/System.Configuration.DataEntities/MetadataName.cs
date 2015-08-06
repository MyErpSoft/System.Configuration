using System;

namespace System.Configuration.DataEntities {

    /// <summary>
    /// Reduced memory footprint, fast equality comparisons and lookups.
    /// </summary>
    internal sealed class MetadataName {
        private readonly string _name;

        internal MetadataName(string name) {
            this._name = name;
        }

        //No need to override the Equals and GetHashCode, Pointer to the object we want to use the default implementation of judgments
        public override string ToString() {
            return this._name;
        }
    }
}
