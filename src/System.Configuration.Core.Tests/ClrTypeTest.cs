using System.Collections.Generic;
using System.Configuration.Core.Metadata;
using System.Configuration.Core.Metadata.Clr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {
    [TestClass]
    public class ClrTypeTest {
        [TestMethod]
        public void TestClrObjectEquals() {
            var controlType = ClrType.GetClrType(typeof(Control));
            var controlType2 = ClrType.GetClrType(typeof(Control));

            Assert.AreEqual<ClrType>(controlType, controlType2);
            HashSet<IType> dict = new HashSet<IType>();
            dict.Add(controlType);
            Assert.IsTrue(dict.Contains(controlType2));
            Assert.IsFalse(dict.Contains(ClrType.GetClrType(typeof(Window))));

            Assert.IsNotNull(controlType.ToString());
        }
    }
}
