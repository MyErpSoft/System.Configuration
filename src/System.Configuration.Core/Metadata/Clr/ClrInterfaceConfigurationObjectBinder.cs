using System.Collections.Generic;

namespace System.Configuration.Core.Metadata.Clr {

    internal sealed class ClrInterfaceConfigurationObjectBinder : IConfigurationObjectBinder {

        public IEnumerable<string> SupportedNames { get { yield return ObjectTypeQualifiedName.ClrInterfaceProviderName; } }

        public bool TryBindToType(ObjectTypeQualifiedName name, out IType type) {
            //todo:安全性检测，不然配置文件可以任意创建一个对象并设置其属性值。
            Type dt = Type.GetType(ConvertInterfaceName(name.QualifiedName), false);
            if (dt != null) {
                type = (IType)Clr.ClrType.GetClrType(dt);
                return true;
            }

            type = null;
            return false;
        }

        private static string ConvertInterfaceName(QualifiedName qName) {
            var name = qName.FullName.Name;
            //类似Window，添加前缀I，变成IWindow
            return new QualifiedName(qName.FullName.Namespace, "I" + name, qName.PackageName).ToString();
        }

        public override string ToString() {
            return ObjectTypeQualifiedName.ClrInterfaceProviderName;
        }
    }
}
