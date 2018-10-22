using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
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

        public static bool CanConvert(Type input, Type output)
        {
            return System.ComponentModel.TypeDescriptor.GetConverter(input).CanConvertTo(output);
        }

        public static Func<Tin, Tout> ObjectToObject<Tin, Tout>(
            BindingFlags inFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly,
            BindingFlags outFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            where Tin : class
            where Tout : class, new()
        {
            return ObjectToObject<Tin, Tout>(
                propertyNames: null,
                inFlags: inFlags,
                outFlags: outFlags);
        }

        public static Func<Tin, Tout> ObjectToObject<Tin, Tout>(
            IReadOnlyList<string> propertyNames,
            BindingFlags inFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly,
            BindingFlags outFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            where Tin : class
            where Tout : class, new()
        {
            IReadOnlyList<PropertyInfo> pi = typeof(Tin).GetProperties(inFlags);
            IReadOnlyList<PropertyInfo> po = typeof(Tout).GetProperties(outFlags)
                .Where(prop => pi.Select(p => p.Name).Contains(prop.Name)).ToList();
            if (propertyNames != null)
                po = po.Where(p => propertyNames.Contains(p.Name)).ToList();
            List<PropertyInfo> piUnion = new List<PropertyInfo>();
            for (int i = 0; i < po.Count; i++) {
                string name = po[i].Name;
                piUnion.Add(pi.First(p => p.Name == name));
            }
            return ObjectToObject<Tin, Tout>(piUnion, po);
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
                    object value = pinfoIn[i].GetValue(input);
                    if (value != null)
                        value = converters[i](value);
                    pinfoOut[i].SetValue(tout, value);
                }
                return tout;
            }
            return objToObj;
        }

        /// <summary>
        /// Creates a function that converts an <see cref="IReadOnlyList{T}"/> of Tin to a Tout.
        /// </summary>
        /// <typeparam name="Tin">The input <see cref="Type"/>.</typeparam>
        /// <typeparam name="Tout">The output <see cref="Type"/>.</typeparam>
        /// <param name="flags">Filters on the properties to obtain..</param>
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
            for (int i = 0; i < propertyNames.Count; i++) {
                PropertyInfo pinfo = pinfos.FirstOrDefault(pi => pi.Name == propertyNames[i]);
                if (pinfo == null) {
                    pinfo = pinfos.FirstOrDefault(pi => pi.Name.Equals(propertyNames[i], StringComparison.OrdinalIgnoreCase));
                }
                tmp.Add(pinfo);
            }
            for (int i = tmp.Count - 1; i >= 0; i--) {
                if (tmp[i] != null)
                    break;
                tmp.RemoveAt(i);
            }
            return CreateListToObject<Tin, Tout>(tmp);
        }

        private static Func<IReadOnlyList<Tin>, Tout> CreateListToObject<Tin, Tout>(IReadOnlyList<PropertyInfo> pinfos) where Tout : class, new()
        {
            Func<object, object>[] converters = new Func<object, object>[pinfos.Count];
            Type notNullableTin = Nullable.GetUnderlyingType(typeof(Tin)) ?? typeof(Tin);
            for (int i = 0; i < pinfos.Count; i++) {
                if (pinfos[i] != null)
                    converters[i] = GetNullableConverter(notNullableTin, pinfos[i].PropertyType);
            }
            Tout listToObj(IReadOnlyList<Tin> list)
            {
                Tout obj = new Tout();
                int count = Math.Min(list.Count, pinfos.Count);
                for (int i = 0; i < count; i++) {
                    if (pinfos[i] != null) {
                        object value = list[i];
                        if (value != null) {
                            value = converters[i](list[i]);
                        }
                        pinfos[i].SetValue(obj, value);
                    }
                }
                return obj;
            }
            return listToObj;
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts from one type to another.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <param name="output">The output <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts from one type to another.</returns>
        public static Func<object, object> GetConverter(Type input, Type output)
        {
            return GetConverter(output)(Nullable.GetUnderlyingType(input) ?? input);
        }

        /// <summary>
        /// Creates a converter that accepts null values. This should be used if the either the input
        /// or output types are <see cref="Nullable"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <param name="output">The output <see cref="Type"/>.</param>
        /// <returns>A function that converts objects from one type to another.</returns>
        public static Func<object, object> GetNullableConverter(Type input, Type output)
        {
            if (!output.IsNullable() || !input.IsNullable()) {
                return GetConverter(output)(input);
            }
            Func<object, object> converter = GetConverter(input, output);
            object nullableConverter(object value)
            {
                return value == null ? null : converter(value);
            }
            return nullableConverter;
        }

        public static Func<Type, Func<object, object>> GetConverter(Type output)
        {
            output = Nullable.GetUnderlyingType(output) ?? output;
            if (ConverterMap.TryGetValue(output, out Func<Type, Func<object, object>> converter))
                return converter;
            if (output.IsSubclassOf(typeof(IConvertible))) {
                return ToIConvertible;
            }
            return NonConverter;
        }

        public static Func<object, object> ToIConvertible(Type output)
        {
            object converter(object value)
            {
                return Converters.ChangeType(value, output);
            }
            return converter;
        }

        /// <summary>
        /// Returns an object of the specified type whose value is equivalent to the specified object.
        /// </summary>
        /// <param name="value">An object that implements the System.IConvertible interface.</param>
        /// <param name="typeCode">The type of object to return.</param>
        /// <returns>An object whose type is conversionType and whose value is equivalent to value.</returns>
        public static object ChangeType(object value, Type type)
        {
            if (value == null)
                return null;
            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode) {
                case TypeCode.Boolean:
                    return ToBoolean(value);
                case TypeCode.Char:
                    return Converters.ToChar(value);
                case TypeCode.SByte:
                    return Converters.ToSByte(value);
                case TypeCode.Byte:
                    return Converters.ToByte(value);
                case TypeCode.Int16:
                    return Converters.ToInt16(value);
                case TypeCode.UInt16:
                    return Converters.ToUInt16(value);
                case TypeCode.Int32:
                    return Converters.ToInt32(value);
                case TypeCode.UInt32:
                    return Converters.ToUInt32(value);
                case TypeCode.Int64:
                    return Converters.ToInt64(value);
                case TypeCode.UInt64:
                    return Converters.ToUInt64(value);
                case TypeCode.Single:
                    return Converters.ToSingle(value);
                case TypeCode.Double:
                    return Converters.ToDouble(value);
                case TypeCode.Decimal:
                    return Converters.ToDecimal(value);
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
                    else if (type == typeof(BigInteger))
                        return ObjectToBigInteger(value);
                    ////else if (value is IConvertible conv)
                    ////   return value;
                    return value;
                case TypeCode.DBNull:
                    if (value == DBNull.Value)
                        return DBNull.Value;
                    throw new InvalidCastException("Invalid cast: TypeCode.DBNull");
                //case TypeCode.Empty:
                //cannot get here
                //    throw new InvalidCastException("Invalid Cast: TypeCode.Empty");
                default:
                    throw new ArgumentException("Argument: unknown TypeCode " + typeCode.ToString());
            }
        }

        /// <summary>
        /// Maps input/output types to a <see cref="Func{T, TResult}"/>.
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
            { typeof(BigInteger), ToBigInteger }
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
        /// <returns>A <see cref="Func{T, TResult}"/> that does nothing.</returns>
        private static object NoConvert(object value) { return value; }

        /// <summary>
        /// A function that returns a <see cref="Func{T, TResult}"/> that does nothing.
        /// </summary>
        private static readonly Func<Type, Func<object, object>> NonConverter = (inp) => { return NoConvert; };

        #region ToString
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="string"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert.</param>
        /// <returns>The <see cref="string"/> representation of the <see cref="object"/>.</returns>
        public static object ToString(object value)
        {
            return ToString(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="string"/>.</returns>
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
        /// <returns>The <see cref="DateTime"/> representation of the <see cref="object"/> representation.</returns>
        public static object ToDateTime(object value)
        {
            return ToDateTime(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="DateTime"/>.</returns>
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
            if (value is string str)
                return StringToDateTimeOffset(str);
            if (value is DateTimeOffset dto)
                return value;
            if (value is DateTime dt)
                return DateTimeToDateTimeOffset(dt);
            if (value is TimeSpan ts)
                return TimeSpanToDateTimeOffset(ts);
            return System.Convert.ChangeType(value, typeof(DateTimeOffset));
        }
        #endregion // ToDateTime

        #region ToDateTimeOffset
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="DateTimeOffset"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>The <see cref="DateTimeOffset"/> representation of the <see cref="object"/>.</returns>
        public static object ToDateTimeOffset(object value)
        {
            return ToDateTimeOffset(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="DateTimeOffset"/>.</returns>
        public static Func<object, object> ToDateTimeOffset(Type input)
        {
            if (input == typeof(string))
                return StringToDateTimeOffset;
            if (input == typeof(DateTime))
                return DateTimeToDateTimeOffset;
            if (input == typeof(TimeSpan))
                return TimeSpanToDateTimeOffset;
            return ToDateTimeOffset;
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
        /// <returns>The <see cref="TimeSpan"/> representation of the <see cref="object"/>.</returns>
        public static object ToTimeSpan(object value)
        {
            if (value is string str)
                return TimeSpan.Parse(str);
            if (value is TimeSpan ts)
                return ts;
            if (value is DateTime dt)
                return dt.TimeOfDay;
            if (value is DateTimeOffset dto)
                return Extensions.ToDateTime((DateTimeOffset) value).TimeOfDay;
            return System.Convert.ChangeType(value, typeof(TimeSpan));
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="TimeSpan"/>.</returns>
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
                converter = ToTimeSpan;
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
        /// <returns>The <see cref="char"/>[] representation of the <see cref="object"/>.</returns>
        public static object ToChars(object value)
        {
            return ToChars(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="char"/>[].
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="char"/>[].</returns>
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

        #region ToBytes
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="byte"/>[] representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>The <see cref="byte"/>[] representation of the <see cref="object"/>.</returns>
        public static object ToBytes(object value)
        {
            return ToBytes(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="byte"/>[].
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="byte"/>[].</returns>
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
        /// <returns>The <see cref="short"/> representation of the <see cref="object"/>.</returns>
        public static object ToInt16(object value)
        {
            return System.Convert.ToInt16(value);
            ////return ToInt16(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="short"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="short"/>.</returns>
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
        /// <returns>The <see cref="int"/> representation of the <see cref="object"/>.</returns>
        public static object ToInt32(object value)
        {
            return ToInt32(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to an <see cref="int"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to an <see cref="int"/>.</returns>
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

        #region ToInt64
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="long"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>The <see cref="long"/> representation of the <see cref="object"/>.</returns>
        public static object ToInt64(object value)
        {
            return System.Convert.ToInt64(value);
            ////return ToInt64(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="long"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="long"/>.</returns>
        public static Func<object, object> ToInt64(Type input)
        {
            return ObjectToInt64;
        }

        private static object ObjectToInt64(object value)
        {
            return System.Convert.ToInt64(value);
        }
        #endregion // ToInt64

        #region ToUInt16
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="ushort"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>The <see cref="ushort"/> representation of the <see cref="object"/>.</returns>
        public static object ToUInt16(object value)
        {
            return System.Convert.ToUInt16(value);
            ////return ToUInt16(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="ushort"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="ushort"/>.</returns>
        public static Func<object, object> ToUInt16(Type input)
        {
            return ObjectToUInt16;
        }

        public static object ObjectToUInt16(object value)
        {
            return System.Convert.ToUInt16(value);
        }
        #endregion // ToUInt16

        #region ToUInt32
        /// <summary>
        /// Converts the value of the specified <see cref="object"/> to its equivalent <see cref="uint"/> representation.
        /// </summary>
        /// <param name="value">An <see cref="object"/> that supplies the value to convert, or null.</param>
        /// <returns>The <see cref="uint"/> representation of the <see cref="object"/>.</returns>
        public static object ToUInt32(object value)
        {
            return System.Convert.ToUInt32(value);
            ////return ToUInt32(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="uint"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="uint"/>.</returns>
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
        /// <returns>The <see cref="ulong"/> representation of the <see cref="object"/>.</returns>
        public static object ToUInt64(object value)
        {
            return System.Convert.ToUInt64(value);
            ////return ToUInt64(value.GetType())(value);
        }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="ulong"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="ulong"/>.</returns>
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
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="bool"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="bool"/>.</returns>
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
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="byte"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="byte"/>.</returns>
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
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to an <see cref="sbyte"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to an <see cref="sbyte"/>.</returns>
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
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="char"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="char"/>.</returns>
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
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="float"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="float"/>.</returns>
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
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="double"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Type"/>.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="double"/>.</returns>
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
        /// Gets a <see cref="Func{T, TResult}"/> that converts objects from a <see cref="Type"/> to a <see cref="decimal"/>.
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

        #region ToBigInteger
        public static object ObjectToBigInteger(object value)
        {
            Type type = value.GetType();
            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode) {
                case TypeCode.Boolean:
                    return new BigInteger((bool) value ? 1 : 0);
                case TypeCode.Byte:
                    return new BigInteger((byte) value);
                case TypeCode.Decimal:
                    return new BigInteger((decimal) value);
                case TypeCode.Double:
                    return new BigInteger((double) value);
                case TypeCode.Int16:
                    return new BigInteger((short) value);
                case TypeCode.Int32:
                    return new BigInteger((int) value);
                case TypeCode.Int64:
                    return new BigInteger((long) value);
                case TypeCode.SByte:
                    return new BigInteger((sbyte) value);
                case TypeCode.Single:
                    return new BigInteger((float) value);
                case TypeCode.String:
                    return BigInteger.Parse(value as string);
                case TypeCode.UInt16:
                    return new BigInteger((ushort) value);
                case TypeCode.UInt32:
                    return new BigInteger((uint) value);
                case TypeCode.UInt64:
                    return new BigInteger((ulong) value);
                case TypeCode.Object:
                    if (type == typeof(byte[]))
                        return new BigInteger(value as byte[]);
                    break;
            }
            throw new InvalidCastException("Cannot cast " + type.ToString() + " to System.Numerics.BigInteger");
        }

        public static Func<object, object> ToBigInteger(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode) {
                case TypeCode.Boolean:
                    return BooleanToBigInteger;
                case TypeCode.Byte:
                    return ByteToBigInteger;
                case TypeCode.Decimal:
                    return DecimalToBigInteger;
                case TypeCode.Double:
                    return DoubleToBigInteger;
                case TypeCode.Int16:
                    return Int16ToBigInteger;
                case TypeCode.Int32:
                    return Int32ToBigInteger;
                case TypeCode.Int64:
                    return Int64ToBigInteger;
                case TypeCode.SByte:
                    return SByteToBigInteger;
                case TypeCode.Single:
                    return SingleToBigInteger;
                case TypeCode.String:
                    return StringToBigInteger;
                case TypeCode.UInt16:
                    return UInt16ToBigInteger;
                case TypeCode.UInt32:
                    return UInt32ToBigInteger;
                case TypeCode.UInt64:
                    return UInt64ToBigInteger;
                case TypeCode.Object:
                    if (type == typeof(byte[]))
                        return ByteArrayToBigInteger;
                    break;
            }
            throw new InvalidCastException("Cannot cast " + type.ToString() + " to System.Numerics.BigInteger");
        }

        private static object BooleanToBigInteger(object value)
        {
            return new BigInteger((bool) value ? 1 : 0);
        }

        private static object ByteToBigInteger(object value)
        {
            return new BigInteger((byte) value);
        }

        private static object DecimalToBigInteger(object value)
        {
            return new BigInteger((decimal) value);
        }

        private static object DoubleToBigInteger(object value)
        {
            return new BigInteger((double) value);
        }

        private static object Int16ToBigInteger(object value)
        {
            return new BigInteger((short) value);
        }

        private static object Int32ToBigInteger(object value)
        {
            return new BigInteger((int) value);
        }

        private static object Int64ToBigInteger(object value)
        {
            return new BigInteger((long) value);
        }

        private static object SByteToBigInteger(object value)
        {
            return new BigInteger((sbyte) value);
        }

        private static object SingleToBigInteger(object value)
        {
            return new BigInteger((float) value);
        }

        private static object StringToBigInteger(object value)
        {
            return BigInteger.Parse(value as string);
        }

        private static object UInt16ToBigInteger(object value)
        {
            return new BigInteger((ushort) value);
        }

        private static object UInt32ToBigInteger(object value)
        {
            return new BigInteger((uint) value);
        }

        private static object UInt64ToBigInteger(object value)
        {
            return new BigInteger((ulong) value);
        }

        private static object ByteArrayToBigInteger(object value)
        {
            return new BigInteger((byte[]) value);
        }
        #endregion
    }
}
