using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration.Core.Metadata;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Core.Tests {

    [AttributeUsage(AttributeTargets.Class)]
    sealed class DataEntityRegisterAttribute : Attribute { }

    class LambdaDependencyAmbientValueAttribute : Attribute {
        public LambdaDependencyAmbientValueAttribute(string str) {
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    sealed class DataEntityAttribute : MetadataInfoAttribute {
        public DataEntityAttribute() : this(null, null) { }
        public DataEntityAttribute(string primaryKey) : this(primaryKey, null) { }
        public DataEntityAttribute(string primaryKey, string alias) {
            this.PrimaryKey = primaryKey;
        }

        public string PrimaryKey { get; private set; }
    }
    

    class DependencyObjectView {
        public DependencyObjectView(): this(null) {
        }

        public DependencyObjectView(DependencyObject obj) {
            this.DependencyObject = obj;
        }

        public DependencyObject DependencyObject { get; set; }

        protected CachedViewPtr<T> RegisterComplexViewPtr<T>(DependencyProperty itemProperty) where T : DependencyObjectView, new() {
            return new CachedComplexViewPtr<T>(this.DependencyObject, itemProperty);
        }

        protected CachedViewPtr<DependencyObjectCollectionView<IT>> RegisterCollectionViewPtr<IT>(DependencyProperty itemProperty) where IT : DependencyObjectView, new() {
            return new CachedColletionViewPtr<IT>(this.DependencyObject, itemProperty);
        }
    }

    abstract class CachedViewPtr<T> {
        public abstract T Value { get; }
    }

    class CachedComplexViewPtr<T> : CachedViewPtr<T> where T : DependencyObjectView, new() {
        T _viewCache;
        DependencyProperty _itemProperty;
        DependencyObject _owner;

        public CachedComplexViewPtr(DependencyObject owner, DependencyProperty itemProperty) {
            #region 参数检查
            if (itemProperty == null) {
                throw new ArgumentNullException("itemProperty");
            }
            #endregion

            _owner = owner;
            _itemProperty = itemProperty;
        }

        public override T Value {
            get {
                var value = _itemProperty.GetValue(_owner);
                if (value == null) {
                    _viewCache = null;
                    return null;
                }
                else {
                    if (_viewCache == null || !object.ReferenceEquals(_viewCache.DependencyObject, value)) {
                        _viewCache = new T();
                        _viewCache.DependencyObject = (DependencyObject)value;
                    }

                    return _viewCache;
                }
            }
        }
    }

    class CachedColletionViewPtr<T> : CachedViewPtr<DependencyObjectCollectionView<T>> where T : DependencyObjectView, new() {
        DependencyObjectCollectionView<T> _viewCache;
        DependencyProperty _itemProperty;
        DependencyObject _owner;

        public CachedColletionViewPtr(DependencyObject owner, DependencyProperty itemProperty) {
            #region 参数检查
            if (itemProperty == null) {
                throw new ArgumentNullException("itemProperty");
            }
            #endregion

            _owner = owner;
            _itemProperty = itemProperty;
        }

        public override DependencyObjectCollectionView<T> Value {
            get {
                var value = _itemProperty.GetValue(_owner);
                if (value == null) {
                    _viewCache = null;
                    return null;
                }
                else {
                    if (_viewCache == null || !object.ReferenceEquals(_viewCache.Items, value)) {
                        _viewCache = new DependencyObjectCollectionView<T>();
                        _viewCache.Items = (IList<DependencyObject>)value;
                    }

                    return _viewCache;
                }
            }
        }
    }

    class DependencyObjectCollectionView<V> : IList<V> where V : DependencyObjectView, new() {

        private IList<DependencyObject> _items;
        public IList<DependencyObject> Items {
            get { return _items; }
            set { _items = value; }
        }

        public V this[int index] {
            get {
                var obj = _items[index];
                var view = new V();
                view.DependencyObject = obj;
                return view;
            }
            set {
                _items[index] = value.DependencyObject;
            }
        }

        public int Count {
            get { return _items.Count; }
        }

        public bool IsReadOnly {
            get { return _items.IsReadOnly; }
        }

        public void Add(V item) {
            _items.Add(item.DependencyObject);
        }

        public void Clear() {
            _items.Clear();
        }

        public bool Contains(V item) {
            return item != null && _items.Contains(item.DependencyObject);
        }

        public void CopyTo(V[] array, int arrayIndex) {
            for (int i = 0; i < _items.Count; i++) {
                array[i + arrayIndex] = this[i];
            }
        }

        public IEnumerator<V> GetEnumerator() {
            for (int i = 0; i < _items.Count; i++) {
                yield return this[i];
            }
        }

        public int IndexOf(V item) {
            if (item != null) {
                return _items.IndexOf(item.DependencyObject);
            }
            return -1;
        }

        public void Insert(int index, V item) {
            _items.Insert(index, item.DependencyObject);
        }

        public bool Remove(V item) {
            if (item != null) {
                return _items.Remove(item.DependencyObject);
            }
            return false;
        }

        public void RemoveAt(int index) {
            _items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
    
    abstract class DependencyProperty {
        protected DependencyProperty(string name) {
            this.Name = name;
        }

        public string Name { get; private set; }

        public abstract object GetValue(DependencyObject obj);

        public abstract void SetValue(DependencyObject obj, object value);

        public int Ordinal { get; internal set; }

        public abstract DependencyProperty Clone();
    }

    [DebuggerDisplay("{Name}:{PropertyType.Name}")]
    class SimpleDependencyProperty : DependencyProperty {
        public SimpleDependencyProperty(string name, Type propertyType,object defaultValue):base(name) {
            this.PropertyType = propertyType;
        }

        public Type PropertyType { get; private set; }

        public object DefaultValue { get; private set; }

        public override object GetValue(DependencyObject obj) {
            var value = obj.GetValueCore(this);
            return (value == null) ? DefaultValue : value;
        }

        public override void SetValue(DependencyObject obj, object value) {
            obj.SetValueCore(this, value);
        }

        public override DependencyProperty Clone() {
            return new SimpleDependencyProperty(this.Name, this.PropertyType, this.DefaultValue);
        }
    }

    [DebuggerDisplay("{Name}:{PropertyType.Name}")]
    class ComplexDependencyProperty : DependencyProperty {
        public ComplexDependencyProperty(string name, DependencyObjectType propertyType):base(name) {
            this.PropertyType = propertyType;
        }
        public DependencyObjectType PropertyType { get; private set; }

        public override object GetValue(DependencyObject obj) {
            return obj.GetValueCore(this);
        }

        public override void SetValue(DependencyObject obj, object value) {
            obj.SetValueCore(this, value);
        }

        public override DependencyProperty Clone() {
            return new ComplexDependencyProperty(this.Name, this.PropertyType);
        }
    }

    [DebuggerDisplay("{Name}:List<{ItemPropertyType.Name}>")]
    class CollectionDependencyProperty : DependencyProperty {
        public CollectionDependencyProperty(string name, DependencyObjectType itemPropertyType):base(name) {
            this.ItemPropertyType = itemPropertyType;
        }
        public DependencyObjectType ItemPropertyType { get; private set; }

        public override object GetValue(DependencyObject obj) {
            var value = obj.GetValueCore(this);
            if (value == null) {
                value = new List<DependencyObject>();
                obj.SetValueCore(this, value);
            }

            return value;
        }

        public override void SetValue(DependencyObject obj, object value) {
            throw new NotSupportedException();
        }

        public override DependencyProperty Clone() {
            return new CollectionDependencyProperty(this.Name, this.ItemPropertyType);
        }
    }

    class DependencyPropertyCollection : ReadOnlyCollection<DependencyProperty> {
        private Dictionary<string, DependencyProperty> _dict;
        public DependencyPropertyCollection(IList<DependencyProperty> items):base(items) {
            _dict = new Dictionary<string, Tests.DependencyProperty>();
            foreach (var item in items) {
                _dict.Add(item.Name, item);
            }
        }

        public DependencyProperty this[string name] {
            get { return _dict[name]; }
        }
    }

    [DebuggerDisplay("{Name}")]
    class DependencyObjectType {
        public DependencyObjectType(string name, Attribute[] atts = null, Type dependencyObjectType = null) {
            this.Name = name;
            this.ObjectType = dependencyObjectType;
            this._declaringProperties = new Dictionary<string, DependencyProperty>();
            this._baseItems = new List<DependencyObjectType>();
        }

        public Type ObjectType { get; private set; }
        public string Name { get; private set; }

        private DependencyPropertyCollection _readOnlyProperties;//所有属性
        private readonly Dictionary<string, DependencyProperty> _declaringProperties; //仅仅是本类型声明的属性清单，不包括基类的
        private readonly List<DependencyObjectType> _baseItems;

        public DependencyPropertyCollection Properties {
            get {
                if (_readOnlyProperties == null) {
                    //Ordinal设置
                    var allProperties = GetAllProperties().Values.ToArray();
                    for (int i = 0; i < allProperties.Length; i++) {
                        var property = allProperties[i];
                        if (!_declaringProperties.ContainsKey(property.Name)) {
                            property = property.Clone();
                            allProperties[i] = property;
                        }
                        property.Ordinal = i;
                    }
                    _readOnlyProperties = new DependencyPropertyCollection(allProperties);
                }

                return _readOnlyProperties;
            }
        }

        private Dictionary<string, DependencyProperty> GetAllProperties() {
            Dictionary<string, DependencyProperty> items = new Dictionary<string, DependencyProperty>(_declaringProperties);
            foreach (var baseItem in _baseItems) {
                foreach (var item in baseItem.GetAllProperties()) {
                    if (!items.ContainsKey(item.Key)) {
                        items.Add(item.Key, item.Value);
                    }
                }
            }

            return items;
        }

        public void RegisterBaseType(DependencyObjectType baseType) {
            _baseItems.Add(baseType);
        }

        public DependencyProperty RegisterSimpleProperty(string name, Type propertyType, object defaultValue, bool isReadOnly, params Attribute[] atts) {
            if (_readOnlyProperties != null) {
                throw new InvalidOperationException();
            }

            var property = new SimpleDependencyProperty(name, propertyType,defaultValue);
            _declaringProperties.Add(name, property);

            return property;
        }

        public DependencyProperty RegisterComplexProperty(string name, DependencyObjectType propertyType, bool isReadOnly, params Attribute[] atts) {
            if (_readOnlyProperties != null) {
                throw new InvalidOperationException();
            }

            var property = new ComplexDependencyProperty(name, propertyType);
            _declaringProperties.Add(name, property);

            return property;
        }

        public DependencyProperty RegisterCollectionProperty(string name, DependencyObjectType itemPropertyType, bool isReadOnly, params Attribute[] atts) {
            if (_readOnlyProperties != null) {
                throw new InvalidOperationException();
            }

            var property = new CollectionDependencyProperty(name, itemPropertyType);
            _declaringProperties.Add(name,property);

            return property;
        }
    }

    class ConfigurationDependencyObject {

    }

    class DependencyObject {
        private readonly static object[] EmptyArray = new object[0];
        private object[] _values;
        private readonly DependencyObjectType _dt;

        public DependencyObject(DependencyObjectType dt) {
            this._dt = dt;
            this._values = EmptyArray;
        }

        public object this[DependencyProperty property] {
            get { return property.GetValue(this); }
            set { property.SetValue(this, value); }
        }


        private void EnsureCapacity() {
            int count = _dt.Properties.Count;
            object[] newValues = new object[count];
            _values.CopyTo(newValues, 0);
            _values = newValues;
        }

        internal object GetValueCore(DependencyProperty property) {
            int ordinal = property.Ordinal;
            if (ordinal < _values.Length) {
                return this._values[ordinal];
            }
            else {
                return null;
            }
        }

        internal void SetValueCore(DependencyProperty property, object value) {
            int ordinal = property.Ordinal;
            if (_values.Length <= ordinal) {
                this.EnsureCapacity();
            }

            this._values[ordinal] = value;
        }

        void ClearValueCore(DependencyProperty property) {
            int ordinal = property.Ordinal;
            if (ordinal < _values.Length) {
                this._values[ordinal] = null;
            }
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    class MetadataInfoAttribute : Attribute {
        public string Description { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    class SimplePropertyAttribute : MetadataInfoAttribute {
        public SimplePropertyAttribute() : this(GeneralDBType.Auto, -1) { }
        public SimplePropertyAttribute(GeneralDBType dbType) : this(dbType, -1) { }
        public SimplePropertyAttribute(GeneralDBType dbType, int pSize) {
            this.DBType = dbType;
            this.Size = pSize;
        }

        //public AutoSync AutoSync { get; set; }
        public GeneralDBType DBType { get; set; }
        public bool IgnoreInsert { get; set; }
        public bool IgnoreUpdate { get; set; }
        public bool IsIndex { get; set; }
        public bool IsLocalizable { get; set; }
        //public CanNull IsNullable { get; set; }
        public bool IsUnique { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    sealed class ComplexPropertyAttribute : MetadataInfoAttribute {
    }

    [AttributeUsage(AttributeTargets.Property)]
    sealed class CollectionPropertyAttribute : MetadataInfoAttribute {
        public CollectionPropertyAttribute(Type itemType) {
            this.ItemType = itemType;
        }

        public Type ItemType { get; private set; }
    }

    enum GeneralDBType {
        Auto = 0,
        Boolean = 1,
        DateTime = 2,
        Decimal = 3,
        Guid = 4,
        Image = 5,
        Int32 = 6,
        String = 7,
        Text = 8,
        Date = 9,
        Int64 = 10,
        BigObject = 11,
        Time = 12,
    }
}
