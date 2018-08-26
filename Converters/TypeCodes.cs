namespace Utilities.Converters
{
    /// <summary>
    /// Nullable TypeCodes are +20 from non-nullable equivalent TypeCodes
    /// </summary>
    public enum TypeCodes
    {
        // Built-in TypeCodes
        Empty = 0,
        Object = System.TypeCode.Object, // 1
        DBNull = System.TypeCode.DBNull, // 2
        Boolean = System.TypeCode.Boolean, // 3
        Char = System.TypeCode.Char, // 4
        SByte = System.TypeCode.SByte, // 5
        Byte = System.TypeCode.Byte, // 6
        Int16 = System.TypeCode.Int16, // 7
        UInt16 = System.TypeCode.UInt16, // 8
        Int32 = System.TypeCode.Int32, // 9
        UInt32 = System.TypeCode.UInt32, // 10
        Int64 = System.TypeCode.Int64, // 11
        UInt64 = System.TypeCode.UInt64, // 12
        Single = System.TypeCode.Single, // 13
        Double = System.TypeCode.Double, // 14
        Decimal = System.TypeCode.Decimal, // 15
        DateTime = System.TypeCode.DateTime, // 16
        String = System.TypeCode.String, // 18

        // Value-Type Custom TypeCodes
        TimeSpan = 17,
        DateTimeOffset = 19,
        // Array-Type Custom TypeCodes
        ByteArray,
        Base64CharArray,
        BitArray,
        // Nullable Value-Type Custom TypeCodes
        BooleanNullable,
        CharNullable,
        SByteNullable,
        ByteNullable,
        Int16Nullable,
        UInt16Nullable,
        Int32Nullable,
        UInt32Nullable,
        Int64Nullable,
        UInt64Nullable,
        SingleNullable,
        DoubleNullable,
        DecimalNullable,
        DateTimeNullable,
        TimeSpanNullable,
        DataTable, // filler for String
        DateTimeOffsetNullable,
        // Reference-Type Custom TypeCodes

        Count = DateTimeOffsetNullable
    }
}
