using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration.Core.Dcxml;
using System.Configuration.Core.Dc;
using System.IO;

namespace System.Configuration.Core.Tests {
    /// <summary>
    /// DcTest 的摘要说明
    /// </summary>
    [TestClass]
    public class DcTest {
        [TestInitialize]
        public void Init() {
            PlatformUtilities.Current = new PlatformTestUtilities(TestDirectory.Create("ConverterTest.xml"));
        }

        [TestMethod]
        public void TestWriteAndRead() {
            DcxmlRepository rep = new DcxmlRepository("root");
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep);

            string file = Path.Combine(Environment.CurrentDirectory, "ConverterTest.dc");
            var source = rep.GetPackage("testPackage");

            BinaryPackageWriter.ConvertToDc(file,source,wp.Binder);
        }
    }
}
