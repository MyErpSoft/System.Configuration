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
            Repository rep1 = new Repository(this.RootDirectory.Path + @"\rep1");
            Repository rep2 = new Repository(this.RootDirectory.Path + @"\rep2", rep1);

            AssertRep2(rep2);
            AssertRep1(rep1);

            //测试第二种情况，即rep1已经有人使用过，rep2再使用（内部实现中rep1将使用缓存的零件）。
            rep1 = new Repository(this.RootDirectory.Path + @"\rep1");
            AssertRep1(rep1);
            rep2 = new Repository(this.RootDirectory.Path + @"\rep2", rep1);
            AssertRep2(rep2);

            //当在rep1检索testPackage时，返回的应该是本地的Package对象，而在rep2中返回的是合并后的
            Assert.AreNotEqual(rep1.GetPackage("testPackage"), rep2.GetPackage("testPackage"));

            //增加第三层的差量测试。
            Repository rep3 = new Repository(this.RootDirectory.Path + @"\rep3", rep2);
            AssertRep3(rep3);
            //都是合并的包，但是对应的合并对象不同。
            Assert.AreNotEqual(rep2.GetPackage("testPackage"), rep3.GetPackage("testPackage"));
        }

        private static void AssertRep2(Repository rep2) {
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep2);
            Window win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f1", "testPackage"));
            Assert.AreEqual("demo1", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f2", "testPackage"));
            Assert.AreEqual("demo2", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f3", "testPackage"));
            Assert.AreEqual("demo3 new", win.Text);
        }

        private static void AssertRep1(Repository rep1) {
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep1);
            Window win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f1", "testPackage"));
            Assert.AreEqual("demo1", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f3", "testPackage"));
            Assert.AreEqual("demo3", win.Text);
        }

        private static void AssertRep3(Repository rep3) {
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep3);
            Window win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f1", "testPackage"));
            Assert.AreEqual("demo1", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f2", "testPackage"));
            Assert.AreEqual("demo2", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f3", "testPackage"));
            Assert.AreEqual("demo4 new", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f4", "testPackage"));
            Assert.AreEqual("demo4", win.Text);

            //利用x:base 测试 对象指针的能力,具体测试目的参考dcxml的描述
            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f5", "testPackage"));
            Assert.AreEqual("demo4", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f6", "testPackage"));
            Assert.AreEqual("demo1", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f7", "testPackage"));
            Assert.AreEqual("demo2.f1", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f8", "testPackage"));
            Assert.AreEqual("demo2.f1 at my.testPackage2", win.Text);

            //使用前缀
            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f9", "testPackage"));
            Assert.AreEqual("demo2.f1", win.Text);

            win = (Window)wp.GetObject(new QualifiedName("company.erp.demo", "f10", "testPackage"));
            Assert.AreEqual("demo2.f1 at my.testPackage2", win.Text);
        }
    }
}
