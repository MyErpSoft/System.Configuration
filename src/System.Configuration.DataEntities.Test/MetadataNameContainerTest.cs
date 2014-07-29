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
            target.GetName(new string(chars));
            target.GetName("_test");
            target.GetName("__test");
            target.GetName("test_");
            target.GetName("test_test");
            target.GetName("test123");
            target.GetName("_012");
        }

        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        public void TestCheckName_1() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetName(null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCheckName_2() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetName("3test");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCheckName_3() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetName(string.Empty);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCheckName_4() {
            MetadataNameContainer target = new MetadataNameContainer();
            char[] chars = new char[256 + 1];
            for (int i = 0; i < chars.Length; i++) {
                chars[i] = 'f';
            }

            target.GetName(new string(chars));
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCheckName_5() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetName("汉字");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCheckName_6() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetName("t*()");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCheckName_7() {
            MetadataNameContainer target = new MetadataNameContainer();

            target.GetName("*()");
        }
    }
}
