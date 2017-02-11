using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Core.Dc {
    internal sealed class DcBinaryWriter : BinaryWriter {
        public DcBinaryWriter(Stream output)
            :base(output, new UTF8Encoding(), true){

        }

        public void Write7BitInt(int value) {
            this.Write7BitEncodedInt(value);
        }

        //写入字符串清单表
        // int              命名空间总数
        // string[]         顺序填写的字符串
        public void WriteStrings(string[] strings, int startIndex, int size) {
            //写入总共多少，以便读取时知道什么时候结束了。
            this.Write7BitEncodedInt(size);

            for (int i = startIndex; i < size + startIndex; i++) {
                this.Write(strings[i]);//不能出现null.
            }
        }
    }
}
