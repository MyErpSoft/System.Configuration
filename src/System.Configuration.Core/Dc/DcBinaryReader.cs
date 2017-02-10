using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System.Configuration.Core.Dc {

    internal sealed class DcBinaryReader : BinaryReader {
        public DcBinaryReader(Stream input) : base(input, new UTF8Encoding(), true) {
        }

        public int Read7BitInt() {
            return this.Read7BitEncodedInt();
        }

        //读取一个字符串清单，并将这些字符串做Intern处理。
        public void ReadInternStrings(List<string> strs) {
            var currentIndex = strs.Count;
            ReadStrings(strs);

            for (int i = currentIndex; i < strs.Count; i++) {
                strs[i] = string.Intern(strs[i]);
            }
        }

        //读取一个字符串数组清单
        public void ReadStrings(List<string> strs) {
            var count = this.Read7BitEncodedInt(); //总数
            if (count > 0) {
                ReadStrings(strs, count);
            }
        }

        //读取指定数量的字符串到集合中。
        public void ReadStrings(List<string> strs, int count) {
            strs.Capacity = strs.Count + count;

            for (int i = 0; i < count; i++) {
                strs.Add(this.ReadString());
            }
        }

        private byte[] _bufferForIntArray;
        private const int _maxBufferIntCount = 64;//最大缓冲的int大小
        public void ReadIntArray(int[] array, int count) {
            if (_bufferForIntArray == null) {
                _bufferForIntArray = new byte[_maxBufferIntCount * 4];
            }

            var size = 0; //已经填充的大小
            int fullCount; //一次循环需要填充几个数据。
            int byteCount;//需要的字节数量

            do {
                // 64 + 64 > 100
                if (size + _maxBufferIntCount > count) {
                    //比如count = 100，那么第二次填充时，就仅仅填充 36 = 100 - 64
                    fullCount = count - size;
                }
                else {
                    fullCount = _maxBufferIntCount;
                }

                byteCount = count * 4;
                this.Read(_bufferForIntArray, 0, byteCount);
                Buffer.BlockCopy(_bufferForIntArray, 0, array, size * 4, byteCount);
                size += fullCount;
            } while (size < count);

        }
    }
}
