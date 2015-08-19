using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using static System.Configuration.Core.Dcxml.DcxmlRepository;

namespace System.Configuration.Core.Dcxml {

    internal sealed class DcxmlFile {

        public DcxmlFile(string fileName,DcxmlPackage package){
            if (string.IsNullOrEmpty(fileName)) {
                Utilities.ThrowArgumentNull(nameof(fileName));
            }
            if (package == null) {
                Utilities.ThrowArgumentNull(nameof(package));
            }

            this._fileName = fileName;
            this._package = package;
        }

        private readonly string _fileName;
        /// <summary>
        /// 返回关联的文件名。
        /// </summary>
        public string FileName {
            get { return _fileName; }
        }

        private string _namespace;
        /// <summary>
        /// 返回当前文件的命名空间
        /// </summary>
        public string Namespace {
            get { return _namespace; }
        }

        private DcxmlPackage _package;
        /// <summary>
        /// 返回此文件所在的包。
        /// </summary>
        public DcxmlPackage Package {
            get { return _package; }
        }

        private UsingCollection _usings;
        /// <summary>
        /// 对其他库的引用。
        /// </summary>
        public UsingCollection Usings {
            get { return _usings; }
        }
        
        internal IEnumerable<KeyValuePair<FullName, ConfigurationObjectPart>> LoadParts() {

            /* <x:ObjectContainer
             *    xmlns="clr-namespace:company.erp.demo.ui,company.erp.demo.ui"
             *    xmlns:x="http://schemas.myerpsoft.com/configuration/2015" 
             *    xmlns:y="clr-namespace:myui,myui.test"
             *    x:namespace="company.erp.demo" >
             * 
             *    <Window x:name="frmMain" x:base="BasicForm" >
             *       <Text>demo</Text>
             *    </Window>
             * 
             *    <y:MyButton x:name="btnOK">
             *        <Text>OK</Text>
             *    </y:MyButton>
             * </x:ObjectContainer>
             *
            */

            //使用xdocument模型实现的好处是代码容易写，但是存在XElement无法释放的问题，
            //等成熟后使用reader
            using (var stream = PlatformUtilities.Current.Open(_fileName, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)) {
                XDocument doc = XDocument.Load(stream, LoadOptions.SetLineInfo);
                var root = doc.Root;
                if (root != null) {
                    _namespace = GetNamespace(root);
                    _usings = new UsingCollection(root.Attribute(DcxmlName_Using) ? .Value, this);

                    foreach (var item in root.Elements()) {
                        if (item.NodeType == XmlNodeType.Element) {
                            var name = item.Attribute(DcxmlName_Name) ? .Value;
                            if (!Utilities.VerifyName(name)) {
                                Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                              "在dcxml：{0}中定义的命名空间 {1} 不准确，必须是字母、数字和下划线。", this.FileName, name));
                            }

                            name = string.Intern(name);
                            var objTypeName = GetTypeName(item.Name);

                            yield return new KeyValuePair<FullName, ConfigurationObjectPart>(
                                    new FullName(_namespace, name),
                                    new DcxmlPart(this, objTypeName, item)
                                    );
                        }
                    }
                }
            }

            //using (var reader = XmlReader.Create(this._fileName)) {
            //    reader.ReadStartElement("ObjectContainer");
            //    if (reader.NodeType == XmlNodeType.Element) {
            //        var objNamespace = GetNamespace(reader);

            //        do {
            //            reader.ReadStartElement();
            //            if (reader.NodeType == XmlNodeType.Element) {
            //                var name = string.Intern(reader.GetAttribute("name"));
            //                var data = reader.ReadOuterXml();
            //                yield return new KeyValuePair<FullName, ConfigurationObjectPart>(
            //                    new FullName(objNamespace, name),
            //                    new DcxmlPart(this,data)
            //                    );
            //            }
            //            else {
            //                break;
            //            }

            //        } while (!reader.EOF);
            //    }
            //}
        }

        private Dictionary<XNamespace, ObjectTypeQualifiedName> _typePackages;
        //通过节点信息（例如Window或y:MyButton）获取到完整的类型描述信息。
        private ObjectTypeQualifiedName GetTypeName(XName name) {
            if (_typePackages == null) {
                _typePackages = new Dictionary<XNamespace, ObjectTypeQualifiedName>();
            }

            var xmlNamespace = name.Namespace;
            ObjectTypeQualifiedName typeNameWithoutName;
            if (!_typePackages.TryGetValue(xmlNamespace, out typeNameWithoutName)) {
                typeNameWithoutName = ObjectTypeQualifiedName.CreateWithoutName(xmlNamespace.NamespaceName);
                _typePackages.Add(xmlNamespace, typeNameWithoutName);
            }

            return typeNameWithoutName.CreateByName(name.LocalName);
        }

        internal static string GetXObjectDebugString(XElement element, XAttribute att) {
            StringBuilder sb = new StringBuilder();
            XObject xObject = null;
            if (element != null) {
                sb.Append('<');
                sb.Append(element.Name);
                xObject = element;
            }

            if (att != null) {
                xObject = att;
                sb.Append("[" + att.Name + "]");
            }
            
            IXmlLineInfo lineInfo = (IXmlLineInfo)xObject;
            if (lineInfo != null && lineInfo.HasLineInfo()) {
                sb.Append('(');
                sb.Append(lineInfo.LineNumber);
                sb.Append(',');
                sb.Append(lineInfo.LinePosition);
                sb.Append(')');
            }

            return sb.ToString();
        }

        //获取文件的默认命名空间
        private string GetNamespace(XElement rootElement) {
            var att = rootElement.Attribute(DcxmlName_Namespace);
            if (att != null && !string.IsNullOrEmpty(att.Value)) { //命名空间可以不填写。
                if (!Utilities.VerifyNameWithNamespace(att.Value)) {
                    Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                              "在dcxml：{0}中定义的命名空间 {1} 不准确，必须是字母、数字和下划线或多个单词使用点分隔。", this.FileName, att.Value));
                }
                return string.Intern(att.Value);
            }

            return null;
        }
    }
}
