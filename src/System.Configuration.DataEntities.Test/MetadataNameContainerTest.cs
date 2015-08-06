using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.DataEntities.Test {
    [TestClass]
    public class MetadataNameContainerTest {
        [TestMethod]
        public void TestGetName() {
            MetadataNameContainer target = new MetadataNameContainer();

            var n1 = "Test";
            var n2 = "Test2";
            char[] narray = {'T','e','s','t'};

            Assert.AreEqual<MetadataName>(target.GetName(n1), target.GetName(new string(narray)));
            Assert.AreNotEqual<MetadataName>(target.GetName(n2), target.GetName(n1));

            System.Collections.Generic.HashSet<MetadataName> names = new Collections.Generic.HashSet<MetadataName>();
            names.Add(target.GetName(n1));
            Assert.IsTrue(names.Contains(target.GetName(n1)));
            Assert.IsTrue(names.Contains(target.GetName(new string(narray))));
            Assert.IsFalse(names.Contains(target.GetName(n2)));

            Assert.AreEqual<string>(n1, target.GetName(n1).ToString());

            var n3 = "System.Configuration.DataEntities.Test";
            var n4 = "System.Configuration.DataEntities.Test,version=2.1";
            Assert.AreEqual<string>(n3,target.GetName(n3).ToString());
            Assert.AreEqual<string>(n4, target.GetName(n4).ToString());
        }

        [TestMethod]
        public void TestTryGetName() {
            MetadataNameContainer target = new MetadataNameContainer();

            var n1 = "Test";
            MetadataName name;
            Assert.IsFalse(target.TryGetName(n1, out name));
            var name2 = target.GetName(n1);
            Assert.IsTrue(target.TryGetName(n1, out name));
            Assert.ReferenceEquals(name, name2);
        }

        [TestMethod]
        public void TestGetNameIgnoreCase() {
            MetadataNameContainer target = new MetadataNameContainer(StringComparer.OrdinalIgnoreCase);

            var n1 = "Test";
            var n2 = "Test2";
            var n3 = "test";
            char[] narray = { 'T', 'E', 'S', 'T' };

            Assert.AreEqual<MetadataName>(target.GetName(n1), target.GetName(new string(narray)));
            Assert.AreEqual<MetadataName>(target.GetName(n1), target.GetName(n3));
            Assert.AreNotEqual<MetadataName>(target.GetName(n2), target.GetName(n1));

            System.Collections.Generic.HashSet<MetadataName> names = new Collections.Generic.HashSet<MetadataName>();
            names.Add(target.GetName(n1));
            Assert.IsTrue(names.Contains(target.GetName(n1)));
            Assert.IsTrue(names.Contains(target.GetName(new string(narray))));
            Assert.IsTrue(names.Contains(target.GetName(n3)));
            Assert.IsFalse(names.Contains(target.GetName(n2)));
        }

        [TestMethod]
        public void TestCheckName() {
            MetadataNameContainer target = new MetadataNameContainer();
            char[] chars = new char[256];
            for (int i = 0; i < chars.Length; i++) {
                chars[i] = 'f';
            }
            target.GetVerifyName(new string(chars));
            target.GetVerifyName("_test");
            target.GetVerifyName("__test");
            target.GetVerifyName("test_");
            target.GetVerifyName("test_test");
            target.GetVerifyName("test123");
            target.GetVerifyName("_012");
        }

        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        public void TestCheckName_0() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetVerifyName(null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void TestCheckName_1() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetName(null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCheckName_2() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetVerifyName("3test");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCheckName_3() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetVerifyName(string.Empty);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCheckName_4() {
            MetadataNameContainer target = new MetadataNameContainer();
            char[] chars = new char[256 + 1];
            for (int i = 0; i < chars.Length; i++) {
                chars[i] = 'f';
            }

            target.GetVerifyName(new string(chars));
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCheckName_5() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetVerifyName("汉字");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCheckName_6() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetVerifyName("t*()");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCheckName_7() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetVerifyName("*()");
        }
    }
}
