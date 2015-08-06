using System.Data;

namespace System.Configuration.DataEntities.Test {

    class DatabaseType : Metadata<DatabaseType> {
        private DbType? _dbType;

        public DbType DbType {
            get {
                DbType result;
                if (this.TryGetDbType(out result)) {
                    return result;
                }

                return Data.DbType.Int32;
            }
            set { _dbType = value; }
        }

        protected virtual bool TryGetDbType(out DbType result) {
            if (this._dbType.HasValue) {
                result = this._dbType.Value;
            }

            if (this.BaseObject != null) {
                return this.BaseDatabaseType.TryGetDbType(out result);
            }

            result = Data.DbType.Int32;
            return false;
        }

        private DatabaseType BaseDatabaseType {
            get { return (DatabaseType)this.BaseObject; }
        }
    }
}
