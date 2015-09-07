using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace System.Configuration.Core.Dc {

    /// <summary>
    /// 二进制的Package
    /// </summary>
    internal sealed class DcPackage : BasicPackage {

        internal DcPackage(string file, DcRepository repository)
            : base(PlatformUtilities.Current.GetFileNameWithoutExtension(file), repository) {
            this._file = file;
        }

        private readonly string _file;
        /// <summary>
        /// 返回此二进制Package的文件全路径。
        /// </summary>
        public string File {
            get { return _file; }
        }

        /// <summary>
        /// 在后台程序读取数据时发生的异常。
        /// </summary>
        internal Exception ReadDataException { get; set; }

        /// <summary>
        /// 读取数据的上下文数据。
        /// </summary>
        internal PackageReadContext ReadContext { get; set; }
        private int _partCount;

        protected override IEnumerable<KeyValuePair<FullName, ConfigurationObjectPart>> LoadPartsCore() {
            Stream stream = null;
            this.ReadContext = null;
            BinaryPackageReader reader = null;

            try {
                stream = PlatformUtilities.Current.Open(File, FileMode.Open, FileAccess.Read, FileShare.Read);
                this.ReadContext = new PackageReadContext();
                reader = new BinaryPackageReader(stream, this, this.ReadContext);

                var allPairs = reader.ReadPackage();
                this._partCount = allPairs.Length;

                return allPairs;
            }
            catch {
                //如果出现异常，异步线程没有启动的话，需要关闭流，否则不能关闭流，不然异步线程就不能工作了。
                if (reader != null) {
                    reader.Close();
                }

                if (stream != null) {
                    stream.Close();
                }

                this.ReadContext = null;
                
                throw;
            }
        }

        private int _openDataCount;
        //在完成一个Part的OpenData后，会计数，以便在最后一个Part完成后释放读取时需要的所有内存。
        //注意：OpenData有可能部分是Reader的后台线程调用的，有可能是外部程序在GetObject时主动调用的。
        internal void CountOpenData() {
            Interlocked.Increment(ref _openDataCount);

            if (_openDataCount == _partCount) {
                //这里不必调用Close，流在第二阶段（ReadOther）后，就已经关闭。
                this.ReadContext = null;
                this.ReadDataException = null;
            }
        }

        public override string ToString() {
            return "DC:" + this._file;
        }
    }
}
