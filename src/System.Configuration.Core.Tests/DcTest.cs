using System.Configuration.Core.Dc;
using System.Configuration.Core.Dcxml;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {
    /// <summary>
    /// DcTest 的摘要说明
    /// </summary>
    [TestClass]
    public class DcTest {
        public TestContext TestContext { get; set; }
        public TestDirectory RootDirectory { get; set; }

        [TestInitialize]
        public void Init() {
            this.RootDirectory = TestDirectory.Create(this, "ConverterTest.xml");
        }
        
        [TestMethod]
        public void TestWriteAndRead() {
            DcxmlRepository rep = new DcxmlRepository(RootDirectory.Path);
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep);

            string file = Path.Combine(RootDirectory.Path, "testPackage.dc");
            var source = rep.GetPackage("testPackage");

            //创建二进制dc
            BinaryPackageWriter.ConvertToDc(file,source,wp.Binder);

            //读取
            DcRepository rep2 = new DcRepository(RootDirectory.Path);
            wp = new ConfigurationWorkspace(rep2);

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
