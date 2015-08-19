using System.Configuration.Core;
using System.Linq;
using System.Configuration.Core.Dcxml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {

    [TestClass]
    public class CollectionTest {
        DcxmlRepository rep1;
        DcxmlRepository rep2;

        [TestInitialize]
        public void Init() {
            PlatformUtilities.Current = new PlatformTestUtilities(TestDirectory.Create("CollectionTest.xml"));
            rep1 = new DcxmlRepository(@"root\rep1");
            rep2 = new DcxmlRepository(@"root\rep2",rep1);
        }

        [TestMethod]
        public void TestCollection() {
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep1);
            Button button = (Button)wp.GetObject(new QualifiedName("company.erp.demo", "A1", "testPackage"));
            Assert.AreEqual("A1", button.Text);
            Assert.AreEqual(2, button.Controls.Count);

            Assert.AreEqual(1, GetCountByText("A1_1", button.Controls));
            Assert.AreEqual(1, GetCountByText("A1_2", button.Controls));

            //派生
            button = (Button)wp.GetObject(new QualifiedName("company.erp.demo", "B1", "testPackage"));
            Assert.AreEqual("B1", button.Text);
            Assert.AreEqual(2, button.Controls.Count);

            Assert.AreEqual(1, GetCountByText("A1_1", button.Controls));
            Assert.AreEqual(0, GetCountByText("A1_2", button.Controls)); //被派生类删除了
            Assert.AreEqual(1, GetCountByText("B1_1", button.Controls)); //新增

            //测试代码中触发HashSet的使用。（DifferenceList.GetList）
            button = (Button)wp.GetObject(new QualifiedName("company.erp.demo", "D1", "testPackage"));
            Assert.AreEqual("D1", button.Text);
            Assert.AreEqual(1, button.Controls.Count);
            Assert.AreEqual(1, GetCountByText("D1_1", button.Controls));
        }

        [TestMethod]
        public void TestDifferenceCollection() {
            ConfigurationWorkspace wp = new ConfigurationWorkspace(rep2);
            Button button = (Button)wp.GetObject(new QualifiedName("company.erp.demo", "A1", "testPackage"));
            Assert.AreEqual("A1", button.Text);
            Assert.AreEqual(2, button.Controls.Count);

            Assert.AreEqual(0, GetCountByText("A1_1", button.Controls));
            Assert.AreEqual(1, GetCountByText("A1_2", button.Controls));
            Assert.AreEqual(1, GetCountByText("A1_3", button.Controls));

            //派生
            button = (Button)wp.GetObject(new QualifiedName("company.erp.demo", "B1", "testPackage"));
            Assert.AreEqual("B1", button.Text);
            Assert.AreEqual(2, button.Controls.Count);

            Assert.AreEqual(0, GetCountByText("A1_1", button.Controls));
            Assert.AreEqual(0, GetCountByText("A1_2", button.Controls));
            Assert.AreEqual(1, GetCountByText("B1_1", button.Controls)); //新增
            Assert.AreEqual(1, GetCountByText("A1_3", button.Controls)); //新增

            //差量文件再次修改了记录
            button = (Button)wp.GetObject(new QualifiedName("company.erp.demo", "D1", "testPackage"));
            Assert.AreEqual("D1", button.Text);
            Assert.AreEqual(2, button.Controls.Count);
            Assert.AreEqual(0, GetCountByText("D1_1", button.Controls));
            Assert.AreEqual(1, GetCountByText("D1_2", button.Controls));
            Assert.AreEqual(1, GetCountByText("A1_1", button.Controls));
        }

        private int GetCountByText(string text,ControlCollection controls) {
            return controls.Where((p) => p.Text == text).Count();
        }
    }
}
