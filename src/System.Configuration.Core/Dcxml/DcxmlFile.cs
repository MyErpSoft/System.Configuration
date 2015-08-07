using System.Collections.Generic;
using System.Xml;

namespace System.Configuration.Core.Dcxml {

    internal sealed class DcxmlFile {

        public DcxmlFile(string fileName){
            if (string.IsNullOrEmpty(fileName)) {
                Utilities.ThrowArgumentNull(nameof(fileName));
            }

            this._fileName = fileName;
        }

        private readonly string _fileName;
        /// <summary>
        /// 返回关联的文件名。
        /// </summary>
        public string FileName {
            get {
                return _fileName;
            }
        }

        public IEnumerable<KeyValuePair<PcakagePartKey, ConfigurationObjectPart>> LoadParts() {

            /* <x:ObjectContainer
             *    xmlns="clr-namespace:company.erp.demo.ui,company.erp.demo.ui"
             *    xmlns:x="http://schemas.myerpsoft.com/configuration/2015" 
             *    namespace="company.erp.demo" >
             * 
             *    <Window x:name="frmMain">
             *       <Text>demo</Text>
             *    </Window>
             * 
             * </x:ObjectContainer>
             *
            */
            using (var reader = XmlReader.Create(this._fileName)) {
                reader.ReadStartElement("ObjectContainer");
                if (reader.NodeType == XmlNodeType.Element) {
                    var objNamespace = GetNamespace(reader);

                    do {
                        reader.ReadStartElement();
                        if (reader.NodeType == XmlNodeType.Element) {
                            var name = string.Intern(reader.GetAttribute("name"));
                            var data = reader.ReadOuterXml();
                            yield return new KeyValuePair<PcakagePartKey, ConfigurationObjectPart>(
                                new PcakagePartKey(objNamespace, name),
                                new DcxmlPart(data)
                                );
                        }
                        else {
                            break;
                        }

                    } while (!reader.EOF);
                }
            }
        }

        //获取文件的命名空间
        private static string GetNamespace(XmlReader reader) {
            var objNamespace = reader.GetAttribute("namespace");
            if (objNamespace != null) {
                if (objNamespace == string.Empty) {
                    objNamespace = null;
                }
                else {
                    objNamespace = string.Intern(objNamespace);
                }
            }

            return objNamespace;
        }
    }
}
