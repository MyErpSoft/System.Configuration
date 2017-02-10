using System.Collections.Generic;
using System.Configuration.Core.Metadata;
using System.IO;
using System.Threading;

namespace System.Configuration.Core.Dc {

    /// <summary>
    /// 二进制的Package
    /// </summary>
    internal sealed class DcPackage : BasicPackage {

        internal DcPackage(string file, IConfigurationObjectBinder binder)
            : base(string.Intern(PlatformUtilities.Current.GetFileNameWithoutExtension(file)), binder) {
            this._file = file;
        }

        private readonly string _file;
        /// <summary>
        /// 返回此二进制Package的文件全路径。
        /// </summary>
        public string File {
            get { return _file; }
        }

        private DcPackageReader _reader;
        /// <summary>
        /// 读取数据的上下文数据。
        /// </summary>
        internal DcPackageReader Reader { get { return _reader; } }
        
        protected override IEnumerable<KeyValuePair<FullName, ConfigurationObjectPart>> LoadPartsCore() {
            using (var stream = PlatformUtilities.Current.Open(File, FileMode.Open, FileAccess.Read, FileShare.Read)) { 
                _reader = new DcPackageReader(stream, this);

                var parts = _reader.ReadParts();
                _partCount = _reader.Count;
            
                return parts;
            }
        }

        protected override void OnLoaded() {
            base.OnLoaded();

            //开启后台任务，将各自零件OpenData
            Threading.Tasks.Task.Run(new Action(this.OpenAllData));
        }

        private void OpenAllData() {
            //后台线程仍然继续，将数据解包。
            foreach (var item in GetParts()) {
                try {
                    var basicPart = item.Value as BasicPart;
                    if (basicPart != null && !basicPart.IsOpened) {
                        basicPart.OpenData();
                    }
                }
                catch {
                    //任何数据的解包失败，都不会阻止此线程的停止
                    //由于IsOpened未设置为true，所以在首次使用时仍然会出现异常。
                }
            }
        }

        private int _partCount;
        private int _openDataCount;
        //在完成一个Part的OpenData后，会计数，以便在最后一个Part完成后释放读取时需要的所有内存。
        //注意：OpenData有可能部分是Reader的后台线程调用的，有可能是外部程序在GetObject时主动调用的。
        internal void CountOpenData() {
            Interlocked.Increment(ref _openDataCount);

            if (_openDataCount == _partCount) {
                //这里不必调用Close，流在LoadPartsCore后，就已经关闭。
                this._reader = null;
            }
        }

        public override string ToString() {
            return "DcPackage:" + this.Name + "  {" + this._file + "}";
        }
    }
}
