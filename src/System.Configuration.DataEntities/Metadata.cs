using System;

namespace System.Configuration.DataEntities {


    public class Metadata<T> {

        #region ctor

        public Metadata() : this(Guid.NewGuid(), default(T)) { }

        public Metadata(Guid objectId)
            : this(objectId, default(T)) {
        }

        public Metadata(Guid objectId, T baseObject) {
            this._objectId = objectId;
            this._baseObject = baseObject;
        }

        #endregion

        #region Properties

        private readonly Guid _objectId;
        public Guid ObjectId {
            get { return this._objectId; }
        }

        private readonly T _baseObject;
        public T BaseObject {
            get { return this._baseObject; }
        }

        #endregion

    }

    public class Property<T> where T : struct {
        public T GetValue(ref Nullable<T> stroge) {
            if (stroge.HasValue) {
                return stroge.Value;
            }

            return default(T);
        }

        public void SetValue(ref Nullable<T> stroge,T value) {
            stroge = value;
        }
    }

    public class TestObject {
        private int? _dbType;
        private static readonly Property<int> DbTypeProperty;

        public int DbType {
            get { return DbTypeProperty.GetValue(ref this._dbType); }
            set { DbTypeProperty.SetValue(ref this._dbType, value); }
        }
    }
}
