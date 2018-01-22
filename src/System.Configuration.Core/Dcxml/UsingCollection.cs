using System.Collections.Generic;
using System.Globalization;

namespace System.Configuration.Core.Dcxml {

    /// <summary>
    /// 解析处理using字符串
    /// </summary>
    internal sealed class UsingCollection {
        //从前缀检索对应的完整信息。
        private Dictionary<string, QualifiedName> _leftToRight;
       // private Dictionary<string, string> _rightToLeft;
        private DcxmlFile _file;
        
        public UsingCollection(string init,DcxmlFile file) {
            _leftToRight = new Dictionary<string, QualifiedName>();
            this._file = file;

            InitUsingString(init);
        }

        private void InitUsingString(string init) {
            if (!string.IsNullOrEmpty(init)) {
                //ex: a:company.erp b:company.erp.crm,comany.erp.crm.demo.ui
                var list = init.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in list) {
                    string prefix, packageName, objNamespace;
                    AnalyzeObjectPtr(item, out prefix, out objNamespace, out packageName);
                    if (string.IsNullOrEmpty(prefix)) {
                        //必须包含 冒号，不然在using中描述就没有意义了。
                        Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                            "在dcxml：{0}中发现不正确的using使用语法，缺少冒号。正确的用法是<prefix:><namespace>[,packageName]", _file.FileName));
                    }

                    if (!Utilities.VerifyNameWithNamespace(objNamespace)) {
                        Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                           "在dcxml：{0}中发现using中出现不准确的命名空间{1}，必须是字母、数字和下划线。", _file.FileName, item));
                    }

                    if (packageName == null) {
                        //未指定包时，使用当前包。
                        packageName = _file.Package.Name;
                    }
                    else {
                        if (!Utilities.VerifyNameWithNamespace(packageName)) {
                            Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                               "在dcxml：{0}中发现using中出现不准确的包{1}描述，必须是字母、数字和下划线。", _file.FileName, item));
                        }
                    }

                    objNamespace = string.Intern(objNamespace);
                    packageName = string.Intern(packageName);
                    var qName = new QualifiedName(objNamespace, null, packageName);
                    if (_leftToRight.ContainsKey(prefix)) {
                        Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                              "在dcxml：{0}中发现using中出现重复的前缀命名。", _file.FileName, item));
                    }
                    _leftToRight.Add(prefix, qName);
                }
            }
        }

        private void AnalyzeObjectPtr(string item,out string prefix, out string fullName, out string packageName) {
            //ex: b:company.erp.crm,comany.erp.crm.demo.ui
            //ex: a:company.erp

            //前缀
            var colonIndex = item.IndexOf(':');
            if (colonIndex < 0) {
                prefix = null;
            }
            else {
                prefix = item.Substring(0, colonIndex).Trim();
            }

            colonIndex++;

            //搜索后面是否存在包描述
            var index = item.IndexOf(',', colonIndex);

            if (index < 0) {
                //没有包描述， ex:company.erp
                fullName = item.Substring(colonIndex).Trim();
                packageName = null;
            }
            else {
                //存在包描述,ex: company.erp.crm,comany.erp.crm.demo.ui
                fullName = item.Substring(colonIndex, index - colonIndex).Trim();
                packageName = item.Substring(index + 1).Trim();
            }
        }

        internal QualifiedName GetQualifiedName(string value) {
            if (string.IsNullOrEmpty(value)) {
                return QualifiedName.Empty;
            }

            //ex: a:BasicForm
            //ex: BasicForm
            //ex: company.erp.BasicForm
            //ex: company.erp.BasicForm,comany.erp.crm.demo.ui
            string prefix, fullName, packageName, objNamesapce,name;
            bool checkNamespace = true, checkName = true, checkPackageName = true;
            AnalyzeObjectPtr(value,out  prefix,out fullName,out packageName);

            if (prefix != null) {
                QualifiedName right;
                if (!_leftToRight.TryGetValue(prefix, out right)) {
                    Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                               "不正确的对象指针描述，无法在当前文件 {0} 中找到其描述的前缀{1}。", _file.FileName, value));
                }

                objNamesapce = right.FullName.Namespace;
                packageName = right.PackageName;
                checkNamespace = false;
                checkPackageName = false;
                name = fullName;
            }
            else {

                if (string.IsNullOrEmpty(packageName)) {
                    packageName = this._file.Package.Name;
                    checkPackageName = false;

                    AnalyzeFullName(fullName, out objNamesapce, out name);
                    if (objNamesapce == null) {
                        objNamesapce = _file.Namespace;
                        checkNamespace = false;
                    }
                }
                else {
                    AnalyzeFullName(fullName, out objNamesapce, out name);
                }
            }

            if (checkNamespace && !Utilities.VerifyNameWithNamespace(objNamesapce)) {
                Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                           "不正确的对象指针描述，其命名空间包含不合法的字符或空。", _file.FileName, value));
            }

            if (checkName && !Utilities.VerifyName(name)) {
                Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                           "不正确的对象指针描述，其名称包含不准确的字符或空。", _file.FileName, value));
            }

            if (checkPackageName && !Utilities.VerifyNameWithNamespace(packageName)) {
                Utilities.ThrowApplicationException(string.Format(CultureInfo.CurrentCulture,
                           "不正确的对象指针描述，包描述包含不准确的字符或空。", _file.FileName, value));
            }

            return new QualifiedName(objNamesapce, name, packageName);
        }

        private void AnalyzeFullName(string fullName, out string objNamesapce, out string name) {
            if (string.IsNullOrEmpty(fullName)) {
                objNamesapce = null;
                name = null;
                return;
            }
            var lastInex = fullName.LastIndexOf('.');

            if (lastInex < 0) {
                objNamesapce = null;
                name = fullName;
            }
            else {
                objNamesapce = fullName.Substring(0, lastInex);
                name = fullName.Substring(lastInex + 1);
            }
        }
    }
}
