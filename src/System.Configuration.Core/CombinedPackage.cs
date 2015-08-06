using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Configuration.Core
{
    internal class CombinedPackage : Package
    {
        private readonly IList<Package> _packages;
        public CombinedPackage(IList<Package> packages) :
            base(packages[0].Name,packages[0].Repository){
            this._packages = packages;
        }

        public override bool TryGetPart(string objNamespace, string name, out ConfigurationObjectPart part) {
            throw new NotImplementedException(); //合并成一个部件。
        }
    }
}
