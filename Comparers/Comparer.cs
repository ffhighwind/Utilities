using System;
using System.Collections;

namespace Utilities.Comparers
{
	/// <summary>
	/// Returns a default <see cref="IComparable"/>.
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> to compare.</typeparam>
	public class Comparer<T> : IComparer where T : struct, IComparable
	{
		int IComparer.Compare(object x, object y)
		{
			return ((T)x).CompareTo((T)y);
		}
	}
}