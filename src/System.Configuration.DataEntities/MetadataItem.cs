using System;

namespace System.Configuration.DataEntities {

    public sealed class MetadataItem {

        internal MetadataItem(MetadataFullName fullName, object data) {
            if (fullName == null) {
                throw new ArgumentNullException("fullName");
            }
            if (data == null) {
                throw new ArgumentNullException("data");
            }

            this._fullName = fullName;
            this._data = data;
        }

        private readonly MetadataFullName _fullName;
        /// <summary>
        /// Return metadata item namespace.
        /// </summary>
        public string Namespace {
            get {
                //ex:fullName=System.Configuration.DataEntities.MetadataItem
                //namespace=System.Configuration.DataEntities
                var pre = this._fullName.Previous;
                if (pre != null) {
                    return pre.ToString();
                }
                else {
                    return null;
                }
            }
        }

        /// <summary>
        /// Return metadata item name.
        /// </summary>
        public string Name {
            get {
                return this._fullName.CurrentName.ToString();
            }
        }

        /// <summary>
        /// Return metadata item full name.
        /// </summary>
        public string FullName {
            get {
                return this._fullName.ToString();
            }
        }

        private object _data;
        /// <summary>
        /// Returns the associated data
        /// </summary>
        public object Data {
            get { return _data; }
        }

    }
}
