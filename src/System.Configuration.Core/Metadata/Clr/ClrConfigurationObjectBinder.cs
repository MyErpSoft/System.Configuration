using System.Collections.Generic;

namespace System.Configuration.Core.Metadata.Clr {

    internal sealed class ClrConfigurationObjectBinder : IConfigurationObjectBinder {

        public IEnumerable<string> SupportedNames { get { yield return ObjectTypeQualifiedName.ClrProviderName; } }

        public bool TryBindToType(ObjectTypeQualifiedName name, out IType type) {
            //todo:安全性检测，不然配置文件可以任意创建一个对象并设置其属性值。
            Type dt = Type.GetType(name.QualifiedName.ToString(), false);
            if (dt != null) {
                type = (IType)Clr.ClrType.GetClrType(dt);
                return true;
            }

            type = null;
            return false;
        }

        public override string ToString() {
            return ObjectTypeQualifiedName.ClrProviderName;
        }
    }
}
