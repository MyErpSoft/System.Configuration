using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Configuration.Core {

    public abstract class ConfigurationObjectPart {

        public abstract Package Package { get; }
        public abstract string Namespace { get; }
        public abstract string Name { get; }
    }
}
