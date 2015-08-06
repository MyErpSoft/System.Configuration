using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.DataEntities.Test {

    [TestClass]
    public class MetadataFullNameContainerTest {
        [TestMethod]
        public void TestGetFullName() {
            MetadataFullNameContainer target = new MetadataFullNameContainer(new MetadataNameContainer());
            var n1 = string.Empty;
            string n2 = null;
            var n3 = "Test";
            var n4 = "System.Configuration.DataEntities";
            var n5 = "System.Configuration";
            var n6 = "System.Configuration.DataEntities.Test";
            var n7 = "Configuration.System";

            Assert.IsNotNull(target.GetFullName(n1));
            Assert.IsNotNull(target.GetFullName(n2));
            Assert.AreEqual<string>(n3, target.GetFullName(n3).ToString());
            Assert.AreEqual<string>(n4, target.GetFullName(n4).ToString());
            Assert.AreEqual<string>(n5, target.GetFullName(n5).ToString());
            Assert.AreEqual<string>(n6, target.GetFullName(n6).ToString());

            Assert.AreNotEqual<MetadataFullName>(target.GetFullName(n5), target.GetFullName(n7));
        }

        [TestMethod]
        public void TestGetFullNamePerformance() {
            MetadataFullNameContainer target = new MetadataFullNameContainer(new MetadataNameContainer());

            var count = 10000;
            var n = new string[count];

            for (int i = 0; i < count; i++) {
                n[i] = "System.Configuration.P" + i.ToString();
            }

            //create
            for (int i = 0; i < count; i++) {
                var result = target.GetFullName(n[i]);
            }

            for (int i = 0; i < count /100; i++) {
                for (int j = 0; j < count; j++) {
                    var result = target.GetFullName(n[j]);
                }
            }
        }
    }
}
