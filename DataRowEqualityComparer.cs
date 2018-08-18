using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Utilities
{
    public class DataRowEqualityComparer<T> : IEqualityComparer<T> where T : DataRow
    {
        public static DataRowEqualityComparer<T> Default { get; } = new DataRowEqualityComparer<T>();

        public static DataRowEqualityComparer<T> Create(DataTable table, params string[] columnNames)
        {
            if (columnNames.Length == 0)
                return Default;
            int[] colIndexes = table.Columns.Cast<DataColumn>().AsEnumerable().Where(col => columnNames.Contains(col.ColumnName)).Select(col => col.Ordinal).ToArray();
            if (colIndexes.Length == table.Columns.Count)
                return Default;
            return new DataRowEqualityComparerAll(colIndexes);
        }

        public static DataRowEqualityComparer<T> Create(params int[] columnIndexes)
        {
            return new DataRowEqualityComparerAll(columnIndexes);
        }

        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            return GetHashCode(obj);
        }

        public static int GetHashCode(T obj)
        {
            int hashcode = 0;
            for (int i = 0; i < obj.ItemArray.Length; i++) {
                hashcode ^= obj.ItemArray[i].GetHashCode();
            }
            return hashcode;
        }

        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            bool result = x.ItemArray.Length == y.ItemArray.Length;
            if (result) {
                for (int i = 0; i < x.ItemArray.Length; i++) {
                    result = x.ItemArray[i].Equals(y.ItemArray[i]);
                    if (!result)
                        break;
                }
            }
            return result;
        }

        private class DataRowEqualityComparerAll : DataRowEqualityComparer<T>, IEqualityComparer<T>
        {
            private readonly int[] columnIndexes;
            public DataRowEqualityComparerAll(params int[] columnIndexes)
            {
                this.columnIndexes = columnIndexes;
            }

            int IEqualityComparer<T>.GetHashCode(T obj)
            {
                int hashcode = 0;
                for (int i = 0; i < columnIndexes.Length; i++) {
                    hashcode ^= obj[columnIndexes[i]].GetHashCode();
                }
                return hashcode;
            }

            bool IEqualityComparer<T>.Equals(T x, T y)
            {
                bool result = x.ItemArray.Length == y.ItemArray.Length;
                if (result) {
                    for (int i = 0; i < columnIndexes.Length; i++) {
                        result = x.ItemArray[columnIndexes[i]].Equals(y.ItemArray[columnIndexes[i]]);
                        if (!result)
                            break;
                    }
                }
                return result;
            }
        }
    }
}