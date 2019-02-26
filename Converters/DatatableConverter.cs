using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Utilities.Converters
{
	public class DataTableConverter<T>
	{
		private Action<object>[] converters;
		private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		public DataTableConverter(DataRow row)
		{
			Load(row.ItemArray.Select(item => item.GetType()).ToList(), typeof(T).GetProperties(DefaultBindingFlags));
		}

		public DataTableConverter(DataTable table, bool matchNames = true)
		{
			PropertyInfo[] props = typeof(T).GetProperties(DefaultBindingFlags);
			Type[] types = new Type[table.Columns.Count];
			if (matchNames) {
				PropertyInfo[] properties = new PropertyInfo[table.Columns.Count];
				for (int i = 0; i < table.Columns.Count; i++) {
					properties[i] = props.FirstOrDefault(prop => prop.Name == table.Columns[i].ColumnName);
					types[i] = table.Columns[i].DataType;
				}
				props = properties;
			}
			Load(types, props);
		}

		private void Load(IReadOnlyList<Type> input, PropertyInfo[] output)
		{
			converters = new Action<object>[Math.Min(input.Count, output.Length)];
			for (int i = 0; i < converters.Length; i++) {
				if (output[i] != null) {
					Func<object, object> converter = Utilities.Converters.Converters.GetConverter(input[i], output[i].PropertyType);
					converters[i] = (inp) => { output[i]?.SetValue(inp, inp == DBNull.Value ? null : converter(inp)); };
				}
			}
		}

		public IEnumerable<T> Convert(DataTable table)
		{
			foreach (DataRow row in table.Rows) {
				yield return Convert(row);
			}
		}

		public T Convert(DataRow row)
		{
			T obj = (T) System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
			for (int i = 0; i < converters.Length; i++) {
				converters[i]?.Invoke(obj);
			}
			return obj;
		}
	}
}
