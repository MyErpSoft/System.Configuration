using System.Configuration.Core.Metadata;

namespace System.Configuration.Core.Dc {

    /// <summary>
    /// 二进制的部件。
    /// </summary>
    internal sealed class DcPart : BasicPart {
        private int _dataOffset;
        private readonly DcPackage _sourcePackage;

        internal DcPart(DcPackage sourcePackage, ArraySegment<byte> data) {
            this._sourcePackage = sourcePackage;
            this._dataOffset = data.Offset;
        }

        private IType _type;
        /// <summary>
        /// 返回当前对象的类型信息。
        /// </summary>
        public override IType Type {
            get { return _type; }
        }

        protected override void OpenDataCore() {
            //一个稍微大些的文件可能有10万个Part
            //为避免每展开一个DcPart都创建一个Memory+Reader对象，我们使用复用技术，但需要考虑并发问题
            using (var reader = _sourcePackage.Reader.CreateDcPartReader(_dataOffset)) {

                //类型
                _type = reader.ReadObjectType();

                foreach (var item in reader.ReadProperties()) {
                    SetLocalValue(item.Key, item.Value);
                }
            }

            //通知Package已经完成一个展开动作，当所有的Part完成展开动作后将自动释放所有二进制数据资源。
            //这里有个bug，当某个零件展开出现异常，会导致计数器无法到达总数，导致内存一直占用。
            _sourcePackage.CountOpenData();
        }
    }
}
