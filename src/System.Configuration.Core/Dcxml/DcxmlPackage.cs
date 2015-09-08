using System.Collections.Generic;
using System.Xml.Linq;

namespace System.Configuration.Core.Dcxml {

    internal class DcxmlPackage : BasicPackage {
        private string[] _files;
        public DcxmlPackage(string packageName, ConfigurationRuntime runtime, string[] files)
            : base(packageName,runtime) {
            this._files = files;
        }

        protected override IEnumerable<KeyValuePair<FullName, ConfigurationObjectPart>> LoadPartsCore() {
            foreach (var file in _files) {
                var dcxmlFile = new DcxmlFile(file, this);
                foreach (var part in dcxmlFile.LoadParts()) {
                    yield return part;
                }
            }
        }
    }
}
