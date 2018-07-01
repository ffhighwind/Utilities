using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class DataRowConverter<T> where T : new()
    {
        protected Func<object, object>[] converters;
        protected PropertyInfo[] pinfos;
        protected static readonly BindingFlags flags = BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Instance;

        private DataRowConverter() { }

        private DataRowConverter(DataRow row, PropertyInfo[] pinfos)
        {
            this.pinfos = pinfos;
            converters = new Func<object, object>[row.ItemArray.Length];
            for (int i = 0; i < row.ItemArray.Length; i++) {
                var conv = Util.Converter(row.ItemArray[i].GetType(), pinfos[i].PropertyType);
                converters[i] = (inp) => { return inp == DBNull.Value ? null : conv(inp); };
            }
        }

        private DataRowConverter(DataTable table, PropertyInfo[] pinfos)
        {
            this.pinfos = pinfos;
            converters = new Func<object, object>[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++) {
                converters[i] = Util.Converter(table.Columns[i].DataType, pinfos[i].PropertyType);
            }
        }

        public static DataRowConverter<T> Create(DataRow row)
        {
            PropertyInfo[] pinfos = typeof(T).GetProperties(flags);
            return new DataRowConverter<T>(row, pinfos);
        }

        public static DataRowConverter<T> Create(DataTable table, bool matchnames = true)
        {
            PropertyInfo[] pinfos = typeof(T).GetProperties(flags);
            if (matchnames && pinfos.Length == table.Columns.Count) {
                for (int col = 0; col < table.Columns.Count; col++) {
                    if(pinfos[col].Name != table.Columns[col].ColumnName) {
                        return new DataRowNameConverter(table, pinfos);
                    }
                }
            }
            return new DataRowConverter<T>(table, pinfos);
        }

        public IEnumerable<T> Convert(DataTable table)
        {
            foreach (DataRow row in table.Rows) {
                yield return Convert(row);
            }
        }

        public IEnumerable<T> Convert(IEnumerable<DataRow> rows)
        {
            foreach (DataRow row in rows) {
                yield return Convert(row);
            }
        }

        public virtual T Convert(DataRow row)
        {
            T obj = new T();
            for(int i = 0; i < converters.Length; i++) {
                pinfos[i].SetValue(obj, converters[i](row[i]));
            }
            return obj;
        }

        private class DataRowNameConverter : DataRowConverter<T>
        {
            private int[] drIndexes;

            public DataRowNameConverter(DataTable table, PropertyInfo[] pinfos)
            {
                List<DataColumn> columns = table.Columns.Cast<DataColumn>().AsEnumerable().ToList();
                IEnumerable<string> columnNames = columns.Select(col => col.ColumnName);
                this.pinfos = pinfos.Where(prop => columnNames.Contains(prop.Name)).ToArray();
                converters = new Func<object, object>[pinfos.Length];
                drIndexes = new int[pinfos.Length];
                for (int i = 0; i < pinfos.Length; i++) {
                    for(int col = 0; col < columns.Count; col++) {
                        if(columns[col].ColumnName == this.pinfos[i].Name) {
                            drIndexes[i] = columns[col].Ordinal;
                            converters[i] = Util.Converter(this.pinfos[i].PropertyType, columns[drIndexes[i]].GetType());
                            break;
                        }
                    }
                }
            }

            public override T Convert(DataRow row)
            {
                T obj = new T();
                for (int i = 0; i < converters.Length; i++) {
                    pinfos[i].SetValue(obj, converters[i](row[drIndexes[i]]));
                }
                return obj;
            }
        }
    }
}
