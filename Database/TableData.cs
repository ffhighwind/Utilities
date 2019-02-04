using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using System.Reflection;
using System.Reflection.Emit;

namespace Utilities.Database
{
	public static class TableData<T> where T : class
	{
		public static void Reinitialize(bool inheritAttributes = false, BindingFlags propertyFlags = BindingFlags.Public | BindingFlags.Instance)
		{
			InheritAttributes = inheritAttributes;
			Properties = GetTableProperties(propertyFlags);
			KeyProperties = Properties.Where(prop => prop.GetCustomAttribute<KeyAttribute>(InheritAttributes) != null).ToArray();
			AutoKeyProperties = KeyProperties.Where(prop => !prop.GetCustomAttribute<KeyAttribute>(InheritAttributes).Required).ToArray();
			CompareProperties = KeyProperties.Length == 0 ? Properties : KeyProperties;

			KeyQuery = GetEqualsParams(" AND ", KeyProperties);
			string selectQueryPart = "(" + string.Join(",", GetColumnNames(SelectProperties)) + ")\nFROM " + TableName + "\n";
			string whereKeyQuery = KeyProperties.Length == 0 ? "" : "\nWHERE " + KeyQuery;
			UpdateQuery = "UPDATE " + TableName + "\nSET " + GetEqualsParams(",", UpdateProperties) + whereKeyQuery;
			InsertQuery = "INSERT INTO " + TableName + "\n\t(" + string.Join(",", GetColumnNames(InsertProperties)) + ")\nVALUES (" + GetParams(InsertProperties) + ")";
			SelectListQuery = "SELECT (" + selectQueryPart + ")";
			SelectListKeysQuery = "SELECT (" + string.Join(",", GetColumnNames(KeyProperties)) + ")\n";
			SelectSingleQuery = "SELECT " + selectQueryPart + whereKeyQuery;
			DeleteSingleQuery = DeleteQuery + whereKeyQuery;
			CountQuery = "SELECT COUNT(*) FROM " + TableName + "\n";

			// NOTES:
			// Use Dapper to check DBMS and choose what to do?
			// Validate ForeignKeyAttribute
			// SqlMapper.AddTypeHandler<T>();
			DAO = new DataAccessObject<T>("");
		}

		static TableData()
		{
			TableAttribute tableAttribute = typeof(T).GetCustomAttribute<TableAttribute>(InheritAttributes);
			TableName = tableAttribute?.Name ?? typeof(T).Name;
			DeleteQuery = "DELETE FROM " + TableName + "\n";
			Reinitialize();
		}

		internal static bool InheritAttributes { get; private set; } = false;
		public static string TableName { get; private set; }
		public static bool IsCachable => KeyProperties.Length > 0;

		internal static DataAccessObject<T> DAO { get; private set; }
		public static PropertyInfo[] Properties { get; private set; } = GetTableProperties();
		public static PropertyInfo[] KeyProperties { get; private set; }
		public static PropertyInfo[] AutoKeyProperties { get; private set; }
		public static PropertyInfo[] SelectProperties => GetProperties(typeof(IgnoreSelectAttribute));
		public static PropertyInfo[] UpdateProperties => GetProperties(typeof(IgnoreUpdateAttribute));
		public static PropertyInfo[] InsertProperties => GetProperties(typeof(IgnoreInsertAttribute));
		internal static PropertyInfo[] CompareProperties { get; private set; }
		internal static string SelectListQuery { get; private set; }
		internal static string SelectListKeysQuery { get; private set; }
		internal static string SelectSingleQuery { get; private set; }
		internal static string UpdateQuery { get; private set; }
		internal static string InsertQuery { get; private set; }
		internal static string UpsertQuery => "IF NOT EXISTS (" + SelectSingleQuery + ")\n" + InsertQuery + "\nELSE\n" + UpdateQuery;
		internal static string DeleteQuery { get; private set; }
		internal static string DeleteSingleQuery { get; private set; }
		internal static string KeyQuery { get; private set; }
		internal static string CountQuery { get; private set; }

		private static string GetEqualsParams(string joinString, params PropertyInfo[] properties)
		{
			string[] names = GetColumnNames(properties);
			if (UpdateProperties.Length == 0)
				return "";
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < properties.Length; i++) {
				sb.AppendFormat("{0}[{1}] = @{1}", joinString, names[i]);
			}
			return sb.Remove(0, joinString.Length).ToString();
		}

		private static string GetParams(params PropertyInfo[] properties)
		{
			string[] names = GetColumnNames(properties);
			if (properties.Length == 0)
				return "";
			return "@" + string.Join(",@", names);
		}

		/// <summary>
		/// Returns true if the destination was modified, or false if they were identical.
		/// </summary>
		public static bool Copy(T source, T dest)
		{
			for (int i = 0; i < Properties.Length; i++) {
				object sourceValue = Properties[i].GetValue(source);
				object destValue = Properties[i].GetValue(dest);
				if (sourceValue != destValue) {
					for (int j = i; j < Properties.Length; j++) {
						Properties[j].SetValue(dest, Properties[j].GetValue(source));
					}
					return true;
				}
			}
			return false;
		}

		public static T CreateObjectFromKey(object key)
		{
			T objKey = (T)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
			TableData<T>.CopyKeyToObject(key, objKey);
			return objKey;
		}

		public static void CopyKeyToObject(object key, T obj)
		{
			Type type = key.GetType();
			for (int i = 0; i < KeyProperties.Length; i++) {
				KeyProperties[i].SetValue(obj, type.GetProperty(KeyProperties[i].Name, BindingFlags.SetProperty | BindingFlags.SetField | BindingFlags.Public | BindingFlags.Instance));
			}
		}

		private static PropertyInfo[] GetTableProperties(BindingFlags propertyFlags = BindingFlags.Instance | BindingFlags.Public)
		{
			return typeof(T).GetProperties(propertyFlags)
				.Where(prop => prop.GetCustomAttribute(typeof(IgnoreAttribute), InheritAttributes) == null
				&& prop.CanRead && prop.CanWrite && !prop.PropertyType.IsClass).ToArray();
		}

		private static PropertyInfo[] GetProperties(params Type[] ignoredAttributes)
		{
			List<PropertyInfo> properties = new List<PropertyInfo>();
			for (int i = 0; i < Properties.Length; i++) {
				Attribute ignoredAttr = null;
				for (int j = 0; j < ignoredAttributes.Length; j++) {
					ignoredAttr = Properties[i].GetCustomAttribute(ignoredAttributes[j]);
					if (ignoredAttr != null) {
						break;
					}
				}
				if (ignoredAttr == null && !AutoKeyProperties.Contains(properties[i])) {
					properties.Add(Properties[i]);
				}
			}
			return properties.Count == Properties.Length
				? Properties : properties.ToArray();
		}

		public static string[] GetColumnNames(params PropertyInfo[] properties)
		{
			string[] columnNames = new string[properties.Length];
			for (int i = 0; i < Properties.Length; i++) {
				columnNames[i] = Properties[i].Name;
				ColumnAttribute colAttr = Properties[i].GetCustomAttribute<ColumnAttribute>(InheritAttributes);
				if (colAttr != null) {
					columnNames[i] = colAttr.Name;
				}
			}
			return columnNames;
		}
	}
}
