using System.Configuration.Core.Dcxml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {
    [TestClass]
    public class ConverterTest {
        [TestInitialize]
        public void Init() {
            PlatformUtilities.Current = new PlatformTestUtilities(TestDirectory.Create("ConverterTest.xml"));
        }

        [TestMethod]
        public void TestConvert() {
            DcxmlRepository rep = new DcxmlRepository("root");
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep);

            //string
            Control btnOK = (Control)wp.GetObject(new QualifiedName("company.erp.demo", "btnOK", "testPackage"));
            Assert.AreEqual<string>("OK", btnOK.Text);

            //bool
            Assert.AreEqual<bool>(true, btnOK.Enabled);
        }
    }
}
