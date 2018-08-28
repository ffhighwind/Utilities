using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utilities.Converters
{
    public static class Converters
    {
        /// <summary>
        /// The default <see cref="Encoding"/> from <see cref="byte"/>[] to <see cref="char"/> and <see cref="string"/>.
        /// </summary>
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        public static Func<Tin, Tout> ObjectToObject<Tin, Tout>(
            BindingFlags inFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly,
            BindingFlags outFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            where Tin : class
            where Tout : class, new()
        {
            return ObjectToObject<Tin, Tout>(null, inFlags, outFlags);
        }

        public static Func<Tin, Tout> ObjectToObject<Tin, Tout>(
            IReadOnlyList<string> propertyNames,
            BindingFlags inFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly,
            BindingFlags outFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            where Tin : class
            where Tout : class, new()
        {
            PropertyInfo[] pi = typeof(Tout).GetProperties(inFlags);
            PropertyInfo[] po = typeof(Tout).GetProperties(outFlags);
            List<string> names = pi.Select(p => p.Name).Where(piName => po.Select(pO => pO.Name).Contains(piName)).ToList();
            if (propertyNames != null)
                names = names.Where(name => propertyNames.Contains(name)).ToList();
            List<PropertyInfo> piIntersect = pi.Where(p => names.Contains(p.Name)).ToList();
            List<PropertyInfo> poIntersect = new List<PropertyInfo>();
            for (int i = 0; i < piIntersect.Count; i++) {
                string name = piIntersect[i].Name;
                poIntersect.Add(po.First(p => p.Name == name));
            }
            return ObjectToObject<Tin, Tout>(piIntersect, poIntersect);
        }

        public static Func<Tin, Tout> ObjectToObject<Tin, Tout>(IReadOnlyList<PropertyInfo> pinfoIn, IReadOnlyList<PropertyInfo> pinfoOut)
            where Tin : class
            where Tout : class, new()
        {
            Func<object, object>[] converters = new Func<object, object>[pinfoIn.Count];
            for (int i = 0; i < pinfoOut.Count; i++) {
                converters[i] = GetConverter(pinfoIn[i].PropertyType, pinfoOut[i].PropertyType);
            }
            Tout objToObj(Tin input)
            {
                Tout tout = new Tout();
                for (int i = 0; i < pinfoOut.Count; i++) {
                    pinfoOut[i].SetValue(tout, pinfoIn[i].GetValue(input));
                }
                return tout;
            }
            return objToObj;
        }

        /// <summary>
        /// Creates a function that converts an <see cref="IEnumerable{T}"/> of Tin to a Tout.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type>"/> of the object.</typeparam>
        /// <returns>A function that converts an <see cref="IEnumerable{T}"/> Tin to a Tout.</returns>
        public static Func<IReadOnlyList<Tin>, Tout> ListToObject<Tin, Tout>(
            BindingFlags flags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            where Tout : class, new()
        {
            return CreateListToObject<Tin, Tout>(typeof(Tout).GetProperties(flags));
        }

        public static Func<IReadOnlyList<Tin>, Tout> ListToObject<Tin, Tout>(
            IReadOnlyList<string> propertyNames,
            BindingFlags flags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            where Tout : class, new()
        {
            PropertyInfo[] pinfos = typeof(Tout).GetProperties(flags);
            List<PropertyInfo> tmp = new List<PropertyInfo>(propertyNames.Count);
            bool safeConvert = false;
            for (int i = 0; i < tmp.Count; i++) {
                PropertyInfo pinfo = pinfos.FirstOrDefault(pi => pi.Name == propertyNames[i]);
                tmp.Add(pinfo);
                if (pinfo == null)
                    safeConvert = true;
            }
            return safeConvert ? CreateListToObjectSafe<Tin, Tout>(tmp) : CreateListToObject<Tin, Tout>(tmp);
        }

        private static Func<IReadOnlyList<Tin>, Tout> CreateListToObject<Tin, Tout>(IReadOnlyList<PropertyInfo> pinfos) where Tout : class, new()
        {
            Func<object, object>[] converters = new Func<object, object>[pinfos.Count];
            for (int i = 0; i < pinfos.Count; i++) {
                converters[i] = GetConverter(pinfos[i].PropertyType)(typeof(Tin));
            }
            Tout listToObj(IReadOnlyList<Tin> list)
            {
                Tout obj = new Tout();
                for (int i = 0; i < list.Count; i++) {
                    pinfos[i].SetValue(obj, converters[i](list[i]));
                }
                return obj;
            }
            return listToObj;
        }

        private static Func<IReadOnlyList<Tin>, Tout> CreateListToObjectSafe<Tin, Tout>(IReadOnlyList<PropertyInfo> pinfos) where Tout : class, new()
        {
            Func<object, object>[] converters = new Func<object, object>[pinfos.Count];
            for (int i = 0; i < pinfos.Count; i++) {
                if (pinfos[i] != null)
                    converters[i] = GetConverter(pinfos[i].PropertyType)(typeof(Tin));
            }
            Tout listToObjectSafe(IReadOnlyList<Tin> list)
            {
                Tout obj = new Tout();
                int count = Math.Max(list.Count, pinfos.Count);
                for (int i = 0; i < count; i++) {
                    if (pinfos[i] != null)
                        pinfos[i].SetValue(obj, converters[i](list[i]));
                }
                return obj;
            }
            return listToObjectSafe;
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts from one type to another.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <param name="output">The output <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts from one type to another.</returns>
        public static Func<object, object> GetConverter(Type input, Type output)
        {
            return GetConverter(output)(input);
        }

        /// <summary>
        /// Creates a converter that accepts null values. This should be used if the input type
        /// and output type are <see cref="Nullable"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <param name="output">The output <see cref="Type"/>.</param>
        /// <returns>A function that converts objects from one type to another.</returns>
        public static Func<object, object> GetNullableConverter(Type input, Type output)
        {
            if (!output.IsNullable())
                return GetConverter(input, output);
            Func<object, object> converter = GetConverter(output)(input);
            object nullableConverter(object value)
            {
                return value == null ? null : converter(value);
            }
            return nullableConverter;
        }

        public static Func<Type, Func<object, object>> GetConverter(Type output)
        {
            if (ConverterMap.TryGetValue(output, out Func<Type, Func<object, object>> converter))
                return converter;
            return NonConverter;
        }

        /// <summary>
        /// Returns an object of the specified type whose value is equivalent to the specified object.
        /// </summary>
        /// <param name="value">An object that implements the System.IConvertible interface.</param>
        /// <param name="typeCode">The type of object to return.</param>
        /// <returns>An object whose type is conversionType and whose value is equivalent to value.</returns>
        public static object ChangeType(object value, Type type)
        {
            if (value == null || value == DBNull.Value)
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
        /// Maps input/output types to a <see cref="Converter{TInput, TOutput}"/>.
        /// </summary>
        private static readonly Dictionary<Type, Func<Type, Func<object, object>>> ConverterMap = new Dictionary<Type, Func<Type, Func<object, object>>>() {
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
        /// A converter that casts and unboxes an <see cref="object"/> .
        /// </summary>
        /// <param name="obj">The <see cref="object"/>  to convert.</param>
        /// <returns>The same <see cref="object"/>  that was input.</returns>
        private static object NoConvert<T>(object obj) { return (T) obj; }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="value">The <see cref="object"/> to convert.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that does nothing.</returns>
        private static object NoConvert(object value) { return value; }

        /// <summary>
        /// A function that returns a <see cref="Converter{TInput, TOutput}"/> that does nothing.
        /// </summary>
        private static readonly Func<Type, Func<object, object>> NonConverter = (inp) => { return NoConvert; };

        #region ToString
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="string"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="string"/> representation.</returns>
        public static object ToString(object value)
        {
            return ToString(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="string"/>.</returns>
        public static Func<object, object> ToString(Type input)
        {
            Func<object, object> converter;
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
        #endregion // ToString

        #region ToDateTime
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="DateTime"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="DateTime"/> representation.</returns>
        public static object ToDateTime(object value)
        {
            return ToDateTime(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="DateTime"/>.</returns>
        public static Func<object, object> ToDateTime(Type input)
        {
            Func<object, object> converter;
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
        #endregion // ToDateTime

        #region ToDateTimeOffset
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="DateTimeOffset"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="DateTimeOffset"/> representation.</returns>
        public static object ToDateTimeOffset(object value)
        {
            return ToDateTimeOffset(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="DateTimeOffset"/>.</returns>
        public static Func<object, object> ToDateTimeOffset(Type input)
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
        #endregion // ToDateTimeOffset

        #region ToTimeSpan
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="TimeSpan"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="TimeSpan"/> representation.</returns>
        public static object ToTimeSpan(object value)
        {
            return ToTimeSpan(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="TimeSpan"/>.</returns>
        public static Func<object, object> ToTimeSpan(Type input)
        {
            Func<object, object> converter;
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
        #endregion // ToTimeSpan

        #region ToChars
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="char"/>[] representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="char"/>[] representation.</returns>
        public static object ToChars(object value)
        {
            return ToChars(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="char"/>[].
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="char"/>[].</returns>
        public static Func<object, object> ToChars(Type input)
        {
            Func<object, object> converter;
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
        #endregion // ToChars

        #region // ToBytes
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="byte"/>[] representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="byte"/>[] representation.</returns>
        public static object ToBytes(object value)
        {
            return ToBytes(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="byte"/>[].
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="byte"/>[].</returns>
        public static Func<object, object> ToBytes(Type input)
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
        #endregion // ToBytes

        #region ToInt16
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="short"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="short"/> representation.</returns>
        public static object ToInt16(object value)
        {
            return System.Convert.ToInt16(value);
            ////return ToInt16(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="short"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="short"/>.</returns>
        public static Func<object, object> ToInt16(Type input)
        {
            return ObjectToInt16;
        }

        private static object ObjectToInt16(object value)
        {
            return System.Convert.ToInt16(value);
        }

        #region ToInt32
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="int"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="int"/> representation.</returns>
        public static object ToInt32(object value)
        {
            return ToInt32(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to an <see cref="int"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to an <see cref="int"/>.</returns>
        public static Func<object, object> ToInt32(Type input)
        {
            Func<object, object> converter;
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
        #endregion // ToInt32

        #endregion ToInt64
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="long"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="long"/> representation.</returns>
        public static object ToInt64(object value)
        {
            return System.Convert.ToInt64(value);
            ////return ToInt64(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="long"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="long"/>.</returns>
        public static Func<object, object> ToInt64(Type input)
        {
            return ObjectToInt64;
        }

        private static object ObjectToInt64(object value)
        {
            return System.Convert.ToInt64(value);
        }

        #region ToUInt16
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="ushort"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="ushort"/> representation.</returns>
        public static object ToUInt16(object value)
        {
            return System.Convert.ToUInt16(value);
            ////return ToUInt16(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="ushort"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="ushort"/>.</returns>
        public static Func<object, object> ToUInt16(Type input)
        {
            return ObjectToUInt16;
        }

        public static object ObjectToUInt16(object value)
        {
            return System.Convert.ToUInt16(value);
        }
        #endregion

        #region ToUInt32
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="uint"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="uint"/> representation.</returns>
        public static object ToUInt32(object value)
        {
            return System.Convert.ToUInt32(value);
            ////return ToUInt32(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="uint"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="uint"/>.</returns>
        public static Func<object, object> ToUInt32(Type input)
        {
            return ObjectToUInt32;
        }

        private static object ObjectToUInt32(object value)
        {
            return System.Convert.ToUInt32(value);
        }
        #endregion // ToUInt32

        #region ToUInt64
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="ulong"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="ulong"/> representation.</returns>
        public static object ToUInt64(object value)
        {
            return System.Convert.ToUInt64(value);
            ////return ToUInt64(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="ulong"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="ulong"/>.</returns>
        public static Func<object, object> ToUInt64(Type input)
        {
            return ObjectToUInt64;
        }

        private static object ObjectToUInt64(object value)
        {
            return System.Convert.ToUInt64(value);
        }
        #endregion // ToUInt32

        #region ToBoolean
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="bool"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="bool"/> representation.</returns>
        public static object ToBoolean(object value)
        {
            return ObjectToBoolean(value);
            ////return ToBoolean(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="bool"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="bool"/>.</returns>
        public static Func<object, object> ToBoolean(Type input)
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
                return System.Convert.ToBoolean(str);
            }
            return System.Convert.ToBoolean(value);
        }
        #endregion // ToBoolean

        #region ToByte
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="byte"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="byte"/> representation.</returns>
        public static object ToByte(object value)
        {
            return System.Convert.ToByte(value);
            ////return ToByte(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="byte"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="byte"/>.</returns>
        public static Func<object, object> ToByte(Type input)
        {
            return ObjectToByte;
        }

        private static object ObjectToByte(object value)
        {
            return System.Convert.ToByte(value);
        }
        #endregion // ToByte

        #region ToSByte
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="sbyte"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="sbyte"/> representation.</returns>
        public static object ToSByte(object value)
        {
            return System.Convert.ToSByte(value);
            ////return ToSByte(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to an <see cref="sbyte"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to an <see cref="sbyte"/>.</returns>
        public static Func<object, object> ToSByte(Type input)
        {
            return ObjectToSByte;
        }

        private static object ObjectToSByte(object value)
        {
            return System.Convert.ToSByte(value);
        }
        #endregion // ToSByte

        #region ToChar
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="char"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="char"/> representation.</returns>
        public static object ToChar(object value)
        {
            return System.Convert.ToChar(value);
            ////return ToChar(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="char"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="char"/>.</returns>
        public static Func<object, object> ToChar(Type input)
        {
            return ObjectToChar;
        }

        private static object ObjectToChar(object value)
        {
            return System.Convert.ToChar(value);
        }
        #endregion // ToChar

        #region ToSingle
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="float"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="float"/> representation.</returns>
        public static object ToSingle(object value)
        {
            return System.Convert.ToSingle(value);
            ////return ToSingle(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="float"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="float"/>.</returns>
        public static Func<object, object> ToSingle(Type input)
        {
            return ObjectToSingle;
        }

        private static object ObjectToSingle(object value)
        {
            return System.Convert.ToSingle(value);
        }
        #endregion // ToSingle

        #region ToDouble
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="double"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="double"/> representation.</returns>
        public static object ToDouble(object value)
        {
            return System.Convert.ToDouble(value);
            ////return ToDouble(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="double"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="double"/>.</returns>
        public static Func<object, object> ToDouble(Type input)
        {
            return ObjectToDouble;
        }

        private static object ObjectToDouble(object value)
        {
            return System.Convert.ToDouble(value);
        }
        #endregion // ToDouble

        #region ToDecimal
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="decimal"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>Converts the value of the specified <see cref="object"/> to its equivalent <see cref="decimal"/> representation.</returns>
        public static object ToDecimal(object value)
        {
            return System.Convert.ToDecimal(value);
            ////return ToDecimal(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="decimal"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Converter{TInput, TOutput}"/> that converts objects from a <see cref="Type"/> to a <see cref="decimal"/>.</returns>
        public static Func<object, object> ToDecimal(Type input)
        {
            return ObjectToDecimal;
        }

        private static object ObjectToDecimal(object value)
        {
            return System.Convert.ToDecimal(value);
        }
        #endregion // ToDecimal
    }
}
