namespace System.Configuration.Core {

    internal abstract class PlatformUtilities {
        private static PlatformUtilities _current = new PlatformWinUtilities();

        public static PlatformUtilities Current {
            get { return _current; }
        }

        public abstract string CombinePath(string path1, string path2);

        public abstract string[] GetFiles(string path, string searchPattern, bool topDirectoryOnly);
    }
}
