using System;
using System.Collections;
using System.ComponentModel;
using System.Numerics;

namespace Utilities.Comparers
{
	public static class Comparers
	{
		public static IComparer GetComparer(Type type)
		{
			TypeCode typeCode = Type.GetTypeCode(type);
			switch (typeCode) {
				case TypeCode.Boolean:
					return System.Collections.Generic.Comparer<bool>.Default;
				case TypeCode.Byte:
					return System.Collections.Generic.Comparer<byte>.Default;
				case TypeCode.Char:
					return System.Collections.Generic.Comparer<char>.Default;
				case TypeCode.DateTime:
					return System.Collections.Generic.Comparer<DateTime>.Default;
				case TypeCode.Decimal:
					return System.Collections.Generic.Comparer<decimal>.Default;
				case TypeCode.Double:
					return System.Collections.Generic.Comparer<double>.Default;
				case TypeCode.Int16:
					return System.Collections.Generic.Comparer<short>.Default;
				case TypeCode.Int32:
					return System.Collections.Generic.Comparer<int>.Default;
				case TypeCode.Int64:
					return System.Collections.Generic.Comparer<long>.Default;
				case TypeCode.SByte:
					return System.Collections.Generic.Comparer<sbyte>.Default;
				case TypeCode.Single:
					return System.Collections.Generic.Comparer<float>.Default;
				case TypeCode.String:
					return System.Collections.Generic.Comparer<string>.Default;
				case TypeCode.UInt16:
					return System.Collections.Generic.Comparer<ushort>.Default;
				case TypeCode.UInt32:
					return System.Collections.Generic.Comparer<uint>.Default;
				case TypeCode.UInt64:
					return System.Collections.Generic.Comparer<ulong>.Default;
				case TypeCode.Object:
					if(type == typeof(DateTimeOffset))
						return System.Collections.Generic.Comparer<DateTimeOffset>.Default;
					if(type == typeof(TimeSpan))
						return System.Collections.Generic.Comparer<TimeSpan>.Default;
					if(type == typeof(BigInteger))
						return System.Collections.Generic.Comparer<BigInteger>.Default;
					if(type == typeof(Guid))
						return System.Collections.Generic.Comparer<Guid>.Default;
					goto default;
				default:
					return System.Collections.Generic.Comparer<object>.Default;
			}
		}
	}
}
