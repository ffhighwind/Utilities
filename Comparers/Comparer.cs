using System;
using System.Collections;

namespace Utilities.Comparers
{
	public class Comparer<T> : IComparer where T : struct, IComparable
	{
		int IComparer.Compare(object x, object y)
		{
			return ((T)x).CompareTo((T)y);
		}
	}
}