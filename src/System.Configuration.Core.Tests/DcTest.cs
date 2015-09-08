using System.Configuration.Core.Dc;
using System.Configuration.Core.Dcxml;
using System.IO;
using System.Text;
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
            BinaryPackageWriter.ConvertToDc(file, source);
            AssertValues("testPackage");
        }

        [TestMethod]
        public void TestChangePackageName() {
            DcxmlRepository rep = new DcxmlRepository(RootDirectory.Path);
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep);

            //任何包的内容，都不能包含包自己的名称信息，例如二进制的包起名称来自文件名。
            string file = Path.Combine(RootDirectory.Path, "TestPackage2.dc");//这里还故意将文件名修改大小写。
            var source = rep.GetPackage("testPackage");

            //创建二进制dc
            BinaryPackageWriter.ConvertToDc(file, source);
            AssertValues("testPackage2");
        }
        
        private void AssertValues(string packageName) {
            //读取
            DcRepository rep2 = new DcRepository(RootDirectory.Path);
            var wp = new ConfigurationWorkspace(rep2);

            Button btnOK = (Button)wp.GetObject(new QualifiedName("company.erp.demo", "btnOK", packageName));
            Assert.AreEqual("OK", btnOK.Text);

            //bool
            Assert.AreEqual(true, btnOK.Enabled);

            //Bottom
            Assert.AreEqual(AnchorStyles.Right | AnchorStyles.Bottom, btnOK.Anchor);

            //Reference
            var refValue = btnOK.BackgroundImage;
            Image imgSky = (Image)wp.GetObject(new QualifiedName("company.erp.demo", "imgSky", packageName));
            Assert.AreEqual(imgSky, refValue);
        }

        [TestMethod]
        public void TestBigFile() {
            CreateBigSourceFile();

            DcxmlRepository rep = new DcxmlRepository(RootDirectory.Path);
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep);

            string file = Path.Combine(RootDirectory.Path, "BigPackage.dc");
            var source = rep.GetPackage("BigPackage");

            //创建二进制dc
            BinaryPackageWriter.ConvertToDc(file, source);

            System.Diagnostics.Stopwatch w = new Diagnostics.Stopwatch();
            w.Start();
            DcRepository rep2 = new DcRepository(RootDirectory.Path);
            var wp2 = new ConfigurationWorkspace(rep2);

            for (int i = 0; i < 10000; i++) {

                Button btnOK = (Button)wp.GetObject(new QualifiedName("company.erp.demo", "btnOK" + i.ToString(), "BigPackage"));
                Assert.AreEqual("OK", btnOK.Text);

                //bool
                Assert.AreEqual(true, btnOK.Enabled);

                //Bottom
                Assert.AreEqual(AnchorStyles.Right | AnchorStyles.Bottom, btnOK.Anchor);

                //Reference
                var refValue = btnOK.BackgroundImage;
                Image imgSky = (Image)wp.GetObject(new QualifiedName("company.erp.demo", "imgSky", "BigPackage"));
                Assert.AreEqual(imgSky, refValue);
            }
            w.Stop();
            Console.WriteLine(w.Elapsed);
        }

        private void CreateBigSourceFile() {
            var s1 = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><x:ObjectContainer xmlns = \"clr-namespace:System.Configuration.Core.Tests,System.Configuration.Core.Tests\" xmlns:x = \"http://schemas.myerpsoft.com/configuration/2015\" x:namespace=\"company.erp.demo\">";
            var s2 = "<Button x:name=\"btnOK";
            var s3 = "\"><Text>OK</Text><Enabled>true</Enabled><Anchor>Right,Bottom</Anchor><BackgroundImage x:ref=\"imgSky\"/></Button>";
            var s4 = "<Image x:name=\"imgSky\"><Path>.\\imgSky.png</Path></Image></x:ObjectContainer>";

            var path1 = Path.Combine(RootDirectory.Path, "BigPackage");
            if (!Directory.Exists(path1)) {
                Directory.CreateDirectory(path1);
            }

            var xmlFile = Path.Combine(path1, "ConverterTest.dcxml");
            using (var stream = File.OpenWrite(xmlFile)) {
                using (var writer = new StreamWriter(stream, Encoding.UTF8)) {
                    writer.Write(s1);
                    for (int i = 0; i < 10000; i++) {
                        writer.Write(s2);
                        writer.Write(i.ToString());
                        writer.Write(s3);
                    }
                    writer.Write(s4);
                }
            }
        }
    }
}
