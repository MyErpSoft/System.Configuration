using System;
using System.Collections.Generic;
using System.Configuration.Core.Metadata;

namespace System.Configuration.Core {
    
    /// <summary>
    /// 提供一些额外的辅助函数。
    /// </summary>
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
        
        /// <summary>
        /// 返回缺省的IPackageProvider，他使用文件系统，并对dc和dcxml文件进行支持。
        /// </summary>
        /// <param name="path">包所在的路径。</param>
        /// <param name="isSupportDcxml">是否支持dcxml文件格式</param>
        /// <returns>一个IPackageProvider实例。</returns>
        public static IPackageProvider CreateDefaultPackageProvider(string path, bool isSupportDcxml = true) {
            if (isSupportDcxml) {
                return new CombinedPackageProvider(new Dc.DcPackageProvider(path), new Dcxml.DcxmlPackageProvider(path));
            }
            else {
                return new Dc.DcPackageProvider(path);
            }
        }

        private readonly static CombinedConfigurationObjectBinder _defaultBinder =
            new CombinedConfigurationObjectBinder(new IConfigurationObjectBinder[] {
                new Metadata.Clr.ClrConfigurationObjectBinder(),
                new Metadata.Clr.ClrInterfaceConfigurationObjectBinder() });

        public static IConfigurationObjectBinder DefaultBinder {
            get { return _defaultBinder; }
        }

        /// <summary>
        /// 提供类型的唯一名称，然后查询对象的类型。
        /// </summary>
        /// <param name="name">要检索的唯一名称。</param>
        /// <returns>对应的唯一名称。</returns>
        public static IType BindToType(this IConfigurationObjectBinder binder, ObjectTypeQualifiedName name) {
            if (name == null) {
                Utilities.ThrowArgumentNull(nameof(name));
            }

            IType result;
            if (!binder.TryBindToType(name,out result)) {
                Utilities.ThrowApplicationException("未能识别的配置对象类型，检查providerName、namespace、packageName、Name是否正确");
            }

            return result;
        }
    }
}
