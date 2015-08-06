using System;

namespace System.Configuration.Difference.Metadata.CLR {

    public class CLRObjectTypeContainer : ObjectTypeContainer<Type> {
        public static readonly CLRObjectTypeContainer Default =
            new CLRObjectTypeContainer(true);

        public CLRObjectTypeContainer(bool addBuildinType) {
            if (addBuildinType) {
                this.AddBuildinTypes();
            }
        }

        private void AddBuildinTypes() {
            this.Items.TryAdd(typeof(Int16), BuildinTypes.Int16);
            this.Items.TryAdd(typeof(Int32), BuildinTypes.Int32);
            this.Items.TryAdd(typeof(Int64), BuildinTypes.Int64);
            this.Items.TryAdd(typeof(UInt16), BuildinTypes.UInt16);
            this.Items.TryAdd(typeof(UInt32), BuildinTypes.UInt32);
            this.Items.TryAdd(typeof(UInt64), BuildinTypes.UInt64);
            
            this.Items.TryAdd(typeof(Boolean), BuildinTypes.Boolean);
            this.Items.TryAdd(typeof(Byte), BuildinTypes.Byte);
            this.Items.TryAdd(typeof(Char), BuildinTypes.Char);
            this.Items.TryAdd(typeof(DateTime), BuildinTypes.DateTime);
            this.Items.TryAdd(typeof(DateTimeOffset), BuildinTypes.DateTimeOffset);
            this.Items.TryAdd(typeof(Decimal), BuildinTypes.Decimal);
            this.Items.TryAdd(typeof(Double), BuildinTypes.Double);
            this.Items.TryAdd(typeof(Guid), BuildinTypes.Guid);
            this.Items.TryAdd(typeof(SByte), BuildinTypes.SByte);
            this.Items.TryAdd(typeof(Single), BuildinTypes.Single);
            this.Items.TryAdd(typeof(String), BuildinTypes.String);
            this.Items.TryAdd(typeof(TimeSpan), BuildinTypes.TimeSpan);
            
        }

        protected void AddValueObjectType<T>(Func<T, string> toStringFunc, Func<string, T> fromStringFunc) where T : struct {
            this.Items.TryAdd(typeof(T), new ValueObjectType<T>(toStringFunc, fromStringFunc));
        }

        /// <summary>
        /// Create IObjectType instance, override this method if you want check sourceType is supported.
        /// </summary>
        /// <param name="sourceType">clr type</param>
        /// <returns>a IObjectType instance.</returns>
        protected override IObjectType CreateObjectTypeCore(Type sourceType) {
            if (sourceType == null) {
                Utils.ExceptionHelper.ThrowArgumentNull("sourceType");
            }

            if (sourceType.IsEnum) {
                return new EnumType(sourceType);
            }

            if (sourceType.IsValueType) {
                return new StructObjectType(sourceType);
            }

            throw new NotImplementedException();
        }

    }
}
