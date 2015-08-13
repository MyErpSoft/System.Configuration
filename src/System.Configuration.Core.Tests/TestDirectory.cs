using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Xml.Serialization;

namespace System.Configuration.Core.Tests {

    public abstract class TestFileItem {
        [XmlAttribute("name")]
        public string Name { get; set; }
    }

    [XmlType("directory")]
    public class TestDirectory : TestFileItem {

        public TestDirectory() {

        }

        public static TestDirectory Create(string resurceName) {
            using (var stream = TestFile.GetResourceStream(resurceName)) {
                XmlSerializer ser = new XmlSerializer(typeof(TestDirectory));
                return (TestDirectory)ser.Deserialize(stream);
            }
        }
        
        [XmlElement("directory", typeof(TestDirectory))]
        [XmlElement("file",typeof(TestFile))]
        public TestFileItem[] Items;

        public TestDirectory GetDirectory(string path) {
            if (string.IsNullOrEmpty(path) || path == ".") {
                return this;
            }

            var sp = path.Split('\\');
            var current = this;
            foreach (var item in sp) {
                if (current.Items == null) {
                    Utilities.ThrowArgumentNull(nameof(path));
                }

                current = (TestDirectory)current.Items.First((p) => p is TestDirectory && p.Name == item);
                if (current == null) {
                    Utilities.ThrowArgumentNull(nameof(path));
                }
            }

            return current;
        }

        public TestFile GetFile(string path) {
            var sp = path.Split('\\');
            var current = this;
            for (int i = 0; i < sp.Length - 1; i++) {
                var item = sp[i];
                if (current.Items == null) {
                    Utilities.ThrowArgumentNull(nameof(path));
                }

                current = (TestDirectory)current.Items.First((p) => p is TestDirectory && p.Name == item);
            }

            var fileName = sp[sp.Length - 1];
            return (from p in current.Items where p is TestFile && p.Name == fileName select (TestFile)p).First();
        }

        public string[] GetFiles(string searchPattern, bool topDirectoryOnly) {
            return (from p in this.Items where p is TestFile && IsOk(p.Name, searchPattern) select p.Name).ToArray();
        }

        private static bool IsOk(string fileName, string searchPattern) {
            if (searchPattern == "*.dcxml") {
                return fileName.EndsWith(".dcxml");
            }

            return false;
        }
    }

    
    [XmlType("file")]
    public class TestFile : TestFileItem {
        internal static Assembly CurrentAssembly = typeof(TestFile).Assembly;

        public Stream Open() {
            return GetResourceStream(this.Name);
        }

        internal static Stream GetResourceStream(string name) {
            return CurrentAssembly.GetManifestResourceStream("System.Configuration.Core.Tests.Files." + name);
        }
    }
}
