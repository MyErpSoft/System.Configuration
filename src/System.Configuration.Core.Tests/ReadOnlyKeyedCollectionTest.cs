using System;
using System.Collections.Generic;
using System.Configuration.Core.Metadata;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Configuration.Core.Tests {

    [TestClass]
    public class ReadOnlyKeyedCollectionTest {

        [TestMethod]
        public void TestReadonlyKeyedCollection() {
            var count = 1000;
            //测试并发环境
            Dictionary<string, CollectionItem> dict = new Dictionary<string, CollectionItem>(StringComparer.Ordinal);
            for (int j = 0; j < count; j++) {
                var key = "K" + j.ToString();
                CollectionItem item = new CollectionItem() { Name = key };
                dict.Add(key, item);
            }
            var col = new MyReadOnlyKeyedCollection(dict);

            //在并发环境下
            Parallel.For(0, count, (i) => {
                var key = "K" + i.ToString();
                col.ContainsKey(key);

                CollectionItem ci;
                Assert.IsTrue(col.TryGetValue(key, out ci));
                foreach (var item in col) {
                    Assert.IsTrue(item != null);
                }
            });

            Assert.AreEqual(count, col.Count);
            var array = col.ToArray();
            Assert.AreEqual(count, array.Length);
            for (int i = 0; i < count; i++) {
                Assert.ReferenceEquals(array[i], col[i]);
            }

            
        }
    }

    internal sealed class MyReadOnlyKeyedCollection : ReadOnlyKeyedCollection<string, CollectionItem> {
        public MyReadOnlyKeyedCollection(Dictionary<string,CollectionItem> dict):base(dict) {

        }
        protected override string GetKeyForItem(CollectionItem item) {
            return item.Name;
        }
    }

    internal sealed class CollectionItem {
        public string Name { get; set; }
    }
}
