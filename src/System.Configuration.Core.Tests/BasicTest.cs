using System.Configuration.Core;
using System.Configuration.Core.Dcxml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {

    [TestClass]
    public class BasicTest {

        public TestContext TestContext { get; set; }
        public TestDirectory RootDirectory { get; set; }

        [TestInitialize]
        public void Init() {
            this.RootDirectory = TestDirectory.Create(this, "BasicTest.xml");
        }

        [TestMethod]
        public void TestHelloWorld() {
            Repository rep = new Repository(RootDirectory.Path);
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep);
            Window win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "frmMain", "testPackage"));
            Assert.AreEqual<string>("Hello, World!", win.Text);
        }
    }
}
