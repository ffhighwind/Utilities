using System;
using System.Collections;

namespace Utilities.Comparers
{
    public class Comparer<Ty> : IComparer where Ty : struct, IComparable
    {
        int IComparer.Compare(object x, object y)
        {
            return ((Ty) x).CompareTo((Ty) y);
        }
    }
}