using System;
using System.Collections.Generic;
using System.Configuration.Core.Metadata.Clr;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Core.Tests {

    public class Window {
        static Window() {
            var t = ClrType.GetClrType(typeof(Window));
            TextProperty = t.GetProperty("Text");
        }

        private readonly ConfigurationObject _cObj;
        public Window(ConfigurationObject cObj) {
            this._cObj = cObj;
        }

        private static readonly ClrProperty TextProperty;

        public string Text {
            get {
                return (string)_cObj.GetValue(TextProperty);
            }
            set {
                throw new NotImplementedException();
            }
        }
    }
}
