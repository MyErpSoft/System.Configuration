#if Windows
using System.IO;

namespace System.Configuration.Core {

    internal sealed class PlatformWinUtilities : PlatformUtilities {
        public override string CombinePath(string path1, string path2) {
            return Path.Combine(path1, path2);
        }

        public override string[] GetFiles(string path,string searchPattern, bool topDirectoryOnly) {
            return Directory.GetFiles(path, searchPattern, topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
        }
    }
}
#endif