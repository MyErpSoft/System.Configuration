using System.Configuration.Core;
using System.Configuration.Core.Dcxml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {

    [TestClass]
    public class BasicTest {

        [TestInitialize]
        public void Init() {
            PlatformUtilities.Current = new PlatformTestUtilities(TestDirectory.Create("G1.xml"));
        }

        [TestMethod]
        public void TestGetValue() {
            DcxmlRepository rep = new DcxmlRepository("root");
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep);
            wp.GetObject(new QualifiedName("company.erp.demo", "frmMain", "testPackage"));
        }
    }
}
