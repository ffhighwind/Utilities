using System;
using System.Collections;

namespace Utilities.Comparers
{
    public class NullableComparer<Ty> : IComparer where Ty : class, IComparable
    {
        int IComparer.Compare(object x, object y)
        {
            if (x == null) {
                if (y == null)
                    return 0;
                return -1;
            }
            else if (y == null)
                return 1;
            return ((Ty) x).CompareTo((Ty) y);
        }
    }
}
