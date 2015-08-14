using System.Configuration.Core;
using System.Configuration.Core.Dcxml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {

    [TestClass]
    public class InheritTest {

        [TestInitialize]
        public void Init() {
            PlatformUtilities.Current = new PlatformTestUtilities(TestDirectory.Create("InheritTest.xml"));
        }

        [TestMethod]
        public void TestGetValue() {
            DcxmlRepository rep = new DcxmlRepository("root");
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep);
            Window win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "baseWindow", "testPackage"));
            Assert.AreEqual<string>("Hello, World!", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "newWindow", "testPackage"));
            Assert.AreEqual<string>("Hello, It's new World!", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "newWindow2", "testPackage"));
            Assert.AreEqual<string>("Hello, World!", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "newWindow3", "testPackage"));
            Assert.AreEqual<string>("Hello, It's new World2!", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "newWindow4", "testPackage"));
            Assert.AreEqual<string>("Hello, It's new World!", win.Text);
        }
    }
}
