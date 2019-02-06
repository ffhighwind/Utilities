using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Dynamic;

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

		public static void SetKey(T obj, object key)
		{
			Queries.SetKey(obj, key);
		}

		public static object MakeKey<KeyType>(KeyType value)
		{
			return Queries.MakeKey<KeyType>(value);
		}

		public static Tout GetKey<Tout>(T obj)
		{
			return (Tout) Queries.GetKey(obj);
		}

		public static object GetKey(T obj)
		{
			return Queries.GetKey(obj);
		}

		public static T Clone(T source)
		{
			return Queries.Clone(source);
		}

		public static bool Copy(T source, T dest)
		{
			return Queries.Copy(source, dest);
		}

		public static PropertyInfo[] Properties => Queries.Properties;
		public static PropertyInfo[] KeyProperties => Queries.KeyProperties;
		public static PropertyInfo[] AutoKeyProperties => Queries.AutoKeyProperties;
		public static PropertyInfo[] SelectProperties => Queries.SelectProperties;
		public static PropertyInfo[] UpdateProperties => Queries.UpdateProperties;
		public static PropertyInfo[] InsertProperties => Queries.InsertProperties;
		public static PropertyInfo[] EqualityProperties => Queries.EqualityProperties;

		public static string[] Columns => Queries.Columns;
		public static string[] KeyColumns => Queries.KeyColumns;
	}
}
