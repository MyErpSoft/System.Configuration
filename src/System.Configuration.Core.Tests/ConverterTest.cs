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
            Button btnOK = (Button)wp.GetObject(new QualifiedName("company.erp.demo", "btnOK", "testPackage"));
            Assert.AreEqual("OK", btnOK.Text);

            //bool
            Assert.AreEqual(true, btnOK.Enabled);

            //Bottom
            Assert.AreEqual(AnchorStyles.Right | AnchorStyles.Bottom, btnOK.Anchor);

            //Reference
            var refValue = btnOK.BackgroundImage;
            Image imgSky = (Image)wp.GetObject(new QualifiedName("company.erp.demo", "imgSky", "testPackage"));
            Assert.AreEqual(imgSky, refValue);
        }
    }
}
