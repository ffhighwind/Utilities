using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension.Interfaces;
using Utilities.Converters;

namespace Dapper.Extension
{
	public class TableDataImpl<T> : ITableData<T> where T : class
	{
		private const string BulkTempTable = "#BulkTempTable_";

		public static TableDataImpl<T> Default { get; private set; } = new TableDataImpl<T>();

		public TableDataImpl(BindingFlags propertyFlags = BindingFlags.Public | BindingFlags.Instance)
		{
			Properties = typeof(T).GetProperties(propertyFlags).Where(prop => prop.GetCustomAttribute<IgnoreAttribute>(false) == null
				&& prop.CanRead && prop.CanWrite && (!prop.PropertyType.IsClass || prop.PropertyType == typeof(string))).ToArray();
			KeyProperties = Properties.Where(prop => prop.GetCustomAttribute<KeyAttribute>(false) != null).ToArray();
			AutoKeyProperties = KeyProperties.Where(prop => !prop.GetCustomAttribute<KeyAttribute>(false).Required).ToArray();
			SelectProperties = GetProperties(Array.Empty<PropertyInfo>(), (prop) => true, typeof(IgnoreSelectAttribute), typeof(IgnoreAttribute));
			InsertProperties = GetProperties(AutoKeyProperties, (prop) => { var attr = prop.GetCustomAttribute<IgnoreInsertAttribute>(); return attr == null || attr.Value != null; }, typeof(IgnoreAttribute));
			UpdateProperties = GetProperties(AutoKeyProperties, (prop) => { var attr = prop.GetCustomAttribute<IgnoreUpdateAttribute>(); return attr == null || attr.Value != null; }, typeof(IgnoreAttribute));
			UpsertProperties = GetProperties(AutoKeyProperties, (prop) => true, typeof(IgnoreAttribute));

			Columns = GetColumnNames(Properties);
			KeyColumns = GetColumnNames(KeyProperties);
			AutoKeyColumns = GetColumnNames(AutoKeyProperties);
			SelectColumns = GetColumnNames(SelectProperties);
			UpdateColumns = GetColumnNames(UpdateProperties);
			InsertColumns = GetColumnNames(InsertProperties);
			UpsertColumns = GetColumnNames(UpsertProperties);

			if (KeyProperties.Length == 0) {
				EqualityProperties = Properties;
				EqualityColumns = Columns;
			}
			else {
				EqualityProperties = KeyProperties;
				EqualityColumns = KeyColumns;
			}
			InsertDefaults = new string[InsertProperties.Length];
			for (int i = 0; i < InsertProperties.Length; i++) {
				InsertDefaults[i] = InsertProperties[i].GetCustomAttribute<IgnoreInsertAttribute>()?.Value;
			}
			UpdateDefaults = new string[UpdateProperties.Length];
			for (int i = 0; i < UpdateProperties.Length; i++) {
				UpdateDefaults[i] = UpdateProperties[i].GetCustomAttribute<IgnoreUpdateAttribute>()?.Value;
			}
			GenerateQueries();
		}

		protected void GenerateQueries()
		{
			string selectQueryPart = "[" + string.Join("],[", GetColumnNames(SelectProperties)) + "] FROM " + TableName;
			string whereEqualsQuery = "WHERE " + GetEqualsParams(" AND ", EqualityProperties, new string[EqualityProperties.Length]);
			string whereKeyQuery = KeyProperties.Length == 0 ? "" : whereEqualsQuery;
			string insertColumnsQuery = "[" + string.Join("],[", InsertColumns) + "]";
			string insertIntoQuery = "INSERT INTO " + TableName + " (" + insertColumnsQuery + ")\n";
			string insertValuesQuery = "VALUES (" + GetValues(InsertProperties, InsertDefaults) + ")";
			string whereTempEqualsQuery = "WHERE " + GetTempEqualsParams();

			InsertQuery = insertIntoQuery + "OUTPUT INSERTED.*\n" + insertValuesQuery;
			//InsertQuery = insertIntoQuery + GetOutput("INSERTED", false) + insertValuesQuery;
			SelectListQuery = "SELECT " + selectQueryPart + "\n";
			SelectListKeysQuery = "SELECT [" + string.Join("],[", KeyColumns) + "] FROM " + TableName + "\n";
			SelectSingleQuery = "SELECT " + selectQueryPart + "\n" + whereEqualsQuery;
			CountQuery = "SELECT COUNT(*) FROM " + TableName + "\n";
			DeleteQuery = "DELETE FROM " + TableName + "\n";
			DeleteSingleQuery = DeleteQuery + whereEqualsQuery;
			DeleteListQuery = DeleteQuery + "OUTPUT DELETED.*\n";
			DeleteListKeysQuery = DeleteQuery + GetOutput("DELETED", true) + "\n";

			string updateQuery = "UPDATE " + TableName + "\nSET " + GetEqualsParams(",", UpdateProperties, UpdateDefaults);
			UpsertQuery = "IF NOT EXISTS (\nSELECT TOP(1) * FROM " + TableName + "\n" + whereEqualsQuery + ")\n" + insertIntoQuery + "\nOUTPUT INSERTED.*\n" + insertValuesQuery;
			//UpsertQuery = "IF NOT EXISTS (\nSELECT TOP(1) * FROM " + TableName + "\n" + whereEqualsQuery + ")\n" + insertIntoQuery + GetOutput("INSERTED", true) + insertValuesQuery;

			BulkUpdateQuery = updateQuery + "\nFROM " + BulkTempTable + "," + TableName + "\nWHERE " + whereTempEqualsQuery;
			BulkInsertNotExistsQuery = insertIntoQuery + "SELECT " + insertColumnsQuery + "\nFROM " + BulkTempTable + "\nWHERE NOT EXISTS (\nSELECT * FROM " + TableName + "\n" + whereTempEqualsQuery + ")";
			string whereExistsTemp = "WHERE EXISTS (\nSELECT * FROM " + TableName + "\n" + whereTempEqualsQuery + ")";
			BulkDeleteQuery = DeleteQuery + whereExistsTemp;
			BulkDeleteListQuery = DeleteListQuery + whereExistsTemp;

			if (KeyProperties.Length == 0) {
				UpdateQuery = "";
				SelectListKeysQuery = "";
			}
			else {
				UpdateQuery = updateQuery + whereKeyQuery;
				UpsertQuery = UpsertQuery + "\n\nELSE\n\n" + updateQuery + GetOutput("DELETED", true) + whereEqualsQuery;
			}
		}

		private static IEnumerable<IEnumerable<Ty>> Partition<Ty>(IEnumerable<Ty> source, int size)
		{
			while (source.Any()) {
				yield return source.Take(size);
				source = source.Skip(size);
			}
		}

		/// <summary>
		/// [Table("Name")] or the class name
		/// </summary>
		protected string TableName => TableData<T>.TableName;

		/// <summary>
		/// x = @x, y = @y
		/// x = @x AND y = @y
		/// x = @x OR  y = @y
		/// </summary>
		private string GetEqualsParams(string joinString, PropertyInfo[] properties, string[] defaultValues)
		{
			if (properties.Length == 0)
				return "";
			string[] columnNames = GetColumnNames(properties);
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < columnNames.Length; i++) {
				sb.AppendFormat("\t{0}[{1}] = {2}\n", joinString, columnNames[i], defaultValues[i] ?? ("@" + properties[i].Name));
			}
			return "\t" + sb.Remove(0, joinString.Length + 1).ToString();
		}

		/// <summary>
		/// #BulkTempTable_.[x] = TableName.[x]
		/// </summary>
		private string GetTempEqualsParams()
		{
			if (EqualityColumns.Length == 0)
				return "";
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < EqualityColumns.Length; i++) {
				sb.AppendFormat("\t AND {1}.[{0}] = {2}.[{0}]\n", EqualityColumns[i], BulkTempTable, TableName);
			}
			return "\t" + sb.Remove(0, 5).ToString(); // remove "\t AND "
		}

		/// <summary>
		/// @a,@b,@c
		/// </summary>
		private string GetValues(PropertyInfo[] properties, string[] defaultValues)
		{
			if (properties.Length == 0)
				return "";
			StringBuilder sb = new StringBuilder(defaultValues[0] ?? ("@" + properties[0].Name));
			for (int i = 1; i < properties.Length; i++) {
				sb.Append(',').Append(defaultValues[i] ?? ("@" + properties[i].Name));
			}
			return sb.ToString();
		}

		/// <summary>
		/// OUTPUT INSERTED.[a] as A
		/// </summary>
		private string GetOutput(string type, bool allIfNoKeys)
		{
			PropertyInfo[] properties = KeyProperties;
			if (KeyProperties.Length == 0) {
				if (!allIfNoKeys)
					return "";
				properties = Properties;
			}
			string[] columnNames = GetColumnNames(properties);
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < columnNames.Length; i++) {
				sb.AppendFormat("\t,{0}.[{1}]", type, columnNames[i]);
				if (columnNames[i] != properties[i].Name) {
					sb.AppendFormat(" as [{0}]", properties[i].Name);
				}
				sb.Append("\n");
			}
			return "OUTPUT \t" + sb.Remove(0, 2).ToString();
		}

		/// <summary>
		/// Gets a list of PropertyInfos from the base list except the ones in a specific set of ignored items.
		/// </summary>
		/// <param name="ignoredProperties">PropertyInfos in this list are ignored.</param>
		/// <param name="ignoredAttributes">PropertyInfos with these any of these Attributes are ignored.</param>
		/// <returns>A set of PropertyInfos.</returns>
		protected PropertyInfo[] GetProperties(PropertyInfo[] ignoredProperties, Func<PropertyInfo, bool> accepted, params Type[] ignoredAttributes)
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
				if (ignoredAttr == null && accepted(Properties[i])) {
					properties.Add(Properties[i]);
				}
			}
			return properties.Count == Properties.Length
				? Properties : properties.ToArray();
		}

		/// <summary>
		/// [Column("Name")] or the name of the properties.
		/// </summary>
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

		internal string SelectListQuery { get; private set; }
		internal string SelectListKeysQuery { get; private set; }
		internal string SelectSingleQuery { get; private set; }
		internal string UpdateQuery { get; private set; }
		internal string InsertQuery { get; private set; }
		internal string UpsertQuery { get; private set; }
		internal string DeleteQuery { get; private set; }
		internal string DeleteSingleQuery { get; private set; }
		internal string DeleteListQuery { get; private set; }
		internal string DeleteListKeysQuery { get; private set; }
		internal string CountQuery { get; private set; }
		internal string BulkUpdateQuery { get; private set; }
		internal string BulkUpsertQuery { get; private set; }
		internal string BulkInsertNotExistsQuery { get; private set; }
		internal string BulkDeleteQuery { get; private set; }
		internal string BulkDeleteListQuery { get; private set; }

		public string[] AutoKeyColumns { get; private set; }
		public string[] SelectColumns { get; private set; }
		public string[] UpdateColumns { get; private set; }
		public string[] InsertColumns { get; private set; }
		public string[] UpsertColumns { get; private set; }
		public string[] EqualityColumns { get; private set; }

		public string[] InsertDefaults { get; set; }
		public string[] UpdateDefaults { get; set; }

		#region ITableQueries<T>
		public override List<T> GetKeys(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(SelectListKeysQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public override bool Delete(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return 0 < connection.Execute(DeleteSingleQuery, key, transaction, commandTimeout);
		}

		public override bool Delete(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return 0 < connection.Execute(DeleteSingleQuery, obj, transaction, commandTimeout);
		}

		public override int Delete(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Execute(DeleteQuery + whereCondition, param, transaction, commandTimeout);
		}

		public override List<T> DeleteList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(DeleteListQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public override T Insert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			IEnumerable<dynamic> key = connection.Query<dynamic>(InsertQuery, obj, transaction, true, commandTimeout);
			if (key.Any()) {
				TableData<T>.SetKey(obj, key.First());
			}
			return obj;
		}

		public override bool Update(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return 0 < connection.Execute(UpdateQuery, obj, transaction, commandTimeout);
		}

		public override T Upsert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			dynamic key = connection.Query<dynamic>(UpsertQuery, obj, transaction, true, commandTimeout).FirstOrDefault();
			if (key != null) {
				TableData<T>.SetKey(obj, key);
			}
			return obj;
		}

		public override T Get(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.Query<T>(SelectSingleQuery, key, transaction, true, commandTimeout).SingleOrDefault();
		}

		public override T Get(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.Query<T>(SelectSingleQuery, obj, transaction, true, commandTimeout).SingleOrDefault();
		}

		public override List<T> GetList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(SelectListQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public override int RecordCount(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.ExecuteScalar<int>(CountQuery + whereCondition, param, transaction, commandTimeout);
		}

		public override List<KeyType> GetKeys<KeyType>(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<KeyType>(SelectListKeysQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public override bool Delete<KeyType>(IDbConnection connection, KeyType key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			dynamic newKey = new ExpandoObject();
			newKey[KeyColumns[0]] = key;
			return Delete(connection, newKey, transaction, commandTimeout);
		}

		public override T Get<KeyType>(IDbConnection connection, KeyType key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			dynamic newKey = new ExpandoObject();
			((IDictionary<string, object>) newKey)[KeyColumns[0]] = key;
			return Get(connection, (object) newKey, transaction, commandTimeout);
		}

		public override List<KeyType> DeleteList<KeyType>(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<KeyType>(DeleteListKeysQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public override int BulkDelete(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			TableData<T>.CloneTable(connection, BulkTempTable, transaction, EqualityColumns);
			BulkInsert_(connection, objs, transaction, BulkTempTable, EqualityColumns, EqualityProperties, commandTimeout);
			int count = connection.Execute(BulkDeleteQuery, null, transaction, commandTimeout);
			connection.Execute("DROP TABLE " + BulkTempTable);
			return count;
			/*
			int count = 0;

			foreach (var param in CreateDynamicParams(objs)) {
				count += connection.Execute(DeleteQuery + param.Item1, param.Item2, transaction, commandTimeout);
			}
			return count;*/
		}

		public override List<T> BulkDeleteList(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			TableData<T>.CloneTable(connection, BulkTempTable, transaction, EqualityColumns);
			BulkInsert_(connection, objs, transaction, BulkTempTable, EqualityColumns, EqualityProperties, commandTimeout);
			List<T> list = connection.Query<T>(BulkDeleteListQuery, null, transaction, buffered, commandTimeout).AsList();
			connection.Execute("DROP TABLE " + BulkTempTable);
			return list;
			/*
			List<T> list = new List<T>();
			foreach (var param in CreateDynamicParams(objs)) {
				list.AddRange(DeleteList(connection, param.Item1, param.Item2, transaction, buffered, commandTimeout));
			}
			return list;
			*/
		}

		public override IEnumerable<T> BulkInsert(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			TableData<T>.CloneTable(connection, BulkTempTable, transaction, InsertColumns);
			BulkInsert_(connection, objs, transaction, TableData<T>.TableName, InsertColumns, InsertProperties, commandTimeout);
			connection.Execute("DROP TABLE " + BulkTempTable);
			return objs;
		}

		private void BulkInsert_(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction, string tableName, string[] columns, PropertyInfo[] properties, int? commandTimeout = null)
		{
			using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)) {
				using (GenericDataReader<T> dataReader = new GenericDataReader<T>(objs, columns, properties)) {
					bulkCopy.DestinationTableName = tableName ?? TableData<T>.TableName;
					bulkCopy.BulkCopyTimeout = commandTimeout ?? 0;
					bulkCopy.WriteToServer(dataReader);
				}
			}
		}

		public override int BulkUpdate(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			TableData<T>.CloneTable(connection, BulkTempTable, transaction, UpdateColumns);
			BulkInsert_(connection, objs, transaction, BulkTempTable, UpdateColumns, UpdateProperties, commandTimeout);
			int count = connection.Execute(BulkUpdateQuery, null, transaction, commandTimeout);
			connection.Execute("DROP TABLE " + BulkTempTable);
			return count;
		}

		public override IEnumerable<T> BulkUpsert(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			TableData<T>.CloneTable(connection, BulkTempTable, transaction, UpsertColumns);
			BulkInsert_(connection, objs, transaction, BulkTempTable, UpsertColumns, UpsertProperties, commandTimeout);
			connection.Execute(BulkUpdateQuery, null, transaction, commandTimeout);
			connection.Execute(BulkInsertNotExistsQuery, null, transaction, commandTimeout);
			connection.Execute("DROP TABLE " + BulkTempTable);
			return objs;
		}

		public override int BulkDelete<KeyType>(SqlConnection connection, IEnumerable<KeyType> keys, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			int count = 0;
			foreach (IEnumerable<KeyType> Keys in Partition<KeyType>(keys, 2000)) {
				count += Delete(connection, "WHERE [" + KeyColumns[0] + "] in @Keys", new { Keys }, transaction, true, commandTimeout);
			}
			return count;
		}

		public override List<KeyType> BulkDeleteList<KeyType>(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			List<KeyType> keys = new List<KeyType>();
			foreach (IEnumerable<KeyType> Keys in Partition<KeyType>(objs.Select(obj => TableData<T>.GetKey<KeyType>(obj)), 2000)) {
				keys.AddRange(DeleteList<KeyType>(connection, "WHERE [" + KeyColumns[0] + "] in @Keys", new { Keys }, transaction, buffered, commandTimeout));
			}
			return keys;
		}
		#endregion // ITableQueries<T>

		private IEnumerable<Tuple<string, DynamicParameters>> CreateDynamicParams(IEnumerable<T> objs)
		{
			int max = 2000 / EqualityProperties.Length;

			foreach (IEnumerable<T> part in Partition<T>(objs, max)) {
				int k = 0;
				StringBuilder sb = new StringBuilder("WHERE ");
				DynamicParameters param = new DynamicParameters();

				foreach (T obj in part) {
					sb.Append("([" + KeyColumns[0] + "] = @p" + k);
					param.Add("@p" + k, KeyProperties[0].GetValue(obj));
					k++;
					for (int j = 1; j < KeyColumns.Length; j++) {
						sb.Append(" AND [" + KeyColumns[j] + "] = @p" + k);
						param.Add("@p" + k, KeyProperties[j].GetValue(obj));
						k++;
					}
					sb.Append(")");
					sb.Append(" OR ");
				}
				sb.Remove(sb.Length - 4, 4);
				yield return new Tuple<string, DynamicParameters>(sb.ToString(), param);
			}
		}
	}
}
