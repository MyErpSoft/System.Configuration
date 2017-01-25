using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {
    /// <summary>
    /// DcTest 的摘要说明
    /// </summary>
    [TestClass]
    public class BigFileTest {
        public TestContext TestContext { get; set; }
        public TestDirectory RootDirectory { get; set; }

        [TestInitialize]
        public void Init() {
            this.RootDirectory = TestDirectory.Create(this, "BigFileTest.xml");
        }

        [TestMethod]
        public void TestRead() {
            //QueryProjectContainerElement container = new QueryProjectContainerElement(new Tests.DependencyObject(QueryProjectContainerElement.DefaultType));
            XDocument doc = XDocument.Load(RootDirectory.Path + @"\rep1\QueryProjectContainer.dcxml");
            var queryProjectContainer = Read(doc.Root);
        }

        private static DependencyObject Read(XElement element) {
            var dt = BindTo(element.Name.LocalName);
            var obj = new DependencyObject(dt);

            foreach (var propertyElement in element.Elements()) {
                var property = dt.Properties[propertyElement.Name.LocalName];
                SimpleDependencyProperty sp = property as SimpleDependencyProperty;
                if (sp != null) {
                    var value = Convert.ChangeType(propertyElement.Value, sp.PropertyType);
                    sp.SetValue(obj, value);
                }
                else {
                    ComplexDependencyProperty cxp = property as ComplexDependencyProperty;
                    if (cxp != null) {
                        if (propertyElement.HasElements) {
                            var value = Read(propertyElement.Elements().First());
                            cxp.SetValue(obj, value);
                        }
                    }                   
                    else {
                        CollectionDependencyProperty colp = (CollectionDependencyProperty)property;
                        var list = (IList<DependencyObject>)colp.GetValue(obj);
                        foreach (var item in propertyElement.Elements()) {
                            list.Add(Read(item));
                        }
                    }
                }
            }

            return obj;
        }

        private static Dictionary<string, DependencyObjectType> _types;
        private static DependencyObjectType BindTo(string name) {
            if (_types == null) {
                _types = GetAllTypes();
            }

            string nameWithElement = name + "Element";
            DependencyObjectType dt;
            if (!_types.TryGetValue(nameWithElement,out dt)) {
                return _types[name];
            }
            return dt;
        }

        private static Dictionary<string, DependencyObjectType> GetAllTypes() {
            var clrTypes = from p in typeof(QueryProjectContainerElement).Assembly.GetTypes()
                           where p.IsDefined(typeof(DataEntityRegisterAttribute), false)
                           select GetDependencyObjectTypeFormClrType(p);
            
            var types = new Dictionary<string, DependencyObjectType>();
            foreach (var item in clrTypes) {
                types.Add(item.Name, item);
            }

            return types;
        }

        private static DependencyObjectType GetDependencyObjectTypeFormClrType(Type p) {
            var field = p.GetField("DefaultType", Reflection.BindingFlags.Static | Reflection.BindingFlags.Public);
            return (DependencyObjectType)field.GetValue(null);
        }
    }
}