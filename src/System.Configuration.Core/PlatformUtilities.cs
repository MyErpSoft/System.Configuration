using System.IO;

namespace System.Configuration.Core {

    internal abstract class PlatformUtilities {
        private static PlatformUtilities _current
#if Windows
            = new PlatformWinUtilities();
#endif
#if Unix
            = new PlatformUnixUtilities();
#endif

        public static PlatformUtilities Current {
            get { return _current; }
            internal set { _current = value; }
        }

        public abstract string CombinePath(string path1, string path2);

        public abstract string[] GetFiles(string path, string searchPattern, bool topDirectoryOnly);

        public virtual Stream Open(string path, FileMode mode) {
            return Open(path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.None);
        }

        public abstract Stream Open(string path, FileMode mode, FileAccess access, FileShare share);

        public abstract string GetFileNameWithoutExtension(string path);
    }
}
