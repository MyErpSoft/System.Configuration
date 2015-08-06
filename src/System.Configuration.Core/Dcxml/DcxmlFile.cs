using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace System.Configuration.Core.Dcxml {

    internal sealed class DcxmlFile {

        public DcxmlFile(string fileName){
            if (string.IsNullOrEmpty(fileName)) {
                Utilities.ThrowArgumentNull(nameof(fileName));
            }

            this._fileName = fileName;
            var doc = XDocument.Load(fileName, LoadOptions.SetLineInfo);
            this._doc = doc;
            this._parts = new Collection<DcxmlPart>();

            this.ReadAll();
        }

        private readonly string _fileName;
        private readonly XDocument _doc;
        private readonly Collection<DcxmlPart> _parts;

        public Collection<DcxmlPart> Parts {
            get { return this._parts; }
        }

        private void ReadAll() {
            _doc.Root.Nodes();
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
           
        }
    }
}
