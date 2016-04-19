using System.Configuration.Core.Dcxml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {
    [TestClass]
    public class ConverterTest {
        public TestContext TestContext { get; set; }
        public TestDirectory RootDirectory { get; set; }

        [TestInitialize]
        public void Init() {
            this.RootDirectory = TestDirectory.Create(this, "ConverterTest.xml");
        }

        [TestMethod]
        public void TestConvert() {
            Repository rep = new Repository(RootDirectory.Path);
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
