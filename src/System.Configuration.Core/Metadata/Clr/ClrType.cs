using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace System.Configuration.Core.Metadata.Clr {

    /// <summary>
    /// 使用IType接口来描述.net运行时类型(Type)。
    /// </summary>
    public class ClrType : MemberMetadataBase<Type>, IType {

        #region ClrType

        private readonly static ConcurrentDictionary<Type, ClrType> _entityTypeCaches =
            new ConcurrentDictionary<Type, ClrType>();

        /// <summary>
        /// 获取一个使用ClrType描述的Type实例。
        /// </summary>
        /// <param name="clrType">要检索的Type实例。</param>
        /// <returns>一个ClrType实例。</returns>
        public static ClrType GetClrType(Type clrType) {
            if (null == clrType) {
                return null;
            }

            if ((clrType.Assembly == null) || (clrType.Assembly.IsDynamic)) {
                return Parse(clrType);
            }

            //在并发环境中，同一个clrType可能造成多次调用Parse，但仅仅采纳一个值，并没有副作用。
            return _entityTypeCaches.GetOrAdd(clrType, Parse);
        }

        //暂时还不打算开放自定义的ClrType的派生实现，等有需求时才公开且允许外界重载。
        private static ClrType Parse(Type clrType) {
            return new ClrType(clrType);
        }
        #endregion

        internal ClrType(Type clrType)
            : base(clrType) { }

        private ObjectTypeQualifiedName _qualifiedName;
        /// <summary>
        /// 返回此类型的完整名称。
        /// </summary>
        public ObjectTypeQualifiedName QualifiedName {
            get {
                if (_qualifiedName == null) {
                    _qualifiedName = ObjectTypeQualifiedName.Create(
                        ObjectTypeQualifiedName.ClrProviderName,
                        this.ClrMapping.Namespace,
                        this.ClrMapping.Name,
                        this.ClrMapping.Assembly.FullName //注意这里可能包含版本，时区等信息，例如：ExampleAssembly, Version=1.0.0.0, Culture=en, PublicKeyToken=a5d015c7d5a0b012
                        );
                }

                return _qualifiedName;
            }
        }

        public ClrProperty GetProperty(string name) {
            ClrProperty property;
            if (TryGetProperty(name, out property)) {
                return property;
            }

            Utilities.ThrowKeyNotFoundException(string.Format(CultureInfo.CurrentCulture,
                Core.Properties.Resources.KeyNotFoundException, this.Name, name));
            return null;
        }

        private PropertyCollection _properties;
        /// <summary>
        /// 尝试获取指定名称的成员
        /// </summary>
        /// <param name="name">要检索的成员名称</param>
        /// <param name="property">如果找到将返回他，否则返回null</param>
        /// <returns>如果找到将返回true，否则返回false.</returns>
        public bool TryGetProperty(string name, out ClrProperty property) {
            if (string.IsNullOrEmpty(name)) {
                property = null;
                return false;
            }

            //从已加载的集合中获取         
            return Properties.TryGetValue(name, out property);
        }

        private PropertyCollection Properties {
            get {
                if (_properties == null) {
                    Interlocked.CompareExchange(ref _properties, CreateProperties(), null);
                }

                return _properties;
            }
        }

        private PropertyCollection CreateProperties() {
            //获取基类，这样可用重用基类的属性描述符。
            Dictionary<string, ClrProperty> properties;
            
            //接口是没有基类
            if (this.ClrMapping.IsInterface) {
                var interfaces = this.ClrMapping.GetInterfaces();
                properties = new Dictionary<string, ClrProperty>(StringComparer.Ordinal);

                if (interfaces != null && interfaces.Length > 0) {
                    foreach (var baseInterface in interfaces) {
                        var baseType = GetClrType(baseInterface);
                        foreach (var item in baseType.Properties) {
                            properties.Add(item.Name, item);
                        }
                    }
                }
            }else {
                var baseClrType = this.ClrMapping.BaseType;
                if (baseClrType != typeof(object)) {
                    var baseType = GetClrType(baseClrType);
                    properties = new Dictionary<string, ClrProperty>(baseType.Properties.Count, StringComparer.Ordinal);
                    foreach (var item in baseType.Properties) {
                        properties.Add(item.Name, item);
                    }
                }
                else {
                    properties = new Dictionary<string, ClrProperty>(StringComparer.Ordinal);
                }
            }

            //获取自身的属性描述符，注意： GetClrMembers仅仅返回当前类型声明的属性。
            var currentProperties = from clrProperty in GetClrMembers()
                                    select CreatePropertyMetadata(clrProperty);
            foreach (var item in currentProperties) {
                if (item != null) {//有可能CreatePropertyMetadata返回null，表示过滤此属性。
                    properties[item.Name] = item; //当派生类 override 属性时，需要冲掉基类的处理。
                }
            }

            return new PropertyCollection(properties);
        }

        /// <summary>
        /// 派生类可以重载此方法，过滤不希望公开的属性或字段
        /// </summary>
        /// <param name="name">要获取的字段或属性的名称</param>
        /// <returns>如果找到并确认公开，返回其实例，否则返回null.</returns>
        protected virtual IEnumerable<MemberInfo> GetClrMembers() {
            return this.ClrMapping.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        }

        /// <summary>
        /// 根据Clr成员信息创建属性对象。
        /// </summary>
        /// <param name="clrMember">一个clr成员</param>
        /// <returns>创建成功的成员</returns>
        protected virtual ClrProperty CreatePropertyMetadata(MemberInfo clrMember) {
            PropertyInfo p = clrMember as PropertyInfo;
            if (p != null) {
                return new ClrProperty(p);
            }
            else {
                return null;
            }
        }

        private TypeConverter _converter;
        /// <summary>
        /// 获取此类型的转换器，用于字符串到此类型的互相转换。
        /// </summary>
        /// <returns>一个转换器实例。</returns>
        public TypeConverter GetConverter() {
            if (_converter == null) {
                Interlocked.CompareExchange(ref _converter, TypeDescriptor.GetConverter(this.ClrMapping), null);
            }

            return _converter;
        }

        #region IType
        
        IProperty IType.GetProperty(string name) {
            return this.GetProperty(name);
        }

        internal Type MappingType {
            get { return this.ClrMapping; }
        }

        private sealed class PropertyCollection : ReadOnlyKeyedCollection<string, ClrProperty> {
            public PropertyCollection(Dictionary<string,ClrProperty> properties) : base(properties) {
            }

            protected override string GetKeyForItem(ClrProperty item) {
                return item.Name;
            }
        }

        #endregion

    }
}
