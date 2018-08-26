using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Utilities.Converters
{
    /// <summary>
    /// Similar to System.<see cref="Converter"/>. It converts more types and null values
    /// will throw an exception unless they are converted to <see cref="Nullable"/> types.
    /// It also does not currently handle <see cref="IFormatProvider"/>.
    /// </summary>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.convert?view=netframework-4.7.2"/>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/conversions"/>
    /// <see href="https://referencesource.microsoft.com/#mscorlib/system/convert.cs,1503"/>
    public static class Converter
    {
        public static readonly DataTable table;
        public static readonly BitArray bit;
        public static object ChangeType(object value, Type conversionType)
        {
            return GetConverter(value.GetType(), conversionType)(value);
        }

        private static readonly Dictionary<Type, TypeCodes> TypeCodesMap = new Dictionary<Type, TypeCodes>() {
            { typeof(DBNull), TypeCodes.DBNull },
            { typeof(string), TypeCodes.String },
            { typeof(bool), TypeCodes.Boolean },
            { typeof(bool?), TypeCodes.BooleanNullable },
            { typeof(byte), TypeCodes.Byte },
            { typeof(byte?), TypeCodes.ByteNullable },
            { typeof(char), TypeCodes.Char },
            { typeof(char?), TypeCodes.CharNullable },
            { typeof(decimal), TypeCodes.Decimal },
            { typeof(decimal?), TypeCodes.DecimalNullable },
            { typeof(double), TypeCodes.Double },
            { typeof(double?), TypeCodes.DoubleNullable },
            { typeof(float), TypeCodes.Single },
            { typeof(float?), TypeCodes.SingleNullable },
            { typeof(int), TypeCodes.Int32 },
            { typeof(int?), TypeCodes.Int32Nullable },
            { typeof(long), TypeCodes.Int64 },
            { typeof(long?), TypeCodes.Int64Nullable },
            { typeof(sbyte), TypeCodes.SByte },
            { typeof(sbyte?), TypeCodes.SByteNullable },
            { typeof(short), TypeCodes.Int16 },
            { typeof(short?), TypeCodes.Int16Nullable },
            { typeof(DateTime), TypeCodes.DateTime },
            { typeof(DateTime?), TypeCodes.DateTimeNullable },
            { typeof(uint), TypeCodes.UInt32 },
            { typeof(uint?), TypeCodes.UInt32Nullable },
            { typeof(ulong), TypeCodes.UInt64 },
            { typeof(ulong?), TypeCodes.UInt64Nullable },
            { typeof(ushort), TypeCodes.UInt16 },
            { typeof(ushort?), TypeCodes.UInt16Nullable },
            { typeof(char[]), TypeCodes.Base64CharArray },
            { typeof(byte[]), TypeCodes.ByteArray },
            { typeof(TimeSpan), TypeCodes.TimeSpan },
            { typeof(TimeSpan?), TypeCodes.TimeSpanNullable },
            { typeof(DateTimeOffset), TypeCodes.DateTimeOffset },
            { typeof(DateTimeOffset?), TypeCodes.DateTimeOffsetNullable },
            // { typeof(System.Dynamic.ExpandoObject), TypeCode.Dynamic },
            { typeof(System.Data.DataTable), TypeCodes.DataTable },
            { typeof(System.Collections.BitArray), TypeCodes.BitArray },
        };

        public static TypeCodes GetTypeCode(Type inputType)
        {
            return TypeCodesMap.TryGetValue(inputType, out TypeCodes tyCode) ? tyCode : TypeCodes.Object;
        }

        public static Func<object, object> GetConverter(Type inputType, Type outputType)
        {
            throw new NotImplementedException();
        }

    }
}