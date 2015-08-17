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
                //如果是对象指针，要包装成指针对象。
                var refAttribute = item.Attribute(DcxmlRepository.DcxmlName_Ref);
                if (refAttribute != null) {
                    SetLocalValue(field, new ObjectPtr(_file.Usings.GetQualifiedName(refAttribute.Value)));
                }
                else {
                    //空节点
                    if (item.IsEmpty) {
                        SetLocalValue(field, field.DefaultValue);
                    }
                    else {
                        //字符串需要转换器转换成对应的值。
                        var value = field.PropertyType.GetConverter().ConvertFromInvariantString(item.Value);
                        SetLocalValue(field, value);
                    }
                }
            }

            //检索 x:base 设置，设置到Base属性中。
            var baseAttribute = _data.Attribute(DcxmlRepository.DcxmlName_Base);
            if (baseAttribute != null) {
                //如果未设置，代表没有设置策略。如果有设置，有可能是空
                SetLocalValue(BasePropertyInstance, new ObjectPtr(_file.Usings.GetQualifiedName(baseAttribute.Value)));
            }
        }
    }
}