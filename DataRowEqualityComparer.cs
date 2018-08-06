using System;
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

        private class DataRowEqualityComparerAll : DataRowEqualityComparer<T>, IEqualityComparer<T>
        {
            private int[] columnIndexes;
            public DataRowEqualityComparerAll(params int[] columnIndexes)
            {
                this.columnIndexes = columnIndexes;
            }

            int IEqualityComparer<T>.GetHashCode(T obj)
            {
                int hashcode = obj[columnIndexes[0]].GetHashCode();
                for (int i = 1; i < columnIndexes.Length; i++) {
                    hashcode ^= obj[columnIndexes[i]].GetHashCode();
                }
                return hashcode;
            }

            bool IEqualityComparer<T>.Equals(T row1, T row2)
            {
                bool result = row1 == row2;
                if (!result) {
                    result = row1.ItemArray.Length == row2.ItemArray.Length;
                    if (result) {
                        for (int i = 0; i < columnIndexes.Length; i++) {
                            result = row1[columnIndexes[i]].Equals(row2[columnIndexes[i]]);
                            if (!result)
                                break;
                        }
                    }
                }
                return result;
            }
        }
    }
}