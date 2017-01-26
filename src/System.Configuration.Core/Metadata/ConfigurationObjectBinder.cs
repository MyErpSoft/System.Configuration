namespace System.Configuration.Core.Metadata {

    /// <summary>
    /// 默认的配置对象绑定器。
    /// </summary>
    public abstract class ConfigurationObjectBinder {

        /// <summary>
        /// 返回此Binder的唯一识别名称。
        /// </summary>
        public abstract string Name { get; }

        public abstract bool TryBindToType(ObjectTypeQualifiedName name, out IType type);
        
    }

    internal sealed class ClrConfigurationObjectBinder : ConfigurationObjectBinder {
        public override string Name { get { return ObjectTypeQualifiedName.ClrProviderName; } }

        public override bool TryBindToType(ObjectTypeQualifiedName name, out IType type) {
            //todo:安全性检测，不然配置文件可以任意创建一个对象并设置其属性值。
            Type dt = Type.GetType(name.QualifiedName.ToString(), false);
            if (dt != null) {
                type = (IType)Clr.ClrType.GetClrType(dt);
                return true;
            }

            type = null;
            return false;
        }
    }

    internal sealed class ClrInterfaceConfigurationObjectBinder : ConfigurationObjectBinder {
        public override string Name { get { return ObjectTypeQualifiedName.ClrInterfaceProviderName; } }

        public override bool TryBindToType(ObjectTypeQualifiedName name, out IType type) {
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
    }
}
