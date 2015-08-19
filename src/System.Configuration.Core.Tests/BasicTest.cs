using System.Configuration.Core;
using System.Configuration.Core.Dcxml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {

    [TestClass]
    public class BasicTest {

        [TestInitialize]
        public void Init() {
            PlatformUtilities.Current = new PlatformTestUtilities(TestDirectory.Create("BasicTest.xml"));
        }

        [TestMethod]
        public void TestHelloWorld() {
            DcxmlRepository rep = new DcxmlRepository("root");
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep);
            Window win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "frmMain", "testPackage"));
            Assert.AreEqual<string>("Hello, World!", win.Text);
        }
    }
}
