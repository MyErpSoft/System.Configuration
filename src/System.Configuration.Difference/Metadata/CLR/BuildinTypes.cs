using System;
using System.Xml;

namespace System.Configuration.Difference.Metadata.CLR {
   
    internal static class BuildinTypes {

        public readonly static IObjectType Int16 =
            new ValueObjectType<Int16>(
                XmlConvert.ToString,
                XmlConvert.ToInt16);

        public readonly static IObjectType Int32 =
            new ValueObjectType<int>(
                XmlConvert.ToString,
                XmlConvert.ToInt32);

        public readonly static IObjectType Int64 =
            new ValueObjectType<Int64>(
                XmlConvert.ToString,
                XmlConvert.ToInt64);

        public readonly static IObjectType UInt16 =
            new ValueObjectType<UInt16>(
                XmlConvert.ToString,
                XmlConvert.ToUInt16);

        public readonly static IObjectType UInt32 =
            new ValueObjectType<UInt32>(
                XmlConvert.ToString,
                XmlConvert.ToUInt32);

        public readonly static IObjectType UInt64 =
            new ValueObjectType<UInt64>(
                XmlConvert.ToString,
                XmlConvert.ToUInt64);

        public readonly static IObjectType Boolean =
            new ValueObjectType<Boolean>(
                XmlConvert.ToString,
                XmlConvert.ToBoolean);

        public readonly static IObjectType Byte =
            new ValueObjectType<Byte>(
                XmlConvert.ToString,
                XmlConvert.ToByte);

        public readonly static IObjectType Char =
            new ValueObjectType<Char>(
                XmlConvert.ToString,
                XmlConvert.ToChar);

        public readonly static IObjectType DateTime =
            new ValueObjectType<DateTime>(
                (obj) => XmlConvert.ToString(obj, XmlDateTimeSerializationMode.Local),
                (str) => XmlConvert.ToDateTime(str, XmlDateTimeSerializationMode.Local));

        public readonly static IObjectType DateTimeOffset =
            new ValueObjectType<DateTimeOffset>(
                XmlConvert.ToString,
                XmlConvert.ToDateTimeOffset);

        public readonly static IObjectType Decimal =
            new ValueObjectType<Decimal>(
                XmlConvert.ToString,
                XmlConvert.ToDecimal);

        public readonly static IObjectType Double =
            new ValueObjectType<Double>(
                XmlConvert.ToString,
                XmlConvert.ToDouble);

        public readonly static IObjectType Guid =
            new ValueObjectType<Guid>(
                XmlConvert.ToString,
                XmlConvert.ToGuid);

        public readonly static IObjectType SByte =
            new ValueObjectType<SByte>(
                XmlConvert.ToString,
                XmlConvert.ToSByte);

        public readonly static IObjectType Single =
            new ValueObjectType<Single>(
                XmlConvert.ToString,
                XmlConvert.ToSingle);

        public readonly static IObjectType String =
            new ValueObjectType<String>(
                (obj) => (string)obj,
                (str) => (string)str);

        public readonly static IObjectType TimeSpan =
            new ValueObjectType<TimeSpan>(
                XmlConvert.ToString,
                XmlConvert.ToTimeSpan);

    }
}
