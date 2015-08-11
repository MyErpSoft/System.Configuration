using System.Xml.Linq;

namespace System.Configuration.Core.Dcxml {

    internal class DcxmlPart : BasicPart {
        private XElement _data;
        private ObjectTypeQualifiedName _typeName;
        private DcxmlFile _file;

        public DcxmlPart(DcxmlFile file, ObjectTypeQualifiedName typeName, XElement data) {
            this._file = file;
            this._typeName = typeName;
            this._data = data;
        }
        
        protected override void OpenDataCore(GetPartContext ctx) {
            var dt = ctx.Binder.BindToType(_typeName);
            foreach (var item in _data.Elements()) {
                var field = dt.Fields[item.Name.LocalName];
                SetLocalValue(field, item.Value); //todo:字符串需要转换器转换成对应的值。
                //todo: 如果是对象指针，要包装成指针对象。
            }

            //todo:检索 x:base 设置，设置到Base属性中。
        }
    }
}