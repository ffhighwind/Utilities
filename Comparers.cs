using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Comparers
    {
        public static Func<object, object, int> GetParseComparer(Type type)
        {
            return GetParseComparer(Type.GetTypeCode(type));
        }

        public static Func<object, object, int> GetParseComparer(TypeCode typeCode)
        {
            switch (typeCode) {
                case TypeCode.String:
            }
            return null;
        }

        public static Func<object, object, int> StringToString(object x, object y)
        {
            return (x, y) => ((string) x).CompareTo((string) y);
        }

        public static int CompareTo(object x, object y)
        {
            if (x == null || x == DBNull.Value) {
                if (y == null || y == DBNull.Value)
                    return 0;
                else
                    return -1;
            }
            else if (y == null || y == DBNull.Value)
                return 1;
            TypeCode typeCode = Type.GetTypeCode(x.GetType());
            if (typeCode == TypeCode.Object) {
                IComparable xCmp = x as IComparable;
                IComparable yCmp = y as IComparable;
                if (xCmp == null || yCmp == null)
                    return 0;
                return xCmp.CompareTo(yCmp);
            }
            return GetComparer(typeCode)(x, y);
        }

        public static Func<object, object, int> GetComparer(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);
            if (typeCode == TypeCode.Object) {
                int comparer(object x, object y)
                {
                    return ((IComparable) x).CompareTo(y as IComparable);
                }
                return comparer;
            }
            return GetComparer(typeCode);
        }

        public static Func<object, object, int> GetComparer<Ty>() where Ty : IComparable<Ty>
        {
            TypeCode typeCode = Type.GetTypeCode(typeof(Ty));
            if (typeCode == TypeCode.Object) {
                int comparer(object x, object y)
                {
                    return ((Ty) x).CompareTo((Ty) y);
                }
                return comparer;
            }
            return GetComparer(typeCode);
        }

        public static Func<object, object, int> GetComparer(TypeCode typeCode)
        {
            Func<object, object, int> result;
            switch (typeCode) {
                case TypeCode.Boolean:
                    result = Boolean;
                    break;
                case TypeCode.Byte:
                    result = Byte;
                    break;
                case TypeCode.Char:
                    result = Char;
                    break;
                case TypeCode.DateTime:
                    result = DateTime;
                    break;
                case TypeCode.Decimal:
                    result = Decimal;
                    break;
                case TypeCode.Double:
                    result = Double;
                    break;
                case TypeCode.Int16:
                    result = Int16;
                    break;
                case TypeCode.Int32:
                    result = Int32;
                    break;
                case TypeCode.Int64:
                    result = Int64;
                    break;
                case TypeCode.SByte:
                    result = SByte;
                    break;
                case TypeCode.Single:
                    result = Single;
                    break;
                case TypeCode.String:
                    result = String;
                    break;
                case TypeCode.UInt16:
                    result = UInt16;
                    break;
                case TypeCode.UInt32:
                    result = UInt32;
                    break;
                case TypeCode.UInt64:
                    result = UInt64;
                    break;
                default:
                    throw new InvalidOperationException();
                    ////case TypeCode.Object:
                    ////case TypeCode.Empty:
                    ////case TypeCode.DBNull:
            }
            return result;
        }

        public static readonly Func<object, object, int> Boolean = CompareBoolean;
        public static readonly Func<object, object, int> String = CompareString;
        public static readonly Func<object, object, int> DateTime = CompareDateTime;

        public static readonly Func<object, object, int> Int64 = CompareInt64;
        public static readonly Func<object, object, int> Int32 = CompareInt32;
        public static readonly Func<object, object, int> Int16 = CompareInt16;

        public static readonly Func<object, object, int> UInt64 = CompareUInt64;
        public static readonly Func<object, object, int> UInt32 = CompareUInt32;
        public static readonly Func<object, object, int> UInt16 = CompareUInt16;

        public static readonly Func<object, object, int> Single = CompareSingle;
        public static readonly Func<object, object, int> Double = CompareDouble;
        public static readonly Func<object, object, int> Decimal = CompareDecimal;

        public static readonly Func<object, object, int> Byte = CompareByte;
        public static readonly Func<object, object, int> SByte = CompareSByte;
        public static readonly Func<object, object, int> Char = CompareChar;

        private static int CompareSingle(object x, object y)
        {
            return ((float) x).CompareTo((float) y);
        }

        private static int CompareDouble(object x, object y)
        {
            return ((double) x).CompareTo((double) y);
        }

        private static int CompareDecimal(object x, object y)
        {
            return ((decimal) x).CompareTo((decimal) y);
        }

        private static int CompareByte(object x, object y)
        {
            return ((byte) x).CompareTo((byte) y);
        }

        private static int CompareSByte(object x, object y)
        {
            return ((sbyte) x).CompareTo((sbyte) y);
        }

        private static int CompareChar(object x, object y)
        {
            return ((char) x).CompareTo((char) y);
        }

        private static int CompareString(object x, object y)
        {
            return ((string) x).CompareTo((string) y);
        }

        private static int CompareDateTime(object x, object y)
        {
            return ((DateTime) x).CompareTo((DateTime) y);
        }

        private static int CompareInt64(object x, object y)
        {
            return ((long) x).CompareTo((long) y);
        }

        private static int CompareInt32(object x, object y)
        {
            return ((int) x).CompareTo((int) y);
        }

        private static int CompareInt16(object x, object y)
        {
            return ((short) x).CompareTo((short) y);
        }

        private static int CompareUInt64(object x, object y)
        {
            return ((ulong) x).CompareTo((ulong) y);
        }

        private static int CompareUInt32(object x, object y)
        {
            return ((uint) x).CompareTo((uint) y);
        }

        private static int CompareUInt16(object x, object y)
        {
            return ((ushort) x).CompareTo((ushort) y);
        }

        private static int CompareBoolean(object x, object y)
        {
            return ((bool) x).CompareTo((bool) y);
        }
    }
}
