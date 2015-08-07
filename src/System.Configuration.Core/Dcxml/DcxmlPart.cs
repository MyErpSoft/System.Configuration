namespace System.Configuration.Core.Dcxml {

    internal class DcxmlPart : ConfigurationObjectPart {
        private string _data;

        public DcxmlPart(string data) {
            this._data = data;
        }
        
        protected override void OpenDataCore() {
            //
        }
    }
}