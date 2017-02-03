namespace System.Configuration.Core {
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
    /// 定义了类型信息的检索名称，派生类允许自定义不同的配置类型。
    /// </summary>
    public abstract class ObjectTypeQualifiedName : IEquatable<ObjectTypeQualifiedName> {

        /// <summary>
        /// 派生类调用此构造传入类型的识别名称。
        /// </summary>
        /// <param name="qualifiedName">类型的限定名称。</param>
        protected ObjectTypeQualifiedName(QualifiedName qualifiedName) {
            this._qualifiedName = qualifiedName;
        }

        #region 属性
        /// <summary>
        /// 指示提供类型信息的提供方，比如 clr-namespace。
        /// </summary>
        public abstract string ProviderName { get; }

        /// <summary>
        /// 从当前实例中复制出一个新的对象，并赋值name
        /// </summary>
        /// <param name="name">新的名称。</param>
        /// <returns>新的实例</returns>
        protected abstract ObjectTypeQualifiedName Clone(string name);

        private readonly QualifiedName _qualifiedName;
        /// <summary>
        /// 返回所在的包。
        /// </summary>
        public string PackageName {
            get { return _qualifiedName.PackageName; }
        }

        /// <summary>
        /// 返回命名空间和名称
        /// </summary>
        public FullName FullName {
            get { return this._qualifiedName.FullName; }
        }

        /// <summary>
        /// 返回命名空间
        /// </summary>
        public string Namespace {
            get { return this._qualifiedName.FullName.Namespace; }
        }

        /// <summary>
        /// 返回名称。
        /// </summary>
        public string Name {
            get { return this._qualifiedName.FullName.Name; }
        }

        /// <summary>
        /// 返回配置对象类型的完整识别名称，例如：company.erp.demo.ui.Window,company.erp.demo.ui
        /// </summary>
        public QualifiedName QualifiedName {
            get { return this._qualifiedName; }
        }
        #endregion

        #region 创建

        /// <summary>
        /// 在解析dcxml文件时，xmlns描述了诸如：xmlns="clr-namespace:company.erp.demo.ui,company.erp.demo.ui"这样的字符串，他未包含具体的名称，需要通过CreateByName创建具体的名称。
        /// </summary>
        /// <param name="str">一个包含ProvoderName，Namespace和PackageName，但没有描述具体的名称。</param>
        /// <returns>一个不包含名称的描述对象</returns>
        public static ObjectTypeQualifiedName CreateWithoutName(string str) {
            if (string.IsNullOrEmpty(str)) {
                Utilities.ThrowArgumentNull(nameof(str));
            }

            //获取clr-namespace这样的提供者字符串
            var providerNameEndIndex = str.IndexOf(':');
            string providerName;
            if (providerNameEndIndex < 0) {
                //可以没有标注，表示没有提供者。
                providerName = string.Empty;
                providerNameEndIndex = -1;
            }
            else {
                providerName = string.Intern((str.Substring(0, providerNameEndIndex).Trim()).ToLowerInvariant());
            }
            providerNameEndIndex++;

            //获取命名空间
            string objNamespace;
            string packageName;
            var objNamespaceEndIndex = str.IndexOf(',', providerNameEndIndex);
            if (objNamespaceEndIndex < 0) {
                //可以没有命名空间，后续部分就完全是包名称。
                objNamespace = null;
                packageName = str.Substring(providerNameEndIndex);
            }
            else {
                //逗号前面是命名空间，后面是包
                objNamespace = string.Intern(str.Substring(providerNameEndIndex, objNamespaceEndIndex - providerNameEndIndex).Trim());
                packageName = str.Substring(objNamespaceEndIndex + 1);
            }
            packageName = string.Intern((packageName.Trim()).ToLowerInvariant());

            return Create(providerName, objNamespace, null, packageName);
        }

        /// <summary>
        /// 创建新的实例。
        /// </summary>
        /// <param name="providerName">提供者</param>
        /// <param name="objNamespace">命名空间</param>
        /// <param name="name">名称</param>
        /// <param name="packageName">包的名称</param>
        /// <returns>新的实例</returns>
        public static ObjectTypeQualifiedName Create(string providerName, string objNamespace, string name, string packageName) {
            if (string.Equals(providerName, ClrProviderName, StringComparison.OrdinalIgnoreCase)) {
                return new ClrTypeQualifiedName(new QualifiedName(objNamespace, name, packageName));
            }
            else if (string.Equals(providerName, ClrInterfaceProviderName, StringComparison.OrdinalIgnoreCase)) {
                return new ClrInterfaceTypeQualifiedName(new QualifiedName(objNamespace, name, packageName));
            }
            else {
                return new BasicTypeQualifiedName(providerName, new QualifiedName(objNamespace, name, packageName));
            }
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

            return Clone(name);
        }

        /// <summary>
        /// 使用clr类进行配置对象类型信息绑定的形式，例如在dcxml中指定xmlns="clr-namespace:System.Configuration.Core.Tests,System.Configuration.Core.Tests"
        /// </summary>
        public readonly static string ClrProviderName = string.Intern("clr-namespace");

        /// <summary>
        /// 使用clr接口进行配置对象类型信息绑定的形式，例如在dcxml中指定xmlns="iclr-namespace:System.Configuration.Core.Tests,System.Configuration.Core.Tests"
        /// </summary>
        public readonly static string ClrInterfaceProviderName = string.Intern("iclr-namespace");

        /// <summary>
        /// 内置clr-namespace的处理逻辑，减少对象ProviderName这个字段的占用。
        /// </summary>
        private sealed class ClrTypeQualifiedName : ObjectTypeQualifiedName {
            public ClrTypeQualifiedName(QualifiedName qualifiedName) : base(qualifiedName) { }

            public override string ProviderName {
                get { return ClrProviderName; }
            }

            protected override ObjectTypeQualifiedName Clone(string name) {
                return new ClrTypeQualifiedName(new QualifiedName(this.FullName.Namespace, name, this.PackageName));
            }
        }

        /// <summary>
        /// 内置iclr-namespace的处理逻辑，减少对象ProviderName这个字段的占用。
        /// </summary>
        private sealed class ClrInterfaceTypeQualifiedName : ObjectTypeQualifiedName {
            public ClrInterfaceTypeQualifiedName(QualifiedName qualifiedName) : base(qualifiedName) { }

            public override string ProviderName {
                get { return ClrInterfaceProviderName; }
            }
            protected override ObjectTypeQualifiedName Clone(string name) {
                return new ClrInterfaceTypeQualifiedName(new QualifiedName(this.FullName.Namespace, name, this.PackageName));
            }
        }

        /// <summary>
        /// 普通的第三方提供者，必须占用一个字段存储ProviderName
        /// </summary>
        private sealed class BasicTypeQualifiedName : ObjectTypeQualifiedName {

            public BasicTypeQualifiedName(string providerName, QualifiedName qualifiedName) : base(qualifiedName) {
                _providerName = providerName;
            }

            private readonly string _providerName;
            /// <summary>
            /// 指示提供类型信息的提供方，比如 clr-namespace。注意：此信息不参与相等判断。
            /// </summary>
            public override string ProviderName {
                get { return _providerName; }
            }
            protected override ObjectTypeQualifiedName Clone(string name) {
                return new BasicTypeQualifiedName(this.PackageName, new QualifiedName(this.FullName.Namespace, name, this.PackageName));
            }
        }
        #endregion

        /// <summary>
        /// 返回此类型的字符串描述，注意注意
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            var qn = this._qualifiedName.ToString();
            return string.IsNullOrEmpty(ProviderName) ? qn : this.ProviderName + ":" + qn;
        }

        public static bool operator ==(ObjectTypeQualifiedName x, ObjectTypeQualifiedName y) {
            if (object.ReferenceEquals(x, y)) {
                return true;
            }

            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null) || x.ProviderName != y.ProviderName) {
                return false;
            }

            return x._qualifiedName == y._qualifiedName;
        }

        public static bool operator !=(ObjectTypeQualifiedName x, ObjectTypeQualifiedName y) {
            return !(x == y);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(this, obj)) {
                return true;
            }

            ObjectTypeQualifiedName qn = obj as ObjectTypeQualifiedName;
            return this.Equals(qn);
        }

        public override int GetHashCode() {
            var hc = this.ProviderName == null ? -7 : this.ProviderName.GetHashCode();
            return hc ^ this._qualifiedName.GetHashCode();
        }

        public bool Equals(ObjectTypeQualifiedName other) {
            if (object.ReferenceEquals(this, other)) {
                return true;
            }

            if (object.ReferenceEquals(other,null)) {
                return false;
            }
            
            return this.ProviderName == other.ProviderName && this._qualifiedName == other._qualifiedName;
        }
    }
}
