using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration.Core.Dcxml;

namespace System.Configuration.Core.Tests {
    /// <summary>
    /// DiffTest 的摘要说明
    /// </summary>
    [TestClass]
    public class DiffTest {
        public TestContext TestContext { get; set; }
        public TestDirectory RootDirectory { get; set; }

        [TestInitialize]
        public void Init() {
            this.RootDirectory = TestDirectory.Create(this, "DiffTest.xml");
        }

        [TestMethod]
        public void TestDiffValue() {

            //rep1和rep2都第一次使用，检测加载的差量是否正确。
            DcxmlRepository rep1 = new DcxmlRepository(this.RootDirectory.Path + @"\rep1");
            DcxmlRepository rep2 = new DcxmlRepository(this.RootDirectory.Path + @"\rep2", rep1);

            AssertRep2(rep2);
            AssertRep1(rep1);

            //测试第二种情况，即rep1已经有人使用过，rep2再使用（内部实现中rep1将使用缓存的零件）。
            rep1 = new DcxmlRepository(this.RootDirectory.Path + @"\rep1");
            AssertRep1(rep1);
            rep2 = new DcxmlRepository(this.RootDirectory.Path + @"\rep2", rep1);
            AssertRep2(rep2);
        }

        private static void AssertRep2(DcxmlRepository rep2) {
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep2);
            Window win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f1", "testPackage"));
            Assert.AreEqual("demo1", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f2", "testPackage"));
            Assert.AreEqual("demo2", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f3", "testPackage"));
            Assert.AreEqual("demo3 new", win.Text);
        }

        private static void AssertRep1(DcxmlRepository rep1) {
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep1);
            Window win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f1", "testPackage"));
            Assert.AreEqual("demo1", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f3", "testPackage"));
            Assert.AreEqual("demo3", win.Text);
        }
    }
}
