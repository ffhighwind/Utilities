using System;
using System.Collections;
using System.Numerics;

namespace Utilities.Comparers
{
    public static class ParseComparer
    {
        public static readonly IComparer Boolean = new BooleanParseComparer();
        public static readonly IComparer Byte = new ByteParseComparer();
        public static readonly IComparer Char = new CharParseComparer();
        public static readonly IComparer DateTime = new DateTimeParseComparer();
        public static readonly IComparer Decimal = new DecimalParseComparer();
        public static readonly IComparer Double = new DoubleParseComparer();
        public static readonly IComparer Int16 = new Int16ParseComparer();
        public static readonly IComparer Int32 = new Int32ParseComparer();
        public static readonly IComparer Int64 = new Int64ParseComparer();
        public static readonly IComparer SByte = new SByteParseComparer();
        public static readonly IComparer Single = new SingleParseComparer();
        public static readonly IComparer String = new StringParseComparer();
        public static readonly IComparer UInt16 = new UInt16ParseComparer();
        public static readonly IComparer UInt32 = new UInt32ParseComparer();
        public static readonly IComparer UInt64 = new UInt64ParseComparer();
        public static readonly IComparer TimeSpan = new TimeSpanParseComparer();
        public static readonly IComparer DateTimeOffset = new DateTimeOffsetParseComparer();
        public static readonly IComparer Guid = new GuidParseComparer();
        public static readonly IComparer BigInteger = new BigIntegerParseComparer();

        public static IComparer GetComparer(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode) {
                case TypeCode.Boolean:
                    return Boolean;
                case TypeCode.Byte:
                    return Byte;
                case TypeCode.Char:
                    return Char;
                case TypeCode.DateTime:
                    return DateTime;
                case TypeCode.Decimal:
                    return Decimal;
                case TypeCode.Double:
                    return Double;
                case TypeCode.Int16:
                    return Int16;
                case TypeCode.Int32:
                    return Int32;
                case TypeCode.Int64:
                    return Int64;
                case TypeCode.SByte:
                    return SByte;
                case TypeCode.Single:
                    return Single;
                case TypeCode.String:
                    return String;
                case TypeCode.UInt16:
                    return UInt16;
                case TypeCode.UInt32:
                    return UInt32;
                case TypeCode.UInt64:
                    return UInt64;
                case TypeCode.Object:
                    if (type == typeof(TimeSpan))
                        return TimeSpan;
                    else if (type == typeof(DateTimeOffset))
                        return DateTimeOffset;
                    else if (type == typeof(Guid))
                        return Guid;
                    else if (type == typeof(BigInteger))
                        return BigInteger;
                    break;
            }
            throw new InvalidOperationException("Invalid TypeCode: " + typeCode.ToString());
        }

        private class StringParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return ((string) x).CompareTo((string) y);
            }
        }

        private class BooleanParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                string xStr = x as string;
                string yStr = y as string;
                bool xVal, yVal;
                if (xStr.Equals("true", StringComparison.OrdinalIgnoreCase))
                    xVal = true;
                else if (xStr.Equals("false", StringComparison.OrdinalIgnoreCase))
                    xVal = false;
                else
                    throw new InvalidCastException();
                if (yStr.Equals("true", StringComparison.OrdinalIgnoreCase))
                    yVal = true;
                else if (yStr.Equals("false", StringComparison.OrdinalIgnoreCase))
                    yVal = false;
                else
                    throw new InvalidCastException();

                return xVal.CompareTo(yVal);
            }
        }

        private class ByteParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return byte.Parse(x as string).CompareTo(byte.Parse(y as string));
            }
        }

        private class CharParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return char.Parse(x as string).CompareTo(char.Parse(y as string));
            }
        }

        private class DateTimeParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return System.DateTime.Parse(x as string).CompareTo(System.DateTime.Parse(y as string));
            }
        }

        private class DecimalParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return decimal.Parse(x as string).CompareTo(decimal.Parse(y as string));
            }
        }

        private class DoubleParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return double.Parse(x as string).CompareTo(double.Parse(y as string));
            }
        }

        private class Int16ParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return short.Parse(x as string).CompareTo(short.Parse(y as string));
            }
        }

        private class Int32ParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return int.Parse(x as string).CompareTo(int.Parse(y as string));
            }
        }

        private class Int64ParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return long.Parse(x as string).CompareTo(long.Parse(y as string));
            }
        }

        private class SByteParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return sbyte.Parse(x as string).CompareTo(sbyte.Parse(y as string));
            }
        }

        private class SingleParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return float.Parse(x as string).CompareTo(float.Parse(y as string));
            }
        }

        private class UInt16ParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return ushort.Parse(x as string).CompareTo(ushort.Parse(y as string));
            }
        }

        private class UInt32ParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return uint.Parse(x as string).CompareTo(uint.Parse(y as string));
            }
        }

        private class UInt64ParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return ulong.Parse(x as string).CompareTo(ulong.Parse(y as string));
            }
        }

        private class DateTimeOffsetParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return System.DateTimeOffset.Parse(x as string).CompareTo(System.DateTimeOffset.Parse(y as string));
            }
        }

        private class TimeSpanParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return System.TimeSpan.Parse(x as string).CompareTo(System.TimeSpan.Parse(y as string));
            }
        }

        private class BigIntegerParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return System.Numerics.BigInteger.Parse(x as string).CompareTo(System.Numerics.BigInteger.Parse(y as string));
            }
        }

        private class GuidParseComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return System.Guid.Parse(x as string).CompareTo(System.Guid.Parse(y as string));
            }
        }
    }
}
