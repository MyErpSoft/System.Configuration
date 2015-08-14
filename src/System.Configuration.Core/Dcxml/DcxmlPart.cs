using System.Xml.Linq;

namespace System.Configuration.Core.Dcxml {

    internal class DcxmlPart : BasicPart {
        private XElement _data;
        private DcxmlFile _file;

        public DcxmlPart(DcxmlFile file, ObjectTypeQualifiedName typeName, XElement data):base(typeName) {
            this._file = file;
            this._data = data;
        }
        
        protected override void OpenDataCore(OpenDataContext ctx) {
            base.OpenDataCore(ctx);
            var dt = this.Type;

            foreach (var item in _data.Elements()) {
                var field = dt.GetProperty(item.Name.LocalName);
                SetLocalValue(field, item.Value); //todo:字符串需要转换器转换成对应的值。
                //todo: 如果是对象指针，要包装成指针对象。
            }

            //检索 x:base 设置，设置到Base属性中。
            var baseAttrbute = _data.Attribute(DcxmlFile.xNamespace + "base");
            if (baseAttrbute != null) {
                //如果未设置，代表没有设置策略。如果有设置，有可能是空
                SetLocalValue(BasePropertyInstance, new ObjectPtr(_file.Usings.GetQualifiedName(baseAttrbute.Value)));
            }
        }
    }
}