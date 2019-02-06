using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace Dapper.Extension
{
	public static class TableData<T> where T : class
	{

		static TableData()
		{
			TableAttribute = typeof(T).GetCustomAttribute<TableAttribute>(false);
			TableName = TableAttribute?.Name.Replace("'", "") ?? typeof(T).Name;
			Queries = new TableDataImpl<T>();
		}

		public static TableAttribute TableAttribute { get; private set; }
		public static ITableData<T> Queries { get; set; }
		public static string TableName { get; private set; }
		public static T CreateObject(object key)
		{
			return Queries.CreateObject(key);
		}
		public static void CopyKey(object key, T obj)
		{
			Queries.CopyKey(key, obj);
		}

		public static PropertyInfo[] Properties => Queries.Properties;
		public static PropertyInfo[] KeyProperties => Queries.KeyProperties;
		public static PropertyInfo[] AutoKeyProperties => Queries.AutoKeyProperties;
		public static PropertyInfo[] SelectProperties => Queries.SelectProperties;
		public static PropertyInfo[] UpdateProperties => Queries.UpdateProperties;
		public static PropertyInfo[] InsertProperties => Queries.InsertProperties;
		internal static PropertyInfo[] CompareProperties => Queries.CompareProperties;

		public static string[] Columns => Queries.Columns;
		public static string[] KeyColumns => Queries.KeyColumns;
	}
}
