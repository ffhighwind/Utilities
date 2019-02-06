using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public class TableDataImpl<T> : ITableData<T> where T : class
	{
		public TableDataImpl(BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
		{
			Properties = typeof(T).GetProperties(bindingFlags).Where(prop => prop.GetCustomAttribute<IgnoreAttribute>(false) == null
				&& prop.CanRead && prop.CanWrite && (!prop.PropertyType.IsClass || prop.PropertyType == typeof(string))).ToArray();
			KeyProperties = Properties.Where(prop => prop.GetCustomAttribute<KeyAttribute>(false) != null).ToArray();
			AutoKeyProperties = KeyProperties.Where(prop => !prop.GetCustomAttribute<KeyAttribute>(false).Required).ToArray();
			CompareProperties = KeyProperties.Length == 0 ? Properties : KeyProperties;
			SelectProperties = GetProperties(AutoKeyProperties, typeof(IgnoreSelectAttribute), typeof(IgnoreAttribute));
			InsertProperties = GetProperties(AutoKeyProperties, typeof(IgnoreInsertAttribute), typeof(IgnoreAttribute));
			UpdateProperties = GetProperties(AutoKeyProperties, typeof(IgnoreUpdateAttribute), typeof(IgnoreAttribute));

			string selectQueryPart = "([" + string.Join("],[", GetColumnNames(SelectProperties)) + "]) FROM " + TableName;
			string whereKeyQuery = "WHERE " + GetEqualsParams(" AND ", KeyProperties);
			string insertIntoQuery = "INSERT INTO " + TableName + " ([" + string.Join("],[", InsertColumns) + "])\n";
			string insertValuesQuery = "VALUES (" + GetParams(InsertProperties) + ")";
			string outputDeletedQuery = GetOutput("DELETED", true);
			string outputInsertedQuery = GetOutput("INSERTED", false);

			InsertQuery = insertIntoQuery + outputInsertedQuery + insertValuesQuery;
			SelectListQuery = "SELECT " + selectQueryPart + "\n";
			SelectListKeysQuery = "SELECT ([" + string.Join("],[", KeyColumns) + "]) FROM " + TableName + "\n";
			SelectSingleQuery = "SELECT " + selectQueryPart + "\n" + whereKeyQuery;
			CountQuery = "SELECT COUNT(*) FROM " + TableName + "\n";
			DeleteQuery = "DELETE FROM " + TableName + "\n";
			DeleteSingleQuery = DeleteQuery + whereKeyQuery;
			DeleteListQuery = DeleteQuery + outputDeletedQuery + "\n";

			string updateQuery = "UPDATE " + TableName + "\nSET \t" + GetEqualsParams(",", UpdateProperties);
			UpdateQuery = updateQuery + whereKeyQuery;
			UpsertQuery = "IF NOT EXISTS (SELECT " + selectQueryPart + ")\n\n" + InsertQuery + "\n\nELSE\n\n" + updateQuery + GetOutput("DELETED", true) + whereKeyQuery;

			// NOTES:
			// Use Dapper to check DBMS and choose what to do?
			// Validate ForeignKeyAttribute
			// SqlMapper.AddTypeHandler<T>();
		}

		/// <summary>
		/// Returns true if the destination was modified, or false if they were identical.
		/// </summary>
		public bool Copy(T source, T dest)
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

		public T CreateObject(object key)
		{
			T objKey = (T)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
			CopyKey(key, objKey);
			return objKey;
		}

		public void CopyKey(object key, T obj)
		{
			Type type = key.GetType();
			for (int i = 0; i < KeyProperties.Length; i++) {
				KeyProperties[i].SetValue(obj, type.GetProperty(KeyProperties[i].Name, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance));
			}
		}

		private string GetEqualsParams(string joinString, params PropertyInfo[] properties)
		{
			if (properties.Length == 0)
				return "";
			string[] columnNames = GetColumnNames(properties);
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < columnNames.Length; i++) {
				sb.AppendFormat("\t{0}[{1}] = @{2}\n", joinString, columnNames[i], properties[i].Name);
			}
			return sb.Remove(0, joinString.Length).ToString();
		}

		private string GetParams(params PropertyInfo[] properties)
		{
			if (properties.Length == 0)
				return "";
			string[] columnNames = GetColumnNames(properties);
			return "@" + string.Join(",@", columnNames);
		}

		private string GetOutput(string type, bool allIfNoKeys)
		{
			PropertyInfo[] properties = KeyProperties;
			if(KeyProperties.Length == 0) {
				if (!allIfNoKeys)
					return "";
				properties = Properties;
			}
			string[] columnNames = GetColumnNames(properties);
			return "OUTPUT " + type + ".[" + string.Join("]," + type + ".[", columnNames) + "]\n";
		}

		/// <summary>
		/// Gets the properties ignoring specified attributes and AutoKeyProperties.
		/// </summary>
		/// <param name="ignoredAttributes"></param>
		/// <returns></returns>
		protected PropertyInfo[] GetProperties(PropertyInfo[] ignoredProperties, params Type[] ignoredAttributes)
		{
			List<PropertyInfo> properties = new List<PropertyInfo>();
			for (int i = 0; i < Properties.Length; i++) {
				if (ignoredProperties.Contains(Properties[i])) {
					continue;
				}
				Attribute ignoredAttr = null;
				for (int j = 0; j < ignoredAttributes.Length; j++) {
					ignoredAttr = Properties[i].GetCustomAttribute(ignoredAttributes[j], false);
					if (ignoredAttr != null) {
						break;
					}
				}
				if(ignoredAttr == null) {
					properties.Add(Properties[i]);
				}
			}
			return properties.Count == Properties.Length
				? Properties : properties.ToArray();
		}

		public string[] GetColumnNames(params PropertyInfo[] properties)
		{
			string[] columnNames = new string[properties.Length];
			for (int i = 0; i < properties.Length; i++) {
				columnNames[i] = properties[i].Name;
				ColumnAttribute colAttr = properties[i].GetCustomAttribute<ColumnAttribute>(false);
				if (colAttr != null) {
					columnNames[i] = colAttr.Name.Replace("'", "''");
				}
			}
			return columnNames;
		}

		protected string SelectListQuery { get; private set; }
		protected string SelectListKeysQuery { get; private set; }
		protected string SelectSingleQuery { get; private set; }
		protected string UpdateQuery { get; private set; }
		protected string InsertQuery { get; private set; }
		protected string UpsertQuery { get; private set; }
		protected string DeleteQuery { get; private set; }
		protected string DeleteSingleQuery { get; private set; }
		protected string DeleteListQuery { get; private set; }
		protected string CountQuery { get; private set; }

		public TableAttribute TableAttribute => TableData<T>.TableAttribute;

		public string TableName => TableData<T>.TableName;

		public PropertyInfo[] Properties { get; private set; }
		public string[] Columns => GetColumnNames(Properties);

		public PropertyInfo[] KeyProperties { get; private set; }
		public string[] KeyColumns => GetColumnNames(KeyProperties);

		public PropertyInfo[] AutoKeyProperties { get; private set; }
		public string[] AutoKeyColumns => GetColumnNames(AutoKeyProperties);

		public PropertyInfo[] SelectProperties { get; private set; }
		public string[] SelectColumns => GetColumnNames(SelectProperties);

		public PropertyInfo[] UpdateProperties { get; private set; }
		public string[] UpdateColumns => GetColumnNames(UpdateProperties);

		public PropertyInfo[] InsertProperties { get; private set; }
		public string[] InsertColumns => GetColumnNames(InsertProperties);

		/// <summary>
		/// KeyProperties, or all Properties if there are no KeyProperties.
		/// </summary>
		public PropertyInfo[] CompareProperties { get; private set; }

		#region ITableQueries<T>
		public List<T> GetKeys(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(SelectListKeysQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public bool Delete(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return 0 < connection.Execute(DeleteSingleQuery, key, transaction, commandTimeout);
		}

		public bool Delete(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return 0 < connection.Execute(DeleteSingleQuery, obj, transaction, commandTimeout);
		}

		public int Delete(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			int count = 0;
			foreach (T obj in objs) {
				if (Delete(connection, obj, transaction, commandTimeout)) {
					count++;
				}
			}
			return count;
		}

		public int Delete(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Execute(DeleteQuery + whereCondition, param, transaction, commandTimeout);
		}

		public List<T> DeleteList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(DeleteListQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public void Insert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			CopyKey(connection.Query<dynamic>(InsertQuery, obj, transaction, true, commandTimeout), obj);
		}

		public void Insert(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			foreach (T obj in objs) {
				Insert(connection, obj, transaction, commandTimeout);
			}
		}

		public bool Update(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return 0 < connection.ExecuteScalar<int>(UpdateQuery, obj, transaction, commandTimeout);
		}

		public int Update(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			int count = 0;
			foreach (T obj in objs) {
				if (Update(connection, obj, transaction, commandTimeout)) {
					count++;
				}
			}
			return count;
		}

		public void Upsert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			CopyKey(connection.Query<T>(UpsertQuery, obj, transaction, true, commandTimeout), obj);
		}

		public void Upsert(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			foreach (T obj in objs) {
				Upsert(connection, obj, transaction, commandTimeout);
			}
		}

		public T Get(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.Query<T>(SelectSingleQuery, key, transaction, true, commandTimeout).SingleOrDefault();
		}

		public T Get(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.Query<T>(SelectSingleQuery, obj, transaction, true, commandTimeout).SingleOrDefault();
		}

		public List<T> GetList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(SelectListQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public int RecordCount(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.ExecuteScalar<int>(CountQuery + whereCondition, param, transaction, commandTimeout);
		}
		#endregion // ITableQueries<T>

		#region ITableQueriesAsync<T>
		public async Task<List<T>> GetKeysAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetKeys(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public async Task<bool> DeleteAsync(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, key, transaction, commandTimeout));
		}

		public async Task<bool> DeleteAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, obj, transaction, commandTimeout));
		}

		public async Task<int> DeleteAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, objs, transaction, commandTimeout));
		}

		public async Task<int> DeleteAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public async Task<List<T>> DeleteListAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => DeleteList(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public async Task InsertAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			await Task.Run(() => Insert(connection, obj, transaction, commandTimeout));
		}

		public async Task InsertAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			await Task.Run(() => Insert(connection, objs, transaction, commandTimeout));
		}

		public async Task<bool> UpdateAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(connection, obj, transaction, commandTimeout));
		}

		public async Task<int> UpdateAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(connection, objs, transaction, commandTimeout));
		}

		public async Task UpsertAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			await Task.Run(() => Upsert(connection, obj, transaction, commandTimeout));
		}

		public async Task UpsertAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			await Task.Run(() => Upsert(connection, objs, transaction, commandTimeout));
		}

		public async Task<T> GetAsync(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(connection, key, transaction, commandTimeout));
		}

		public async Task<T> GetAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(connection, obj, transaction, commandTimeout));
		}

		public async Task<List<T>> GetListAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetList(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public async Task<int> RecordCountAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => RecordCount(connection, whereCondition, param, transaction, commandTimeout));
		}
		#endregion // ITableQueriesAsync<T>
	}
}
