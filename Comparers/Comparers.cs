using System;
using System.Collections;
using System.ComponentModel;
using System.Numerics;

namespace Utilities.Comparers
{
    public static class Comparers
    {
        public static readonly IComparer Boolean = new Comparer<bool>();
        public static readonly IComparer Byte = new Comparer<byte>();
        public static readonly IComparer Char = new Comparer<char>();
        public static readonly IComparer DateTime = new Comparer<DateTime>();
        public static readonly IComparer Decimal = new Comparer<decimal>();
        public static readonly IComparer Double = new Comparer<double>();
        public static readonly IComparer Short = new Comparer<short>();
        public static readonly IComparer Int64 = new Comparer<long>();
        public static readonly IComparer Int32 = new Comparer<int>();
        public static readonly IComparer Int16 = new Comparer<short>();
        public static readonly IComparer SByte = new Comparer<sbyte>();
        public static readonly IComparer Single = new Comparer<float>();
        public static readonly IComparer String = new NullableComparer<string>();
        public static readonly IComparer UInt64 = new Comparer<ulong>();
        public static readonly IComparer UInt32 = new Comparer<uint>();
        public static readonly IComparer UInt16 = new Comparer<ushort>();
        public static readonly IComparer DateTimeOffset = new Comparer<DateTimeOffset>();
        public static readonly IComparer Guid = new Comparer<Guid>();
        public static readonly IComparer TimeSpan = new Comparer<TimeSpan>();
        public static readonly IComparer BigInteger = new Comparer<BigInteger>();
        public static readonly IComparer Object = new NullableComparer<IComparable>();

        public static bool CanCompare(Type t)
        {
            return t.IsSubclassOf(typeof(IComparable));
        }

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
                case TypeCode.Object:
                    return Object;
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
            }
            throw new InvalidOperationException("Invalid TypeCode: " + typeCode.ToString());
        }

        public static int Compare(object x, object y)
        {
            if (x == null || x == DBNull.Value) {
                if (y == null || y == DBNull.Value)
                    return 0;
                return -1;
            }
            else if (y == null || y == DBNull.Value)
                return 1;
            if (x is IComparable xCmp) {
                Type xTy = x.GetType();
                Type yTy = y.GetType();
                if (TypeDescriptor.GetConverter(xTy).CanConvertTo(yTy)) {
                    y = Converters.Converters.ChangeType(y, xTy);
                    return xCmp.CompareTo(y);
                }
                else if (TypeDescriptor.GetConverter(yTy).CanConvertTo(xTy)) {
                    x = Converters.Converters.ChangeType(x, yTy);
                    return (x as IComparable).CompareTo(y);
                }
            }
            return 0;
        }

        public static int Compare(bool x, bool y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(string x, string y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(DateTime x, DateTime y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(long x, long y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(int x, int y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(short x, short y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(ulong x, ulong y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(uint x, uint y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(ushort x, ushort y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(decimal x, decimal y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(double x, double y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(float x, float y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(byte x, byte y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(sbyte x, sbyte y)
        {
            return x.CompareTo(y);
        }

        public static int Compare(char x, char y)
        {
            return x.CompareTo(y);
        }

        public static int Compare<T>(T x, T y) where T : IComparable<T>
        {
            return x.CompareTo(y);
        }
    }
}
