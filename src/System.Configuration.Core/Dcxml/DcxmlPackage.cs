using System.Collections.Generic;
using System.Configuration.Core.Metadata;
using System.Xml.Linq;

namespace System.Configuration.Core.Dcxml {

    internal class DcxmlPackage : BasicPackage {

        private string[] _files;
        public DcxmlPackage(string packageName, IConfigurationObjectBinder binder, string[] files)
            : base(packageName, binder) {
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

        public override string ToString() {
            if (_files.Length > 0) {
                return "DcxmlPackage:" + this.Name + "  {" + this._files[0] + "...}";
            }

            return base.ToString();
        }

        #region Xml namespace
        private static string _configurationXmlNamespace;
        /// <summary>
        /// 返回缺省的配置系统命名空间。"http://schemas.myerpsoft.com/configuration/2015"
        /// </summary>
        public static string ConfigurationXmlNamespace {
            get { return _configurationXmlNamespace; }
            set {
                _configurationXmlNamespace = value;
                UpdateXmlNamespace();
            }
        }

        static DcxmlPackage() {
            _configurationXmlNamespace = "http://schemas.myerpsoft.com/configuration/2015";
            UpdateXmlNamespace();
        }

        static void UpdateXmlNamespace() {
            XmlNamespace = _configurationXmlNamespace;
            DcxmlName_ObjectContainer = XmlNamespace + "ObjectContainer";
            DcxmlName_Namespace = XmlNamespace + "namespace";
            DcxmlName_Using = XmlNamespace + "using";
            DcxmlName_Name = XmlNamespace + "name";
            DcxmlName_Base = XmlNamespace + "base";
            DcxmlName_Ref = XmlNamespace + "ref";
            DcxmlName_ListAction = XmlNamespace + "action";
        }

        internal static XNamespace XmlNamespace;
        internal static XName DcxmlName_ObjectContainer;
        internal static XName DcxmlName_Namespace;
        internal static XName DcxmlName_Using;
        internal static XName DcxmlName_Name;
        internal static XName DcxmlName_Base;
        internal static XName DcxmlName_Ref;
        internal static XName DcxmlName_ListAction;
        #endregion
    }
}
