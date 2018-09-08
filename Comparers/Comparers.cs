using System;
using System.Collections;

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
        public static readonly IComparer String = new Comparer<string>();
        public static readonly IComparer UInt64 = new Comparer<ulong>();
        public static readonly IComparer UInt32 = new Comparer<uint>();
        public static readonly IComparer UInt16 = new Comparer<ushort>();
        public static readonly IComparer DateTimeOffset = new Comparer<DateTimeOffset>();
        public static readonly IComparer Guid = new Comparer<Guid>();
        public static readonly IComparer TimeSpan = new Comparer<TimeSpan>();
        public static readonly IComparer BigInteger = new Comparer<System.Numerics.BigInteger>();
        public static readonly IComparer Object = new Comparer<IComparable>();
    }
}
