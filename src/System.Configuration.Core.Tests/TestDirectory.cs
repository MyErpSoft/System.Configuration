using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {
    /*
    测试程序如何工作的？

    以BasicTest为例，在测试类初始化时，将在测试目录：..\..\TestResults\Deploy_tansm 2015-09-06 17_17_48\Out
    下建立目录：System.Configuration.Core.Tests.BasicTest，包含命名空间和类名。

    然后根据传入的配置文件初始化信息，例如：BasicTest.xml。根据配置文件的描述，在之前的目录下再建立对应的目录：testPackage
    以及将Files目录下的文件复制到此目录。
    */

    /// <summary>
    /// 目录项。
    /// </summary>
    public abstract class TestFileItem {
        [XmlAttribute("name")]
        public string Name { get; set; }
    }

    [XmlType("directory")]
    public class TestDirectory : TestFileItem {
        private string _path;

        public TestDirectory() {

        }

        public static TestDirectory Create(object instance, string resurceName) {
            TestContext testContext = GetTestContext(instance);

            //测试输出目录。
            var outputDirectory = testContext.TestDeploymentDir;
            //命名空间+名称的目录
            var rootDirectory = instance.GetType().FullName;
            
            TestDirectory root;
            using (var stream = TestFile.GetResourceStream(resurceName)) {
                XmlSerializer ser = new XmlSerializer(typeof(TestDirectory));
                root = (TestDirectory)ser.Deserialize(stream);
            }

            root.Name = rootDirectory;
            root.Build(outputDirectory);

            return root;
        }

        private void Build(string parent) {
            this._path = IO.Path.Combine(parent, this.Name);
            Directory.CreateDirectory(Path);

            if (this.Items != null) {
                foreach (var item in Items) {
                    TestDirectory dir = item as TestDirectory;
                    if (dir != null) {
                        dir.Build(Path);
                    }
                    else {
                        TestFile file = item as TestFile;
                        if (file != null) {
                            file.Build(Path);
                        }
                        else {
                            Utilities.ThrowNotSupported("?");
                        }
                    }
                }
            }
        }

        private static TestContext GetTestContext(object instance) {
            if (instance == null) {
                Utilities.ThrowArgumentNull(nameof(instance));
            }
            var testContextProperty = instance.GetType().GetProperty("TestContext");
            if (testContextProperty == null) {
                Utilities.ThrowApplicationException("测试程序没有包含TestContext属性");
            }
            var testContext = (TestContext)testContextProperty.GetValue(instance);
            return testContext;
        }

        [XmlElement("directory", typeof(TestDirectory))]
        [XmlElement("file",typeof(TestFile))]
        public TestFileItem[] Items;

        public string Path {
            get {
                return _path;
            }
        }
        
    }

    
    [XmlType("file")]
    public class TestFile : TestFileItem {
        
        private string _path;

        internal static Stream GetResourceStream(string resurceName) {
            var sourceFile = Path.Combine(Environment.CurrentDirectory, "Files", resurceName);
            return File.OpenRead(sourceFile);
        }

        internal void Build(string parent) {
            var targetFile = Path.Combine(parent, this.Name);
            if (!File.Exists(targetFile)) {
                var sourceFile = Path.Combine(Environment.CurrentDirectory, "Files", this.Name);
                File.Copy(sourceFile, targetFile);
            }            
            _path = targetFile;
        }

        
    }
}
