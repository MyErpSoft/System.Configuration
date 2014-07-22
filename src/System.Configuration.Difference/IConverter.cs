using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Configuration.Difference {

    public interface IConverter {

        string ToString(object value);

        object FromString(string str);
    }
}
