using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Utilities
{
    /// <summary>
    /// DataRow IEqualityComparer.
    /// </summary>
    public class DataRowEqualityComparer : IEqualityComparer<DataRow>
    {
        public static DataRowEqualityComparer Default { get; } = new DataRowEqualityComparer();

        public static DataRowEqualityComparer Create(DataTable table, params string[] columnNames)
        {
            if (columnNames.Length == 0)
                return Default;
            int[] colIndexes = table.Columns.Cast<DataColumn>().AsEnumerable().Where(col => columnNames.Contains(col.ColumnName)).Select(col => col.Ordinal).ToArray();
            if (colIndexes.Length == table.Columns.Count)
                return Default;
            return new DataRowEqualityComparerAll(colIndexes);
        }

        public static DataRowEqualityComparer Create(params int[] columnIndexes)
        {
            return new DataRowEqualityComparerAll(columnIndexes);
        }

        int IEqualityComparer<DataRow>.GetHashCode(DataRow obj)
        {
            return GetHashCode(obj);
        }

        public static int GetHashCode(DataRow obj)
        {
            int hashcode = 0;
            for (int i = 0; i < obj.ItemArray.Length; i++) {
                hashcode ^= obj.ItemArray[i].GetHashCode();
            }
            return hashcode;
        }

        bool IEqualityComparer<DataRow>.Equals(DataRow x, DataRow y)
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

        private class DataRowEqualityComparerAll : DataRowEqualityComparer, IEqualityComparer<DataRow>
        {
            private readonly int[] columnIndexes;
            public DataRowEqualityComparerAll(params int[] columnIndexes)
            {
                this.columnIndexes = columnIndexes;
            }

            int IEqualityComparer<DataRow>.GetHashCode(DataRow obj)
            {
                int hashcode = 0;
                for (int i = 0; i < columnIndexes.Length; i++) {
                    hashcode ^= obj[columnIndexes[i]].GetHashCode();
                }
                return hashcode;
            }

            bool IEqualityComparer<DataRow>.Equals(DataRow x, DataRow y)
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