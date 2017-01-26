using System.Globalization;
using System.Threading;
using System.IO;
using System.Configuration.Core.Metadata;

namespace System.Configuration.Core.Dc {

    /// <summary>
    /// 二进制的部件。
    /// </summary>
    internal sealed class DcPart : BasicPart {
        //由BinaryPackageReader填充
        internal byte[] _data;
        private readonly DcPackage _sourcePackage;

        public DcPart(DcPackage sourcePackage) {
            this._sourcePackage = sourcePackage;
        }

        private IType _type;
        /// <summary>
        /// 返回当前对象的类型信息。
        /// </summary>
        public override IType Type {
            get { return _type; }
        }

        protected override void OpenDataCore(ConfigurationRuntime runtime) {
            //等待后台线程将_data填充。
            if (!SpinWait.SpinUntil(this.DataIsFilled,60000)) {
                Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                    "加载配置文件{0}超时。", this._sourcePackage.Name));
            }

            if (_data.Length == 0 && _sourcePackage.ReadDataException != null) {
                throw _sourcePackage.ReadDataException;
            }

            var readContext = _sourcePackage.ReadContext;
            using (var ms = new MemoryStream(_data)) {
                using (var reader = new BinaryPackageReader(ms,_sourcePackage,readContext)) {

                    //类型
                    var typeData = reader.ReadTypeData();
                    if (typeData.Type == null) {
                        typeData.Type = runtime.Binders.BindToType(typeData.Name);
                    }
                    _type = typeData.Type;

                    foreach (var item in reader.ReadProperties(typeData)) {
                        this.SetLocalValue(item.Key, item.Value);
                    }
                }
            }


            this._data = null;
            _sourcePackage.CountOpenData();
        }

        private bool DataIsFilled() {
            return _data != null;
        }
        
    }
}
