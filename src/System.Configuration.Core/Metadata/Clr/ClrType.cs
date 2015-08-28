using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace System.Configuration.Core.Metadata.Clr {

    public class ClrType : MemberMetadataBase<Type>, IType {

        #region ClrType

        private readonly static ConcurrentDictionary<Type, ClrType> _entityTypeCaches =
            new ConcurrentDictionary<Type, ClrType>();

        public static ClrType GetClrType(Type clrType) {
            if (null == clrType) {
                return null;
            }

            ClrType entityType;
            if ((clrType.Assembly == null) || (clrType.Assembly.IsDynamic)) {
                entityType = new ClrType(clrType);
                return entityType;
            }

            //在并发环境中，同一个clrType可能造成多次调用CreateEntityType，但仅仅采纳一个值，并没有副作用。
            return _entityTypeCaches.GetOrAdd(clrType, Parse);
        }

        public static ClrType Parse(Type clrType) {
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
                    _qualifiedName = new ObjectTypeQualifiedName(
                        ConfigurationObjectBinder.ClrBinderProviderName,
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
                Properties.Resources.KeyNotFoundException, this.Name, name));
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
            if (_properties != null && _properties.TryGetValue(name, out property)) {
                return true;
            }

            //反射获取成员信息。
            var clrMemberInfo = GetClrMember(name);
            if (clrMemberInfo == null) {
                property = null;
                return false;
            }

            //加入缓存
            return this.TryGetPropertyCore(clrMemberInfo, out property);
        }

        /// <summary>
        /// 派生类可以重载此方法，过滤不希望公开的属性或字段
        /// </summary>
        /// <param name="name">要获取的字段或属性的名称</param>
        /// <returns>如果找到并确认公开，返回其实例，否则返回null.</returns>
        protected virtual MemberInfo GetClrMember(string name) {
            return this.ClrMapping.GetProperty(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private bool TryGetPropertyCore(MemberInfo clrMember, out ClrProperty property) {
            //如果成员来自基类，那么应该使用基类的对象，这样可以节约Member对象的数量。
            lock (this) {
                if (_properties != null && _properties.TryGetValue(clrMember.Name, out property)) {
                    return true;
                }

                //如果成员定义在基类，那么应该从基类获取，这样可以大大减少Property的描述对象
                var declaringType = clrMember.DeclaringType;
                if (declaringType != this.ClrMapping) {
                    //这里调用TryGetPropertyCore，目的是减少一次反射，由于TryGetPropertyCore第一句话还会检查缓存，所以还是会使用缓存的。
                    if (!ClrType.GetClrType(declaringType).TryGetPropertyCore(clrMember, out property)) {
                        return false; //基类可能认为此属性不适合公开
                    }
                }
                else {
                    property = this.CreatePropertyMetadata(clrMember);
                }

                if (_properties == null) {
                    _properties = new PropertyCollection();
                }
                _properties.Add(property);
                return true;
            }
        }

        /// <summary>
        /// 根据Clr成员信息创建属性对象。
        /// </summary>
        /// <param name="clrMember">一个clr成员</param>
        /// <returns>创建成功的成员</returns>
        protected ClrProperty CreatePropertyMetadata(MemberInfo clrMember) {
            PropertyInfo p = clrMember as PropertyInfo;
            if (p != null) {
                return new ClrProperty(p);
            }
            else {
                throw new NotSupportedException();
            }
        }

        private TypeConverter _converter;
        /// <summary>
        /// 获取此类型的转换器，用于字符串到此类型的互相转换。
        /// </summary>
        /// <returns>一个转换器实例。</returns>
        public TypeConverter GetConverter() {
            if (_converter == null) {
                _converter = TypeDescriptor.GetConverter(this.ClrMapping);
            }

            return _converter;
        }

        IProperty IType.GetProperty(string name) {
            return this.GetProperty(name);
        }

        internal Type MappingType {
            get { return this.ClrMapping; }
        }

        private sealed class PropertyCollection : ReadOnlyKeyedCollection<string, ClrProperty> {
            public PropertyCollection() : base(null) {
            }

            protected override string GetKeyForItem(ClrProperty item) {
                return item.Name;
            }

            /// <summary>
            /// Allows a derived class to add data to the collection.
            /// </summary>
            /// <param name="items">To add a collection of elements</param>
            internal void AddRange(IEnumerable<ClrProperty> items) {
                InsertItems(items);
            }

            /// <summary>
            /// Allows a derived class to add data.
            /// </summary>
            /// <param name="item">The element to be added.</param>
            internal void Add(ClrProperty item) {
                InsertItem(item);
            }
        }
    }
}
