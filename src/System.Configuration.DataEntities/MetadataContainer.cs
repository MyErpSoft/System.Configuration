using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Globalization;

namespace System.Configuration.DataEntities {

    /// <summary>
    /// Storage container for all metadata, is the highest level of metadata.
    /// </summary>
    public class MetadataContainer {

        private readonly MetadataFullNameContainer _fullNameContainer;
        private readonly MetadataNameContainer _nameContainer;

        /// <summary>
        /// Create MetadataContainer instance.
        /// </summary>
        public MetadataContainer(bool ignoreCase = false) {
            if (ignoreCase) {
                this._nameContainer = new MetadataNameContainer();
            }
            else {
                this._nameContainer = new MetadataNameContainer(StringComparer.OrdinalIgnoreCase);
            }

            this._fullNameContainer = new MetadataFullNameContainer(this._nameContainer);
            this._assemblies = new ConcurrentDictionary<MetadataName, MetadataAssembly>();
        }

        private readonly ConcurrentDictionary<MetadataName, MetadataAssembly> _assemblies;

        protected virtual MetadataAssembly CreateAssembly(string codeBase) {
            if (string.IsNullOrEmpty(codeBase)) {
                throw new ArgumentNullException("codeBase");
            }

            var name = this._nameContainer.GetName(codeBase);
            var assembly = new MetadataAssembly(name, this);
            if (this._assemblies.TryAdd(name, assembly)) {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture, "Configure Assembly {0} is already loaded in the current container.", codeBase), "codeBase");
            }

            return assembly;
        }

        internal MetadataAssembly CreateAssemblyForTest(string codeBase) {
            return this.CreateAssembly(codeBase);
        }

        public MetadataAssembly[] GetAssemblies() {
            var pairs = this._assemblies.ToArray();
            var result = new MetadataAssembly[pairs.Length];
            for (int i = 0; i < pairs.Length; i++) {
                result[i] = pairs[i].Value;
            }
            return result;
        }

        public bool TryGetAssembly(string codeBase, out MetadataAssembly assembly) {
            MetadataName name;
            if (this._nameContainer.TryGetName(codeBase, out name)) {
                return this.TryGetAssembly(name, out assembly);
            }

            assembly = null;
            return true;
        }

        internal bool TryGetAssembly(MetadataName codeBase, out MetadataAssembly assembly) {
            return this._assemblies.TryGetValue(codeBase, out assembly);
        }
    }
}
