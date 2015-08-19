using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration.Core.Metadata.Clr;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Core.Tests {

    public class Window {
        static Window() {
            var t = ClrType.GetClrType(typeof(Window));
            TextProperty = t.GetProperty(nameof(Text));
        }

        private readonly ConfigurationObject _cObj;
        public Window(ConfigurationObject cObj) {
            this._cObj = cObj;
        }

        private static readonly ClrProperty TextProperty;

        public string Text {
            get {
                return (string)_cObj.GetSimpleValue(TextProperty);
            }
            set {
                throw new NotImplementedException();
            }
        }
    }

    public class Control {
        static Control() {
            var t = ClrType.GetClrType(typeof(Control));
            TextProperty = t.GetProperty(nameof(Text));
            EnabledProperty = t.GetProperty(nameof(Enabled));
            AnchorProperty = t.GetProperty(nameof(Anchor));
            BackgroundImageProperty = t.GetProperty(nameof(BackgroundImage));
            ControlsProperty = t.GetProperty(nameof(Controls));
        }

        private readonly ConfigurationObject _cObj;
        public Control(ConfigurationObject cobj) {
            this._cObj = cobj;
        }

        private static readonly ClrProperty TextProperty;
        public string Text {
            get {
                return (string)_cObj.GetSimpleValue(TextProperty);
            }
            set {
                throw new NotImplementedException();
            }
        }

        private static readonly ClrProperty EnabledProperty;
        public bool Enabled {
            get {
                return (bool)_cObj.GetSimpleValue(EnabledProperty);
            }
            set {
                throw new NotImplementedException();
            }
        }

        private static readonly ClrProperty AnchorProperty;
        public AnchorStyles Anchor {
            get { return (AnchorStyles)_cObj.GetSimpleValue(AnchorProperty); }
            set { throw new NotImplementedException(); }
        }

        private static readonly ClrProperty BackgroundImageProperty;
        public Image BackgroundImage {
            get { return (Image)_cObj.GetReferenceValue(BackgroundImageProperty); }
        }

        private static readonly ClrProperty ControlsProperty;
        private ControlCollection _controls;
        public ControlCollection Controls {
            get {
                if (_controls == null) {
                    _controls = new ControlCollection(_cObj.GetListValue(ControlsProperty).Cast<Control>().ToList());
                }

                return _controls;
            }
        }
    }

    public class ControlCollection : Collection<Control> {
        public ControlCollection(IList<Control> list):base(list) {
        }
    }

    public class Image {
        static Image() {
            var t = ClrType.GetClrType(typeof(Image));
            PathProperty = t.GetProperty(nameof(Path));
        }

        private readonly ConfigurationObject _cObj;
        public Image(ConfigurationObject cobj) {
            this._cObj = cobj;
        }

        private static readonly ClrProperty PathProperty;
        public string Path {
            get { return (string)_cObj.GetSimpleValue(PathProperty); }
            set {
                throw new NotImplementedException();
            }
        }
    }

    public class Button : Control {
        public Button(ConfigurationObject cobj):base(cobj) {
        }
    }

    [Flags]
    public enum AnchorStyles {

        /// <include file='doc\AnchorStyles.uex' path='docs/doc[@for="AnchorStyles.Top"]/*' />
        /// <devdoc>
        ///    <para>
        ///       The control is anchored to the top edge of its container.
        ///    </para>
        /// </devdoc>
        Top = 0x01,
        /// <include file='doc\AnchorStyles.uex' path='docs/doc[@for="AnchorStyles.Bottom"]/*' />
        /// <devdoc>
        ///    <para>
        ///       The control is anchored to the bottom edge of its container.
        ///    </para>
        /// </devdoc>
        Bottom = 0x02,
        /// <include file='doc\AnchorStyles.uex' path='docs/doc[@for="AnchorStyles.Left"]/*' />
        /// <devdoc>
        ///    <para>
        ///       The control is anchored to the left edge of its container.
        ///    </para>
        /// </devdoc>
        Left = 0x04,
        /// <include file='doc\AnchorStyles.uex' path='docs/doc[@for="AnchorStyles.Right"]/*' />
        /// <devdoc>
        ///    <para>
        ///       The control is anchored to the right edge of its container.
        ///    </para>
        /// </devdoc>
        Right = 0x08,

        /// <include file='doc\AnchorStyles.uex' path='docs/doc[@for="AnchorStyles.None"]/*' />
        /// <devdoc>
        ///    <para>
        ///       The control is not anchored to any edges of its container.
        ///    </para>
        /// </devdoc>
        None = 0,
    }

}
