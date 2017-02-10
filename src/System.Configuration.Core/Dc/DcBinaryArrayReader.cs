using System;
using System.Text;

namespace System.Configuration.Core.Dc {
    /* 数据块格式详解。
     * 分区     数据类型         说明
     * A        int32       数组的大小，例如存储的是字符串，那么就描述了字符串数组的大小
     * B        int32[A]    一组数据，描述了每个元素的数据结束位置，位置以C区的0开始计数，假设
     *                      第一个元素占用9 byte，第二个元素占用 13byte，那么数据就是 9,22
     *                      所以通过 4 + A * 4 + B[Last]是可以得到 整个数据块的大小，即占用了多少的byte；
     * C        byte[]      实际数据的存储区，
    */

    /// <summary>
    /// 一种使用二进制数组存储的数组，在需要时解压对应的数据，例如在读取二进制文件时，一个大的值堆包含很多的字符串，我们仅仅在需要的时候才展开数据。
    /// </summary>
    internal abstract class DcBinaryArrayReader<T> where T : class {
        private byte[] _bytes;      //二进制的数据
        private int _bytesOffset;   //本数据块在数组的开始位置。
        private T[] _items;         //获取到的数据，注意是延迟创建真正的元素
        private int[] _endOffsets;  //每个元素对应的结束位置

        public DcBinaryArrayReader(byte[] bytes,int offset) {
            if (bytes == null) {
                Utilities.ThrowArgumentNull(nameof(bytes));
            }
            if (offset < 0 || offset + 4 > bytes.Length) {
                Utilities.ThrowArgumentException("开始位置大于数组的大小", nameof(offset));
            }
            _bytes = bytes;
            _bytesOffset = offset;

            //创建存储元素的数组
            var itemCount = GetInt32(0);
            _items = new T[itemCount];

            //构建索引数组
            _endOffsets = new int[itemCount];
            Buffer.BlockCopy(_bytes, _bytesOffset + 4, _endOffsets, 0, itemCount * 4);
        }

        /// <summary>
        /// 返回实际元素的数组大小。
        /// </summary>
        public int ItemCount {
            get { return _items.Length; }
        }

        /// <summary>
        /// 返回此数据块实际占用的大小。
        /// </summary>
        public int ByteCount {
            get {
                return 4 +                                                          //A 区 ，
                       _items.Length * 4 +                                          //B 区 ，索引
                       _items.Length == 0 ? 0 : _endOffsets[_items.Length - 1];     //C 区， 最后一个元素描述了相对本数据块的最后位置。
            }
        }

        /// <summary>
        /// 获取指定索引位置的元素，注意：他是从1开始计数的，0永远返回null。
        /// </summary>
        /// <param name="index">从1开始计数的索引</param>
        /// <returns></returns>
        public T this[int index] {
            get {
                //注意：他是从1开始计数的，0永远返回null。
                if (index == 0) {
                    return null;
                }

                index--;
                var item = _items[index];
                if (item != null) {
                    return item;
                }

                item = ConvertItem(GetItemBytes(index));
                _items[index] = item;

                return item;
            }
        }

        protected abstract T ConvertItem(ArraySegment<byte> bytes);

        /// <summary>
        /// 获取数组中某个元素的数据位置，注意index从0计数。
        /// </summary>
        protected ArraySegment<byte> GetItemBytes(int index) {
            var offset = (index == 0) ? 0 : _endOffsets[index - 1];
            var count = _endOffsets[index] - offset;
            return new ArraySegment<byte>(_bytes, offset, count);
        }

        protected int GetInt32(int offset) {
            return (int)(_bytes[offset] | _bytes[offset + 1] << 8 | _bytes[offset + 2] << 16 | _bytes[offset + 3] << 24);
        }
        
    }

    //internal sealed class DcStringArrayReader : DcBinaryArrayReader<string> {
    //    public DcStringArrayReader(byte[] bytes, int offset) :base(bytes,offset){ }

    //    //protected override string ConvertItem(ArraySegment<byte> bytes) {
    //    //    charsRead = m_decoder.GetChars(m_charBytes, 0, n, m_charBuffer, 0);
    //    //}
    //}
}
