using System;
using System.Collections.Generic;

namespace System.Configuration.Core {
    
    internal static class Utilities {

        /// <summary>
        /// Verifies that the string is composed of letters or numbers (allow underscore). Separation of and supports the use of a namespace
        /// </summary>
        internal static bool VerifyNameWithNamespace(string str) {
            char item;
            int wordStartIndex = 0;  //Start position of a Word
            int endIndex = str.Length;
            int wordSize = 0;

            if (string.IsNullOrEmpty(str)) {
                return false;
            }

            for (int i = 0; i < endIndex; i++) {
                item = str[i];
                if (!((item >= 'a' && item <= 'z') ||
                    (item >= 'A' && item <= 'Z') ||
                    (item == '_'))) {
                    if (item >= '0' && item <= '9') {
                        //Cannot start with a number
                        if (i == wordStartIndex) {
                            return false;
                        }
                    }
                    else if (item == '.') {
                        //Using split words should not be empty, the last may not be
                        //A Word cannot be longer than 256 characters.
                        if ((wordSize == 0) || (i == endIndex) || (wordSize > 256)) {
                            return false;
                        }
                        wordStartIndex = i + 1;
                        wordSize = 0;
                        continue;
                    }
                    else {
                        return false;
                    }
                }

                wordSize++;
            }

            return true;
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

        internal static void ThrowArgumentNull(string paramName) {
            throw new ArgumentNullException(paramName);
        }

        internal static void ThrowNotSupported(string message) {
            throw new NotSupportedException(message);
        }

        internal static void ThrowArgumentException(string message, string paramName) {
            throw new ArgumentException(message, paramName);
        }

        internal static void ThrowApplicationException(string message, Exception innerException) {
            throw new ApplicationException(message, innerException);
        }

        internal static void ThrowApplicationException(string message) {
            throw new ApplicationException(message);
        }

        internal static void ThrowKeyNotFoundException(string message) {
            throw new KeyNotFoundException(message);
        }
    }
}
