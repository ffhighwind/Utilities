using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Converters
{
    public static class Converters
    {
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        public static Converter<object, object> GetConverter(Type input, Type output)
        {
            return GetConverter(output)(input);
        }

        public static Converter<object, object> GetNullableConverter(Type input, Type output)
        {
            Converter<object, object> converter = GetConverter(output)(input);
            object nullableConverter(object value)
            {
                return value == null ? null : converter(value);
            }
            return nullableConverter;
        }

        public static Func<Type, Converter<object, object>> GetConverter(Type output)
        {
            if (ConverterMap.TryGetValue(output, out Func<Type, Converter<object, object>> converter))
                return converter;
            return NonConverter;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="typeCode"></param>
        /// <returns></returns>
        public static object ChangeType(object value, Type type)
        {
            if (value == null)
                return null;
            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode) {
                case TypeCode.Boolean:
                    return ToBoolean(value);
                case TypeCode.Char:
                    return Convert.ToChar(value);
                case TypeCode.SByte:
                    return Convert.ToSByte(value);
                case TypeCode.Byte:
                    return Convert.ToByte(value);
                case TypeCode.Int16:
                    return Convert.ToInt16(value);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(value);
                case TypeCode.Int32:
                    return Convert.ToInt32(value);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(value);
                case TypeCode.Int64:
                    return Convert.ToInt64(value);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value);
                case TypeCode.Single:
                    return Convert.ToSingle(value);
                case TypeCode.Double:
                    return Convert.ToDouble(value);
                case TypeCode.Decimal:
                    return Convert.ToDecimal(value);
                case TypeCode.DateTime:
                    return ToDateTime(value);
                case TypeCode.String:
                    return ToString(value);
                case TypeCode.Object:
                    if (type == typeof(TimeSpan))
                        return ToTimeSpan(value);
                    else if (type == typeof(DateTimeOffset))
                        return ToDateTimeOffset(value);
                    else if (type == typeof(char[]))
                        return ToChars(value);
                    else if (type == typeof(byte[]))
                        return ToBytes(value);
                    return value;
                case TypeCode.DBNull:
                    throw new InvalidCastException("Invalid cast: TypeCode.DBNull");
                case TypeCode.Empty:
                    throw new InvalidCastException("Invalid Cast: TypeCode.Empty");
                default:
                    throw new ArgumentException("Argument: unknown TypeCode " + typeCode.ToString());
            }
        }

        /// <summary>
        /// Converters for most basic types. converterMap(outputType)(inputType) returns a Converter from inputType to outputType
        /// </summary>
        private static readonly Dictionary<Type, Func<Type, Converter<object, object>>> ConverterMap = new Dictionary<Type, Func<Type, Converter<object, object>>>() {
            { typeof(string), ToString },
            { typeof(DateTime), ToDateTime },
            { typeof(DateTimeOffset), ToDateTimeOffset },
            { typeof(TimeSpan), ToTimeSpan },
            { typeof(char[]), ToChars },
            { typeof(byte[]), ToBytes },
            { typeof(short), ToInt16 }, // equivalent to Convert.ToInt16
            { typeof(int), ToInt32 }, // equivalent to Convert.ToInt32
            { typeof(long), ToInt64 }, // equivalent to Convert.ToInt64
            { typeof(ushort), ToUInt16 }, // equivalent to Convert.ToUInt16
            { typeof(uint), ToUInt32 }, // equivalent to Convert.ToUInt32
            { typeof(ulong), ToUInt64 }, // equivalent to Convert.ToUInt64
            { typeof(bool), ToBoolean },
            { typeof(byte), ToByte }, // equivalent to Convert.ToByte
            { typeof(sbyte), ToSByte }, // equivalent to Convert.ToSByte
            { typeof(char), ToChar }, // equivalent to Convert.ToChar
            { typeof(float), ToSingle }, // equivalent to Convert.ToSingle
            { typeof(double), ToDouble }, // equivalent to Convert.ToDouble
            { typeof(decimal), ToDecimal }, // equivalent to Convert.ToDecimal
        };

        /// <summary>
        /// A converter that boxes and unboxes a type and then does nothing.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>The same object that was input.</returns>
        private static object NoConvert<T>(object obj) { return (T) obj; }

        private static readonly Func<Type, Converter<object, object>> NonConverter = (inp) => { return NoConvert<object>; };

        // ToString
        public static object ToString(object value)
        {
            return ToString(value.GetType())(value);
        }

        public static Converter<object, object> ToString(Type input)
        {
            Converter<object, object> converter;
            if (input == typeof(DateTime))
                converter = DateTimeToString;
            else if (input == typeof(TimeSpan))
                converter = TimeSpanToString;
            else if (input == typeof(DateTimeOffset))
                converter = DateTimeOffsetToString;
            else if (input == typeof(char[]))
                converter = CharsToString;
            else if (input == typeof(byte[]))
                converter = BytesToString;
            else
                converter = System.Convert.ToString;
            return converter;
        }

        private static object DateTimeToString(object value)
        {
            return ((DateTime) value).ToString("M-d-yyyy H:mm:ss.fff");
        }

        private static object TimeSpanToString(object value)
        {
            return ((TimeSpan) value).ToString("h:mm:ss.fff");
        }

        private static object DateTimeOffsetToString(object value)
        {
            return Extensions.ToDateTime((DateTimeOffset) value).ToString("M-d-yyyy H:mm:ss.fff");
        }

        private static object CharsToString(object value)
        {
            return new string((char[]) value);
        }

        private static object BytesToString(object value)
        {
            return DefaultEncoding.GetString((byte[]) value);
        }

        // ToDateTime
        public static object ToDateTime(object value)
        {
            return ToDateTime(value.GetType())(value);
        }

        public static Converter<object, object> ToDateTime(Type input)
        {
            Converter<object, object> converter;
            if (input == typeof(TimeSpan))
                converter = TimeSpanToDateTime;
            else if (input == typeof(DateTimeOffset))
                converter = DateTimeOffsetToDateTime;
            else
                converter = ObjectToDateTime;
            return converter;
        }

        private static object TimeSpanToDateTime(object value)
        {
            return new DateTime(((TimeSpan) value).Ticks);
        }

        private static object DateTimeOffsetToDateTime(object value)
        {
            DateTimeOffset dateTimeOff = (DateTimeOffset) value;
            if (dateTimeOff.Offset.Equals(TimeSpan.Zero))
                return dateTimeOff.UtcDateTime;
            else if (dateTimeOff.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTimeOff.DateTime)))
                return DateTime.SpecifyKind(dateTimeOff.DateTime, DateTimeKind.Local);
            else
                return dateTimeOff.DateTime;
        }

        private static object ObjectToDateTime(object value)
        {
            return System.Convert.ToDateTime(value);
        }

        // ToDateTimeOffset
        public static object ToDateTimeOffset(object value)
        {
            return ToDateTimeOffset(value.GetType())(value);
        }

        public static Converter<object, object> ToDateTimeOffset(Type input)
        {
            if (input == typeof(string))
                return StringToDateTimeOffset;
            if (input == typeof(DateTime))
                return DateTimeToDateTimeOffset;
            else if (input == typeof(TimeSpan))
                return TimeSpanToDateTimeOffset;
            else
                return NoConvert<DateTimeOffset>;
        }

        private static object StringToDateTimeOffset(object value)
        {
            return DateTimeOffset.Parse((string) value);
        }

        private static object DateTimeToDateTimeOffset(object value)
        {
            return new DateTimeOffset((DateTime) value);
        }

        private static object TimeSpanToDateTimeOffset(object value)
        {
            return new DateTimeOffset(0, (TimeSpan) value);
        }

        // ToTimeSpan
        public static object ToTimeSpan(object value)
        {
            return ToTimeSpan(value.GetType())(value);
        }

        public static Converter<object, object> ToTimeSpan(Type input)
        {
            Converter<object, object> converter;
            if (input == typeof(string))
                converter = StringToTimeSpan;
            else if (input == typeof(DateTime))
                converter = DateTimeToTimeSpan;
            else if (input == typeof(DateTimeOffset))
                converter = DateTimeOffsetToTimeSpan;
            else
                converter = NoConvert<TimeSpan>;
            return converter;
        }

        private static object StringToTimeSpan(object value)
        {
            return TimeSpan.Parse((string) value);
        }

        private static object DateTimeToTimeSpan(object value)
        {
            return ((DateTime) value).TimeOfDay;
        }

        private static object DateTimeOffsetToTimeSpan(object value)
        {
            return Extensions.ToDateTime((DateTimeOffset) value).TimeOfDay;
        }

        // ToChars
        public static object ToChars(object value)
        {
            return ToChars(value.GetType())(value);
        }

        public static Converter<object, object> ToChars(Type input)
        {
            Converter<object, object> converter;
            if (input == typeof(byte[]))
                converter = BytesToChars;
            else if (input == typeof(string))
                converter = StringToChars;
            else
                converter = NoConvert<char[]>;
            return converter;
        }

        private static object BytesToChars(object value)
        {
            return DefaultEncoding.GetChars((byte[]) value);
        }

        private static object StringToChars(object value)
        {
            return ((string) value).ToCharArray();
        }

        // ToBytes
        public static object ToBytes(object value)
        {
            return ToBytes(value.GetType())(value);
        }

        public static Converter<object, object> ToBytes(Type input)
        {
            if (input == typeof(string))
                return StringToBytes;
            else if (input == typeof(byte))
                return ByteToBytes;
            else if (input == typeof(char))
                return CharToBytes;
            else if (input == typeof(char[]))
                return CharsToBytes;
            return NoConvert<byte[]>;
        }

        private static object StringToBytes(object value)
        {
            return DefaultEncoding.GetBytes(((string) value).ToCharArray());
        }

        private static object CharsToBytes(object value)
        {
            return DefaultEncoding.GetBytes((char[]) value);
        }

        private static object CharToBytes(object value)
        {
            return DefaultEncoding.GetBytes(new char[] { (char) value });
        }

        private static object ByteToBytes(object value)
        {
            return new byte[] { (byte) value };
        }

        // ToInt16
        public static object ToInt16(object value)
        {
            return System.Convert.ToInt16(value);
            ////return ToInt16(value.GetType())(value);
        }

        public static Converter<object, object> ToInt16(Type input)
        {
            return ObjectToInt16;
        }

        private static object ObjectToInt16(object value)
        {
            return System.Convert.ToInt16(value);
        }

        // ToInt32
        public static object ToInt32(object value)
        {
            return ToInt32(value.GetType())(value);
        }
        public static Converter<object, object> ToInt32(Type input)
        {
            Converter<object, object> converter;
            if (input == typeof(string))
                converter = StringToInt32;
            else
                converter = ObjectToInt32;
            return converter;
        }

        private static object StringToInt32(object value)
        {
            return int.Parse((string) value);
        }

        private static object ObjectToInt32(object value)
        {
            return System.Convert.ToInt32(value);
        }

        // ToInt64
        public static object ToInt64(object value)
        {
            return System.Convert.ToInt64(value);
            ////return ToInt64(value.GetType())(value);
        }

        public static Converter<object, object> ToInt64(Type input)
        {
            return ObjectToInt64;
        }

        private static object ObjectToInt64(object value)
        {
            return System.Convert.ToInt64(value);
        }

        // ToUInt16
        public static object ToUInt16(object value)
        {
            return System.Convert.ToUInt16(value);
            ////return ToUInt16(value.GetType())(value);
        }

        public static Converter<object, object> ToUInt16(Type input)
        {
            return ObjectToUInt16;
        }

        public static object ObjectToUInt16(object value)
        {
            return System.Convert.ToUInt16(value);
        }

        // ToUInt32
        public static object ToUInt32(object value)
        {
            return System.Convert.ToUInt32(value);
            ////return ToUInt32(value.GetType())(value);
        }

        public static Converter<object, object> ToUInt32(Type input)
        {
            return ObjectToUInt32;
        }

        private static object ObjectToUInt32(object value)
        {
            return System.Convert.ToUInt32(value);
        }

        // ToUInt64
        public static object ToUInt64(object value)
        {
            return System.Convert.ToUInt64(value);
            ////return ToUInt64(value.GetType())(value);
        }

        public static Converter<object, object> ToUInt64(Type input)
        {
            return ObjectToUInt64;
        }

        private static object ObjectToUInt64(object value)
        {
            return System.Convert.ToUInt64(value);
        }

        // ToBoolean
        public static object ToBoolean(object value)
        {
            return ObjectToBoolean(value);
            ////return ToBoolean(value.GetType())(value);
        }

        public static Converter<object, object> ToBoolean(Type input)
        {
            return ObjectToBoolean;
        }

        private static object ObjectToBoolean(object value)
        {
            if (value is string str) {
                if (string.Equals(str, "true", StringComparison.OrdinalIgnoreCase))
                    return true;
                if (string.Equals(str, "false", StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return System.Convert.ToBoolean(value);
        }

        // ToByte
        public static object ToByte(object value)
        {
            return System.Convert.ToByte(value);
            ////return ToByte(value.GetType())(value);
        }

        public static Converter<object, object> ToByte(Type input)
        {
            return ObjectToByte;
        }

        private static object ObjectToByte(object value)
        {
            return System.Convert.ToByte(value);
        }

        // ToSByte
        public static object ToSByte(object value)
        {
            return System.Convert.ToSByte(value);
            ////return ToSByte(value.GetType())(value);
        }

        public static Converter<object, object> ToSByte(Type input)
        {
            return ObjectToSByte;
        }

        private static object ObjectToSByte(object value)
        {
            return System.Convert.ToSByte(value);
        }

        // ToChar
        public static object ToChar(object value)
        {
            return System.Convert.ToChar(value);
            ////return ToChar(value.GetType())(value);
        }

        public static Converter<object, object> ToChar(Type input)
        {
            return ObjectToChar;
        }

        private static object ObjectToChar(object value)
        {
            return System.Convert.ToChar(value);
        }

        // ToSingle
        public static object ToSingle(object value)
        {
            return System.Convert.ToSingle(value);
            ////return ToSingle(value.GetType())(value);
        }

        public static Converter<object, object> ToSingle(Type input)
        {
            return ObjectToSingle;
        }

        private static object ObjectToSingle(object value)
        {
            return System.Convert.ToSingle(value);
        }

        // ToDouble
        public static object ToDouble(object value)
        {
            return System.Convert.ToDouble(value);
            ////return ToDouble(value.GetType())(value);
        }
        public static Converter<object, object> ToDouble(Type input)
        {
            return ObjectToDouble;
        }

        private static object ObjectToDouble(object value)
        {
            return System.Convert.ToDouble(value);
        }

        // ToDecimal
        public static object ToDecimal(object value)
        {
            return System.Convert.ToDecimal(value);
            ////return ToDecimal(value.GetType())(value);
        }

        public static Converter<object, object> ToDecimal(Type input)
        {
            return ObjectToDecimal;
        }

        private static object ObjectToDecimal(object value)
        {
            return System.Convert.ToDecimal(value);
        }
    }
}
