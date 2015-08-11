﻿namespace System.Configuration.Core {
    /*
    在下面的dcxml示例中，Window的完整字符串是：clr-namespace:company.erp.demo.ui.Window,company.erp.demo.ui 其中，
    providerName        = clr-namespace
    namespace           = company.erp.demo.ui
    name                = Window
    assemblyName        = company.erp.demo.ui

    <x:ObjectContainer
       xmlns="clr-namespace:company.erp.demo.ui,company.erp.demo.ui"
       xmlns:x="http://schemas.myerpsoft.com/configuration/2015"
       xmlns:y="clr-namespace:myui,myui.test"
       namespace="company.erp.demo" >

      <Window x:name="frmMain" x:base="BasicForm" >
        <Text>demo</Text>
      </Window>

    </x:ObjectContainer>
    */

    /// <summary>
    /// 定义了类型信息的检索名称
    /// </summary>
    public sealed class ObjectTypeQualifiedName : QualifiedName {

        public ObjectTypeQualifiedName(string providerName, string objNamespace, string name, string packageName)
            : this(providerName, new FullName(objNamespace, name), packageName) {
        }

        public ObjectTypeQualifiedName(string providerName, FullName fullName, string packageName)
            : base(fullName, packageName) {
            this._providerName = providerName;
        }

        private readonly string _providerName;
        /// <summary>
        /// 指示提供类型信息的提供方，比如 clr-namespace。注意：此信息不参与相等判断。
        /// </summary>
        public string ProviderName {
            get { return _providerName; }
        }

        public static ObjectTypeQualifiedName CreateWithoutName(string str) {
            if (string.IsNullOrEmpty(str)) {
                Utilities.ThrowArgumentNull(nameof(str));
            }

            //获取clr-namespace这样的提供者字符串
            var providerNameEndIndex = str.IndexOf(':');
            string providerName;
            if (providerNameEndIndex < 0) {
                //可以没有标注，表示没有提供者。
                providerName = null;
                providerNameEndIndex = -1;
            }
            else {
                providerName = string.Intern(str.Substring(0, providerNameEndIndex).Trim());
            }
            providerNameEndIndex++;

            //获取命名空间
            string objNamespace;
            string packageName;
            var objNamespaceEndIndex = str.IndexOf(',', providerNameEndIndex);
            if (objNamespaceEndIndex < 0) {
                //可以没有命名空间，后续部分就完全是包名称。
                objNamespace = null;
                packageName = string.Intern(str.Substring(providerNameEndIndex).Trim());
            }
            else {
                //逗号前面是命名空间，后面是包
                objNamespace = string.Intern(str.Substring(providerNameEndIndex, objNamespaceEndIndex - providerNameEndIndex).Trim());
                packageName = string.Intern(str.Substring(objNamespaceEndIndex + 1).Trim());
            }

            return new ObjectTypeQualifiedName(providerName, new FullName(objNamespace, null), packageName);
        }

        /// <summary>
        /// 创建一个新的实例，此实例由CreateWithoutName静态方法创建的实例添加一个Name，适合Dcxml内部处理。
        /// </summary>
        /// <param name="name">新的对象类型名称</param>
        /// <returns>一个新的类型名称。</returns>
        public ObjectTypeQualifiedName CreateByName(string name) {
            if (!string.IsNullOrEmpty(this.FullName.Name)) {
                Utilities.ThrowNotSupported("必须是CreateWithoutName创建的实例才能调用此方法。");
            }
            if (!Utilities.VerifyName(name)) {
                Utilities.ThrowArgumentException("不正确的类型名称。", nameof(name));
            }

            return new ObjectTypeQualifiedName(this._providerName, new FullName(this.FullName.Namespace, name), this.PackageName);
        }

        //注意：此信息不参与相等判断，无论来源哪里，都使用标准的相等判断。
    }
}