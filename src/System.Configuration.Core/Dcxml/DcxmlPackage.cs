using System.Collections.Generic;
using System.Xml.Linq;

namespace System.Configuration.Core.Dcxml {

    internal class DcxmlPackage : BasicPackage {
        private string[] _files;
        public DcxmlPackage(string packageName, DcxmlRepository repository, string[] files)
            : base(packageName, repository) {
            this._files = files;
        }

        protected override IEnumerable<KeyValuePair<PcakagePartKey, ConfigurationObjectPart>> LoadPartsCore() {
            foreach (var file in _files) {
                var dcxmlFile = new DcxmlFile(file);
                foreach (var part in dcxmlFile.LoadParts()) {
                    yield return part;
                }
            }
        }
    }
}
