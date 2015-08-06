using System;

namespace System.Configuration.DataEntities {

    public sealed class MetadataAssembly {

        internal MetadataAssembly(MetadataName codeBase, MetadataContainer metadataContainer) {
            this._codeBase = codeBase;
            this._metadataContainer = metadataContainer;
        }

        private readonly MetadataName _codeBase;
        private readonly MetadataContainer _metadataContainer;

        public string CodeBase {
            get { return this._codeBase.ToString(); }
        }
    }
}
