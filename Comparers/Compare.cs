using System;
using System.ComponentModel;

namespace Utilities.Comparers
{
    public static class Compare
    {
        public static int To(object x, object y)
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
                if (TypeDescriptor.GetConverter(yTy).CanConvertTo(xTy)) {
                    x = Converters.Converters.ChangeType(x, yTy);
                    return (x as IComparable).CompareTo(y);
                }
            }
            return 0;
        }

        public static int To(bool x, bool y)
        {
            return x.CompareTo(y);
        }

        public static int To(string x, string y)
        {
            return x.CompareTo(y);
        }

        public static int To(DateTime x, DateTime y)
        {
            return x.CompareTo(y);
        }

        public static int To(long x, long y)
        {
            return x.CompareTo(y);
        }

        public static int To(int x, int y)
        {
            return x.CompareTo(y);
        }

        public static int To(short x, short y)
        {
            return x.CompareTo(y);
        }

        public static int To(ulong x, ulong y)
        {
            return x.CompareTo(y);
        }

        public static int To(uint x, uint y)
        {
            return x.CompareTo(y);
        }

        public static int To(ushort x, ushort y)
        {
            return x.CompareTo(y);
        }

        public static int To(decimal x, decimal y)
        {
            return x.CompareTo(y);
        }

        public static int To(double x, double y)
        {
            return x.CompareTo(y);
        }

        public static int To(float x, float y)
        {
            return x.CompareTo(y);
        }

        public static int To(byte x, byte y)
        {
            return x.CompareTo(y);
        }

        public static int To(sbyte x, sbyte y)
        {
            return x.CompareTo(y);
        }

        public static int To(char x, char y)
        {
            return x.CompareTo(y);
        }

        public static int To<Ty>(Ty x, Ty y) where Ty : IComparable<Ty>
        {
            return x.CompareTo(y);
        }
    }
}
