//using System.Data.DataEntities.Dynamic;
//using System.Data.Metadata.DataEntities.Dynamic;

//namespace System.Configuration.Core {

//    internal sealed class ConfigurationEntityStorage : IDynamicEntityStorage {

//        public void ClearValue(DynamicEntityField field) {
//            throw new NotImplementedException();
//        }

//        public object GetValue(DynamicEntityField field) {
//            throw new NotImplementedException();
//        }

//        public void SetValue(DynamicEntityField field, object value) {
//            throw new NotImplementedException();
//        }

//        ////todo:此实现应该放到 配置对象存储区 实现，因为base需要延迟的加载，且不能缓存到当前对象中。
//        //public bool TryGetValue(IEntityProperty property, out object value) {
//        //    //首先在本地搜索数据，如果找到直接返回；
//        //    //否则，看是否包含Base引用，如果有，从Base获取。
//        //    if (TryGetLocaleValue(property, out value)) {
//        //        return true;
//        //    }

//        //    var basePart = this.Base;
//        //    if (basePart != null) {
//        //        //注意Base可能还有Base
//        //        return basePart.TryGetValue(property, out value);
//        //    }

//        //    return false;
//        //}
//    }
//}
