using System.IO;

namespace System.Configuration.Core.Tests {

    internal class PlatformTestUtilities : PlatformUtilities {

        public PlatformTestUtilities(TestDirectory root) {
            this.Root = new TestDirectory() { Items = new TestFileItem[] { root } };
        }

        public TestDirectory Root { get; private set; }

        public override string CombinePath(string path1, string path2) {
            return Path.Combine(path1, path2);
        }

        public override string[] GetFiles(string path, string searchPattern, bool topDirectoryOnly) {
            var dir = this.Root.GetDirectory(path);
            return dir.GetFiles(searchPattern, topDirectoryOnly);
        }

        public override Stream Open(string path, FileMode mode, FileAccess access, FileShare share) {
            //测试时检索到的文件名是没有path路径的，只有文件名
            //var file = this.Root.GetFile(path);
            //return file.Open();
            return TestFile.GetResourceStream(path);
        }
    }
}
