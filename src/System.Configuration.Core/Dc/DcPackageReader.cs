using System.Collections.Generic;
using System.Configuration.Core.Metadata;
using System.Globalization;
using System.IO;
using System.Linq;

/* 
* 文件读取流程说明：
* 读取二进制的文件使用Read方法，他会读取所有的二进制数据到内存中，并返回所有Part对象，但这些对象的二进制数据没有展开。
* ReadParts :
*       => ReadFileHead        读取文件头，确保读取的文件是指定的格式；
*       => ReadInternStrings   返回所有重要字符串的清单，这些字符串需要Intern处理，以便全局公用引用。
*       => ReadObjectTypes     读取类型数据以及用到的属性（注意只是数据还没有指向IType)，这样就建立了一个映射表，在后续的实际数据中通过一个序号可以检索到一个IType或对应的属性；
*          => ReadStrings      读取后续类型和属性要用到的所有字符串，例如IType的namespace name package providerName,以及用到的各个属性的名称。
*          =>                  Type总数  
*            => ReadObjectType      此Type的providerName namespace name package 对应的字符串索引位置；
*            => ReadProperties      此Type使用到的属性清单总数，逐个读取属性名称。
*                   
*       => ReadObjectIds       读取所有的id清单数组，并创建对应的DcPart实例，注意DcPart实例此时没有被填充任何数据。
*           =>                 对象总数
*           =>                 对象命名空间，名称的索引，数据块的开始位置
*       => ReadData            读取对应的二进制数据；
*       返回Pairs
*          
* 当返回所有零件之后，DcPackage会提交一个任务，异步的完成展开的动作。
* 注：之前的版本使用了复杂的多线程处理，但会造成处理复杂，先改为较简单的模式，文件一次性读取，异步完成解包。
*/
namespace System.Configuration.Core.Dc {

    /// <summary>
    /// DcPackage的文件读取器，传入一个流可以返回实际的数据。
    /// </summary>
    internal partial class DcPackageReader {

        public DcPackageReader(Stream input, Package sourcePackage) {
            if (sourcePackage == null) {
                Utilities.ThrowArgumentNull(nameof(sourcePackage));
            }
            this._strings = new List<string>();
            this._types = new List<ObjectTypeReadData>();

            //List<T>的索引是从0开始的，而我们的字符串索引是从1开始的，0预留给null.
            this._strings.Add(null);
            this._types.Add(null);

            this._reader = new DcBinaryReader(input);
            this._sourcePackage = sourcePackage;
        }

        // 所有可用的字符串清单
        private List<string> _strings;
        private List<ObjectTypeReadData> _types;

        private readonly DcBinaryReader _reader;
        private readonly Package _sourcePackage;
        private FullName[] _objectIds;                          //所有的对象Id数组
        private int[] _partEndOffsets;                          //每个零件在数据块中结束位置
        private byte[] _data;                                   //数据的二进制形式

        /// <summary>
        /// 读取所有的零件，并返回这些零件与名称的对应关系。
        /// </summary>
        /// <returns>一个数组，包含零件名称和零件。</returns>
        public KeyValuePair<FullName, ConfigurationObjectPart>[] ReadParts() {

            try {
                //1 读取文件头
                this.ReadFileHead();

                //2  返回所有重要字符串的清单
                _reader.ReadInternStrings(_strings);

                //3 读取这些对象其所属类型
                this.ReadObjectTypes();

                //5 读取所有的id清单数组
                this.ReadObjectIds();

                //6 读取对应的二进制数据
                this.ReadData();

                return this.GetParts().ToArray();
            }
            catch (EndOfStreamException) {
                Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                            "不是有效的dc文件格式。", _sourcePackage.Name));
                return null;
            }
        }

        protected void ReadFileHead() {
            var strLen = _reader.Read7BitInt();
            if (strLen != DcPackageWriter.FileHead.Length) {
                Utilities.ThrowApplicationException(
                    string.Format(CultureInfo.CurrentCulture,"不是有效的dc文件格式。", _sourcePackage.Name));
            }

            foreach (var item in DcPackageWriter.FileHead) {
                var currentChar = _reader.ReadChar();
                if (currentChar != item) {
                    Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                        "不是有效的dc文件格式。", _sourcePackage.Name));
                }
            }
        }

        protected virtual void ReadObjectTypes() {
            //读取后续类型和属性要用到的所有字符串
            _reader.ReadStrings(_strings);

            var count = _reader.Read7BitInt();
            _types.Capacity = _types.Count + count;

            //使用4个int描述的ObjectType，最后一个描述属性总数
            for (int i = 0; i < count; i++) {
                //对象类型信息的描述，
                var providerName = _strings[_reader.Read7BitInt()];
                var objNamespace = _strings[_reader.Read7BitInt()];
                var name = _strings[_reader.Read7BitInt()];
                var packageName = _strings[_reader.Read7BitInt()];

                var typeData = new ObjectTypeReadData() {
                    Name = ObjectTypeQualifiedName.Create(providerName, objNamespace, name, packageName)
                };

                //使用到的属性清单,
                var propertyCount = _reader.Read7BitInt();
                var properties = typeData.Properties;
                properties.Capacity = properties.Count + propertyCount;

                for (int j = 0; j < propertyCount; j++) {
                    properties.Add(
                        new ObjectPropertyReadData() {
                            //这里Name是属性的名称，注意有个特殊的属性，即Base
                            Name = _strings[_reader.Read7BitInt()]
                        });
                }

                _types.Add(typeData);
            }
        }

        /// <summary>
        /// 获取对象编号清单，他是由一个命名空间索引（ReadNamespaces），和一个普通字符串组成。
        /// </summary>
        protected void ReadObjectIds() {
            string objNamespace;
            string name;

            var count = _reader.Read7BitInt();
            _objectIds = new FullName[count];
            _partEndOffsets = new int[count];

            for (int i = 0; i < count; i++) {
                //命名空间使用索引，有利于减少重复字符串的描述。
                objNamespace = _strings[_reader.Read7BitInt()];
                //由于同一个命名空间下名称重复的概率很低，所以这里直接存储字符串，而不是索引。
                name = _reader.ReadString();
                _objectIds[i] = new FullName(objNamespace, name);
                //数据的结束位置（bytes，注意是_data的相对位置，而不是流中的位置）
                _partEndOffsets[i] = _reader.Read7BitInt();
            }
        }

        protected void ReadData() {
            //我们不依赖于流的长度作为数据的大小描述，最后一个数据其实描述了最大的数据块大小。
            var dataSize = _partEndOffsets.Length == 0 ? 0 : _partEndOffsets[_partEndOffsets.Length - 1];
            _data = new byte[dataSize];
            _reader.Read(_data, 0, dataSize);
        }

        /// <summary>
        /// 返回所有的零件。
        /// </summary>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<FullName, ConfigurationObjectPart>> GetParts() {
            //枚举所有的零件
            for (int i = 0; i < _objectIds.Length; i++) {
                //数据的开始和大小
                yield return new KeyValuePair<FullName, ConfigurationObjectPart>(
                    _objectIds[i],
                    CreatePart(GetPartData(i)));
            }
        }

        private ArraySegment<byte> GetPartData(int index) {
            var offset = (index == 0) ? 0 : _partEndOffsets[index - 1];
            var count = _partEndOffsets[index] - offset;
            return new ArraySegment<byte>(_data, offset, count);
        }

        protected virtual ConfigurationObjectPart CreatePart(ArraySegment<byte> data) {
            return new DcPart((DcPackage)_sourcePackage, data);
        }
    }
}
