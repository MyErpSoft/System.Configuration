using System.Collections.Generic;
using System.Xml.Linq;

namespace System.Configuration.Core.Dcxml {

    internal class DcxmlPackage : BasicPackage {
        private string[] _files;
        public DcxmlPackage(string packageName, DcxmlRepository repository, string[] files)
            : base(packageName, repository) {
            this._files = files;
        }

        protected override IEnumerable<ConfigurationObjectPart> LoadPartsCore() {
            throw new NotImplementedException();
        }
    }
}
