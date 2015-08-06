using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;

namespace System.Configuration.DataEntities {
    
    /// <summary>
    /// Storage container for the name and fullname.
    /// </summary>
    internal sealed class MetadataNameContainer {
        private readonly ConcurrentDictionary<string, MetadataName> _names;

        public MetadataNameContainer()
            : this(EqualityComparer<string>.Default) {
        }

        public MetadataNameContainer(IEqualityComparer<string> comparer) {
            this._names = new ConcurrentDictionary<string, MetadataName>(comparer);
        }

        public MetadataName GetName(string name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            return this._names.GetOrAdd(name, new MetadataName(name));
        }

        public MetadataName GetVerifyName(string name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            MetadataName result;
            if (this._names.TryGetValue(name,out result)) {
                return result;
            }

            if (!VerifyName(name)) {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, "Name {0} is not correct, can only be a combination of letters, numbers, and underscores.", name), "name");
            }
            return this._names.GetOrAdd(name, new MetadataName(name));
        }

        /// <summary>
        /// Trying to get name corresponding to the MetadataName, but does not automatically add the container.
        /// </summary>
        /// <param name="name">The string to test</param>
        /// <param name="result">If name is found in the container will return it, otherwise it returns null</param>
        /// <returns>If name is found in the container will return true, otherwise it returns false</returns>
        public bool TryGetName(string name, out MetadataName result) {
            if (name == null) {
                result = null;
                return false;
            }

            return this._names.TryGetValue(name, out result);
        }

        /// <summary>
        /// Verifies that the string is composed of letters or numbers (allow underscore)
        /// </summary>
        internal static bool VerifyName(string str) {
            char item;
            int endIndex = str.Length;

            if (string.IsNullOrEmpty(str)) {
                return false;
            }

            if (endIndex > 256) {
                return false;
            }

            for (int i = 0; i < endIndex; i++) {
                item = str[i];
                if (!((item >= 'a' && item <= 'z') ||
                    (item >= 'A' && item <= 'Z') ||
                    (item == '_'))) {
                    if (item >= '0' && item <= '9') {
                        //Cannot start with a number
                        if (i == 0) {
                            return false;
                        }

                        continue;
                    }
                    return false;
                }
            }

            return true;
        }
    }
}
