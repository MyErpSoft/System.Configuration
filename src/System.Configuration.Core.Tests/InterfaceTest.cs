using System.Configuration.Core;
using System.Configuration.Core.Dcxml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {

    [TestClass]
    public class InterfaceTest {
        public TestContext TestContext { get; set; }
        public TestDirectory RootDirectory { get; set; }

        [TestInitialize]
        public void Init() {
            this.RootDirectory = TestDirectory.Create(this, "InterfaceTest.xml");
        }

        [TestMethod]
        public void TestInterface() {
            Repository rep = new Repository(this.RootDirectory.Path);
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep);
            //获取接口形式的对象
            IWindow win = (IWindow)wp.GetObject(new QualifiedName("company.erp.demo", "A1", "interfaceTestPackage"));
            Assert.AreEqual<string>("A1", win.Text);

            IButton btnA1_1 = (IButton)wp.GetObject(new QualifiedName("company.erp.demo", "A1_1", "interfaceTestPackage"));
            Assert.AreEqual<string>("A1_1", btnA1_1.Text);

            IButton btnA1_2 = (IButton)wp.GetObject(new QualifiedName("company.erp.demo", "A1_2", "interfaceTestPackage"));
            Assert.AreEqual<string>("A1_2", btnA1_2.Text);

            //int count = 0;
            //foreach (var control in win.Controls) {
            //    if (control == btnA1_1) {
            //        count++;
            //    }else if(control == btnA1_2) {
            //        count++;
            //    }
            //    else {
            //        Assert.Fail();
            //    }
            //}
            //Assert.AreEqual<int>(2, count);

            IWindow winB1 = (IWindow)wp.GetObject(new QualifiedName("company.erp.demo", "B1", "interfaceTestPackage"));
            Assert.AreEqual<string>("B1", winB1.Text);

            Button btnD1 = (Button)wp.GetObject(new QualifiedName("company.erp.demo", "D1", "interfaceTestPackage"));
            Assert.AreEqual<string>("D1", btnD1.Text);

            IWindow winD2 = (IWindow)wp.GetObject(new QualifiedName("company.erp.demo", "D2", "interfaceTestPackage"));
            Assert.AreEqual<string>("D2", winD2.Text);


        }
    }
}
