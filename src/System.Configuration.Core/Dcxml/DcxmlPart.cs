using System.Collections.Generic;
using System.Configuration.Core.Collections;
using System.Configuration.Core.Metadata;
using System.Globalization;
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

                if (item.HasElements) {
                    SetLocalValue(field, GetListValue(item.Elements(), field));
                }
                else {
                    SetLocalValue(field, GetItemValue(item, field));
                }
            }

            //检索 x:base 设置，设置到Base属性中。
            var baseAttribute = _data.Attribute(DcxmlRepository.DcxmlName_Base);
            if (baseAttribute != null) {
                //如果未设置，代表没有设置策略。如果有设置，有可能是空
                SetLocalValue(BasePropertyInstance, new ObjectPtr(_file.Usings.GetQualifiedName(baseAttribute.Value)));
            }

            //todo:释放_data引用，目前只能等到所有Part都解包才能释放，没有实际意义，只能日后用DataReader实现。
        }

        private object GetListValue(IEnumerable<XElement> elements, IProperty field) {
            //集合
            OnlyNextList<ListDifferenceItem> list = new OnlyNextList<ListDifferenceItem>();

            foreach (var item in elements) {
                ListDifferenceAction action = GetListItemAction(field, item);
                var itemValue = GetListItemValue(field, item);
                list.Add(new ListDifferenceItem(action, itemValue));
            }

            return list;
        }

        private object GetListItemValue(IProperty field, XElement item) {
            //对象指针，要包装成指针对象。
            //ex: <Button x:ref="btnOK"/>
            var refAttribute = item.Attribute(DcxmlRepository.DcxmlName_Ref);
            if (refAttribute != null) {
                var refValue = refAttribute.Value;
                if (string.IsNullOrWhiteSpace(refValue)) {
                    Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                           "属性 {0} 描述为集合，但其明细项{1}的对象指针为空。", field.Name, DcxmlFile.GetXObjectDebugString(item, refAttribute)));
                }

                return new ObjectPtr(_file.Usings.GetQualifiedName(refValue));
            }
            else {
                //目前只能允许是对象指针节点，不允许出现下面类型的节点。
                //ex: <String />
                //ex: <Int32>17</Int32>
                //ex: <Button><Text>OK</Text></Button>

                //为什么不允许直接的值类型呢？
                //如果希望添加的值是重复的数据（例如37，37），如何处理呢？添加到集合很简单，如果是删除，那么应该删除一个37呢？还是删除全部37？
                //在这个问题还没有解决前，先不放开此功能。

                //为什么不能直接嵌入对象呢？
                //会造成Dcxml的复杂性，从一开始设计，我们就希望对象是完全平铺的。
                Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                           "属性 {0} 描述为集合，其明细项{1}不是一个对象指针。", field.Name, DcxmlFile.GetXObjectDebugString(item, refAttribute)));
            }

            return null;
        }

        private static ListDifferenceAction GetListItemAction(IProperty field, XElement item) {
            var action = ListDifferenceAction.Add;
            //ex: <Button x:action="remove"/>
            var actionAttribute = item.Attribute(DcxmlRepository.DcxmlName_ListAction); //默认add
            if (actionAttribute != null) {
                var actionValue = actionAttribute.Value;
                if (string.Equals(actionValue, "remove", StringComparison.OrdinalIgnoreCase)) {
                    action = ListDifferenceAction.Remove;
                }
                else if (string.Equals(actionValue, "add", StringComparison.OrdinalIgnoreCase)) {
                    action = ListDifferenceAction.Add;
                }
                else {
                    Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                           "属性 {0} 描述为集合，其明细项{1}的action不正确，只能是add和remove.", field.Name, DcxmlFile.GetXObjectDebugString(item, actionAttribute)));
                }
            }

            return action;
        }

        private object GetItemValue(XElement item,IProperty field) {
            //如果是对象指针，要包装成指针对象。
            //ex: <BackgroundImage x:ref="imgSky"/>
            var refAttribute = item.Attribute(DcxmlRepository.DcxmlName_Ref);
            if (refAttribute != null) {
                return new ObjectPtr(_file.Usings.GetQualifiedName(refAttribute.Value));
            }
            else {
                //空节点
                //ex: <Text />
                if (item.IsEmpty) {
                    return field.DefaultValue;
                }
                else {
                    //字符串需要转换器转换成对应的值。
                    //ex: <Anchor>Right,Bottom</Anchor>
                    return field.PropertyType.GetConverter().ConvertFromInvariantString(item.Value);
                }
            }
        }

    }
}