using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Utilities
{
    public class DataRowEqualityComparer<T> : IEqualityComparer<T> where T : DataRow
    {
        private static DataRowEqualityComparer<T> _default = new DataRowEqualityComparer<T>();

        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            return GetHashCode(obj);
        }

        public static int GetHashCode(T obj)
        {
            int hashcode = obj.ItemArray.Length == 0 ? 1 : obj[0].GetHashCode();
            for (int i = 0; i < obj.ItemArray.Length; i++) {
                hashcode ^= obj[i].GetHashCode();
            }
            return hashcode;
        }

        bool IEqualityComparer<T>.Equals(T row1, T row2)
        {
            bool result = row1 == row2;
            if (!result) {
                result = row1.ItemArray.Length == row2.ItemArray.Length;
                if (result) {
                    for (int i = 0; i < row1.ItemArray.Length; i++) {
                        result = row1[i].Equals(row2[i]);
                        if (!result)
                            break;
                    }
                }
            }
            return result;
        }

        public static DataRowEqualityComparer<T> Default {
            get {
                return _default;
            }
        }

        public static DataRowEqualityComparer<T> Create(params int[] cols)
        {
            if (cols.Length == 0)
                return _default;
            return new DataRowEqualityComparerAll(cols);
        }

        private class DataRowEqualityComparerAll : DataRowEqualityComparer<T>, IEqualityComparer<T>
        {
            private int[] cols;
            public DataRowEqualityComparerAll(params int[] cols)
            {
                this.cols = cols;
            }

            int IEqualityComparer<T>.GetHashCode(T obj)
            {
                int hashcode = obj[cols[0]].GetHashCode();
                for (int i = 1; i < cols.Length; i++) {
                    hashcode ^= obj[cols[i]].GetHashCode();
                }
                return hashcode;
            }

            bool IEqualityComparer<T>.Equals(T row1, T row2)
            {
                bool result = row1 == row2;
                if (!result) {
                    result = row1.ItemArray.Length == row2.ItemArray.Length;
                    if (result) {
                        for (int i = 0; i < cols.Length; i++) {
                            result = row1[cols[i]].Equals(row2[cols[i]]);
                            if (!result)
                                break;
                        }
                    }
                }
                return result;
            }
        }
    }
    /*
    public class DataRowComparer<T> : IComparer<T> where T : DataRow
    {
        private bool isdesc;
        private int[] cols;
        private static readonly DataRowComparer<T> ascending = new DataRowComparer<T>(false);
        private static readonly DataRowComparer<T> descending = new DataRowComparer<T>(true);

        public DataRowComparer(bool descending, params int[] cols)
        {
            this.isdesc = descending;
            this.cols = cols;
        }

        private int CompareAllColumns(T row1, T row2)
        {
            int result = 0;
            object obj1, obj2;
            for (int i = 0; i < row1.ItemArray.Length; i++) {
                obj1 = row1[i];
                obj2 = row2[i];
                result = DataRowEqualityComparer<T>.GetHashCode(row1) - DataRowEqualityComparer<T>.GetHashCode(row2);
                if (result != 0)
                    break;
                result = obj1.GetType().GUID.CompareTo(obj2.GetType().GUID);
                if (result != 0)
                    break;
            }
            return isdesc ? -result : result;
        }

        private int CompareSomeColumns(T row1, T row2)
        {
            int result = 0;
            object obj1, obj2;
            for (int i = 0; i < cols.Length; i++) {
                obj1 = row1[cols[i]];
                obj2 = row2[cols[i]];
                result = DataRowEqualityComparer<T>.GetHashCode(row1) - DataRowEqualityComparer<T>.GetHashCode(row2);
                if (result != 0)
                    break;
                result = obj1.GetType().GUID.CompareTo(obj2.GetType().GUID);
                if (result != 0)
                    break;
            }
            return isdesc ? -result : result;
        }

        public int Compare(T row1, T row2)
        {
            int result;
            if (row1 == row2)
                result = 0;
            else {
                result = row1.ItemArray.Length - row2.ItemArray.Length;
                if (result == 0) {
                    result = cols.Length == 0 ? CompareAllColumns(row1, row2) : CompareSomeColumns(row1, row2);
                }
            }
            return result;
        }

        public static DataRowComparer<T> Ascending {
            get {
                return ascending;
            }
        }

        public static DataRowComparer<T> Descending {
            get {
                return descending;
            }
        }
    }
    */
}