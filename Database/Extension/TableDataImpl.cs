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
		private const string BulkTempStagingTable = "#_DappBulkTable_";

		public static TableDataImpl<T> Default { get; private set; } = new TableDataImpl<T>();

		public TableDataImpl(BindingFlags propertyFlags = BindingFlags.Public | BindingFlags.Instance)
		{
			Properties = typeof(T).GetProperties(propertyFlags).Where(prop => prop.CanRead && prop.CanWrite && (!prop.PropertyType.IsClass || prop.PropertyType == typeof(string))).ToArray();
			KeyProperties = Properties.Where(prop => prop.GetCustomAttribute<KeyAttribute>(true) != null).ToArray();
			AutoKeyProperties = KeyProperties.Where(prop => !prop.GetCustomAttribute<KeyAttribute>(true).Required).ToArray();
			for (int i = 0; i < KeyProperties.Length; i++) {
				if (KeyProperties[i].GetCustomAttribute<IgnoreSelectAttribute>(true) != null || KeyProperties[i].GetCustomAttribute<IgnoreAttribute>(true) != null
					|| KeyProperties[i].GetCustomAttribute<IgnoreUpdateAttribute>(true) != null || KeyProperties[i].GetCustomAttribute<IgnoreInsertAttribute>(true) != null) {
					throw new InvalidOperationException("Cannot ignore key properties");
				}
			}
			Properties = Properties.Where(prop => prop.GetCustomAttribute<IgnoreAttribute>(true) == null && (prop.GetCustomAttribute<IgnoreSelectAttribute>(true) == null
				|| prop.GetCustomAttribute<IgnoreInsertAttribute>(true) == null || prop.GetCustomAttribute<IgnoreUpdateAttribute>(true) == null)).ToArray();
			SelectProperties = GetProperties(Array.Empty<PropertyInfo>(), (prop) => true, typeof(IgnoreSelectAttribute), typeof(IgnoreAttribute));
			InsertProperties = GetProperties(AutoKeyProperties, (prop) => { var attr = prop.GetCustomAttribute<IgnoreInsertAttribute>(true); return attr == null || attr.Value != null; }, typeof(IgnoreAttribute));
			UpdateProperties = GetProperties(KeyProperties, (prop) => { var attr = prop.GetCustomAttribute<IgnoreUpdateAttribute>(true); return attr == null || attr.Value != null; }, typeof(IgnoreAttribute));
			PropertyInfo[] MatchUpdateProperties = UpdateProperties.Where(x => x.GetCustomAttribute<MatchUpdateAttribute>(true) != null).ToArray();
			if (MatchUpdateProperties.Length > 0) {
				UpdateProperties = UpdateProperties.Where(x => !MatchUpdateProperties.Contains(x)).ToArray();
			}
			PropertyInfo[] MatchDeleteProperties = Properties.Where(prop => prop.GetCustomAttribute<KeyAttribute>(true) != null || prop.GetCustomAttribute<MatchDeleteAttribute>(true) != null).ToArray();

			Columns = GetColumnNames(Properties);
			KeyColumns = GetColumnNames(KeyProperties);
			AutoKeyColumns = GetColumnNames(AutoKeyProperties);
			SelectColumns = GetColumnNames(SelectProperties);
			UpdateColumns = GetColumnNames(UpdateProperties);
			InsertColumns = GetColumnNames(InsertProperties);

			if (KeyProperties.Length == 0) {
				EqualityProperties = Properties;
				EqualityColumns = Columns;
				UpdateEqualityProperties = EqualityProperties;
				UpdateEqualityColumns = Columns;
				DeleteEqualityColumns = Columns;
			}
			else {
				EqualityProperties = KeyProperties;
				EqualityColumns = KeyColumns;
				UpdateEqualityProperties = EqualityProperties.Union(MatchUpdateProperties).ToArray();
				DeleteEqualityProperties = EqualityProperties.Union(MatchDeleteProperties).ToArray();
				UpdateEqualityColumns = GetColumnNames(UpdateEqualityProperties);
				DeleteEqualityColumns = GetColumnNames(DeleteEqualityProperties);
			}

			GenerateQueries();
		}

		protected void GenerateQueries()
		{
			string[] insertDefaults = new string[InsertProperties.Length];
			for (int i = 0; i < InsertProperties.Length; i++) {
				insertDefaults[i] = InsertProperties[i].GetCustomAttribute<IgnoreInsertAttribute>(true)?.Value;
			}
			string[] updateDefaults = new string[UpdateProperties.Length];
			for (int i = 0; i < UpdateProperties.Length; i++) {
				updateDefaults[i] = UpdateProperties[i].GetCustomAttribute<IgnoreUpdateAttribute>(true)?.Value;
			}

			string whereEquals = "WHERE " + GetEqualsParams(" AND ", EqualityProperties, EqualityColumns, new string[EqualityProperties.Length]);
			string whereUpdateEquals = "WHERE " + GetEqualsParams(" AND ", UpdateEqualityProperties, UpdateEqualityColumns, new string[UpdateEqualityProperties.Length]);
			string whereDeleteEquals = "WHERE " + GetEqualsParams(" AND ", DeleteEqualityProperties, DeleteEqualityColumns, new string[DeleteEqualityProperties.Length]);

			string paramsInsert = "[" + string.Join("],[", InsertColumns) + "]";
			string insertTableParams = "INSERT " + TableName + " (" + paramsInsert + ")\n";
			string valuesInserted = "VALUES (" + GetValues(InsertProperties, insertDefaults) + ")\n";
			string outputInserted = GetOutput("INSERTED", Properties) + "\n";
			string outputInsertedKeys = KeyProperties.Length == 0 ? outputInserted : (GetOutput("INSERTED", KeyProperties) + "\n");
			string outputDeleted = GetOutput("DELETED", Properties) + "\n";
			string outputDeletedKeys = KeyProperties.Length == 0 ? outputDeleted : (GetOutput("DELETED", KeyProperties) + "\n");
			string updateTableSetParams = "UPDATE " + TableName + "\nSET " + GetEqualsParams(",", UpdateProperties, UpdateColumns, updateDefaults);

			InsertQuery = insertTableParams + outputInsertedKeys + valuesInserted;
			SelectListQuery = "SELECT " + GetAsParams(SelectProperties) + " FROM " + TableName + "\n";
			SelectSingleQuery = SelectListQuery + whereEquals;
			CountQuery = "SELECT COUNT(*) FROM " + TableName + "\n";
			DeleteQuery = "DELETE FROM " + TableName + "\n";
			DeleteSingleQuery = DeleteQuery + whereDeleteEquals;
			DeleteListQuery = DeleteQuery + outputDeleted;
			DeleteListKeysQuery = DeleteQuery + outputDeletedKeys;

			UpsertQuery = "IF NOT EXISTS (\nSELECT TOP(1) * FROM " + TableName + "\n" +  whereEquals + ")\n" + insertTableParams + outputInserted + valuesInserted;

			BulkUpdateQuery = "UPDATE " + TableName + "\nSET " + GetTempSetParams(UpdateColumns, updateDefaults)
				+ "\nFROM " + BulkTempStagingTable + "\nWHERE " + GetTempAndEqualsParams(UpdateEqualityColumns);
			BulkInsertNotExistsQuery = insertTableParams + outputInserted + "SELECT " + paramsInsert + "\nFROM " + BulkTempStagingTable + "\nWHERE NOT EXISTS (\nSELECT * FROM " + TableName + "\nWHERE " + GetTempAndEqualsParams(EqualityColumns) + ")";
			string whereDeleteExistsBulk = "WHERE EXISTS (\nSELECT * FROM [" + BulkTempStagingTable + "]\nWHERE " + GetTempAndEqualsParams(DeleteEqualityColumns) + ")";
			BulkDeleteQuery = DeleteQuery + whereDeleteExistsBulk;
			BulkDeleteListQuery = DeleteListQuery + whereDeleteExistsBulk;
			BulkInsertListQuery = insertTableParams + outputInserted + "SELECT * FROM " + BulkTempStagingTable;

			if (KeyProperties.Length == 0) {
				UpdateQuery = ""; //no keys means can't update
				BulkUpdateQuery = "";
				BulkUpsertQuery = BulkInsertNotExistsQuery;
				SelectListKeysQuery = SelectListQuery;
			}
			else {
				UpdateQuery = updateTableSetParams + whereUpdateEquals;
				UpsertQuery = UpsertQuery + "\n\nELSE\n\n" + updateTableSetParams + outputDeleted + whereUpdateEquals;
				SelectListKeysQuery = "SELECT " + GetAsParams(KeyProperties) + " FROM " + TableName + "\n";
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
		private string GetEqualsParams(string joinString, PropertyInfo[] properties, string[] columnNames, string[] defaultValues)
		{
			if (properties.Length == 0)
				return "";
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < columnNames.Length; i++) {
				sb.AppendFormat("\t{0}[{1}] = {2}\n", joinString, columnNames[i], defaultValues[i] ?? ("@" + properties[i].Name));
			}
			return "\t" + sb.Remove(0, joinString.Length + 1).ToString();
		}

		/// <summary>
		/// #BulkTempTable_.[x] = TableName.[x] AND #BulkTempTable_.[y] = TableName.[y]
		/// </summary>
		private string GetTempAndEqualsParams(string[] columns)
		{
			if (columns.Length == 0)
				return "";
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < columns.Length; i++) {
				sb.AppendFormat("\t AND [{1}].[{0}] = [{2}].[{0}]\n", columns[i], TableName, BulkTempStagingTable);
			}
			return "\t" + sb.Remove(0, 6).ToString();
		}


		/// <summary>
		/// TableName.[x] = #BulkTempTable_.[x], TableName.[y] = getdate()
		/// </summary>
		private string GetTempSetParams(string[] columns, string[] defaultValues)
		{
			if (columns.Length == 0)
				return "";
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < columns.Length; i++) {
				sb.AppendFormat("\t[{0}].[{1}] = ", TableName, columns[i]);
				if (defaultValues[i] == null) {
					sb.AppendFormat("[{0}].[{1}],\n", BulkTempStagingTable, columns[i]);
				}
				else {
					sb.Append(defaultValues[i] + ",\n");
				}
			}
			return sb.Remove(sb.Length - 2, 2).Append('\n').ToString();
		}

		/// <summary>
		/// #BulkTempTable_.[x] = TableName.[x] as [name]
		/// </summary>
		private string GetAsParams(PropertyInfo[] properties)
		{
			if (properties.Length == 0)
				return "";

			string[] columnNames = GetColumnNames(SelectProperties);
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < properties.Length; i++) {
				sb.Append("[" + columnNames[i] + "]");
				if (properties[i].Name != columnNames[i]) {
					sb.Append(" as [" + properties[i].Name + "]");
				}
				sb.Append(',');
			}
			sb.Remove(sb.Length - 1, 1);
			return sb.ToString();
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
		private string GetOutput(string type, PropertyInfo[] properties)
		{
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
					ignoredAttr = Properties[i].GetCustomAttribute(ignoredAttributes[j], true);
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
				ColumnAttribute colAttr = properties[i].GetCustomAttribute<ColumnAttribute>(true);
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
		internal string BulkInsertListQuery { get; private set; }
		internal string BulkInsertNotExistsQuery { get; private set; }
		internal string BulkDeleteQuery { get; private set; }
		internal string BulkDeleteListQuery { get; private set; }

		public string[] AutoKeyColumns { get; private set; }
		public string[] SelectColumns { get; private set; }
		public string[] UpdateColumns { get; private set; }
		public string[] InsertColumns { get; private set; }
		public string[] EqualityColumns { get; private set; }
		public string[] UpdateEqualityColumns { get; private set; }
		public string[] DeleteEqualityColumns { get; private set; }

		#region ITableQueries<T>
		public override IEnumerable<T> GetKeys(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(SelectListKeysQuery + whereCondition, param, transaction, buffered, commandTimeout);
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
			string query = string.IsNullOrWhiteSpace(whereCondition) ? "TRUNCATE TABLE " + TableName : DeleteQuery + whereCondition;
			return connection.Execute(query, param, transaction, commandTimeout);
		}

		public override IEnumerable<T> DeleteList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(DeleteListQuery + whereCondition, param, transaction, buffered, commandTimeout);
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

		public override IEnumerable<T> GetList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(SelectListQuery + whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override int RecordCount(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.ExecuteScalar<int>(CountQuery + whereCondition, param, transaction, commandTimeout);
		}

		public override IEnumerable<KeyType> GetKeys<KeyType>(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<KeyType>(SelectListKeysQuery + whereCondition, param, transaction, buffered, commandTimeout);
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

		public override IEnumerable<KeyType> DeleteList<KeyType>(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<KeyType>(DeleteListKeysQuery + whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override int BulkDelete(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			TableData<T>.CreateTable(connection, BulkTempStagingTable, transaction, EqualityColumns, commandTimeout);
			BulkInsert_(connection, objs, transaction, BulkTempStagingTable, EqualityColumns, EqualityProperties, commandTimeout);
			int count = connection.Execute(BulkDeleteQuery, null, transaction, commandTimeout);
			connection.Execute("DROP TABLE " + BulkTempStagingTable, null, transaction, commandTimeout);
			return count;
		}

		public override IEnumerable<T> BulkDeleteList(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			TableData<T>.CreateTable(connection, BulkTempStagingTable, transaction, EqualityColumns, commandTimeout);
			BulkInsert_(connection, objs, transaction, BulkTempStagingTable, EqualityColumns, EqualityProperties, commandTimeout);
			IEnumerable<T> list = connection.Query<T>(BulkDeleteListQuery, null, transaction, buffered, commandTimeout);
			connection.Execute("DROP TABLE " + BulkTempStagingTable, null, transaction, commandTimeout);
			return list;
		}

		public override IEnumerable<T> BulkInsert(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			if (AutoKeyProperties.Length > 0) {
				TableData<T>.CreateTable(connection, BulkTempStagingTable, transaction, InsertColumns, commandTimeout);
				BulkInsert_(connection, objs, transaction, BulkTempStagingTable, InsertColumns, InsertProperties, commandTimeout);
				IEnumerable<T> values = connection.Query<T>(BulkInsertListQuery, null, transaction, true, commandTimeout);
				connection.Execute("DROP TABLE " + BulkTempStagingTable, null, transaction, commandTimeout);
				return values;
			}
			else {
				BulkInsert_(connection, objs, transaction, TableData<T>.TableName, InsertColumns, InsertProperties, commandTimeout);
				return objs;
			}
		}

		private void BulkInsert_(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction, string tableName, string[] columns, PropertyInfo[] properties, int? commandTimeout = null)
		{
			using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default | SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls, transaction)) {
				using (GenericDataReader<T> dataReader = new GenericDataReader<T>(objs, properties)) {
					bulkCopy.DestinationTableName = tableName ?? TableData<T>.TableName;
					bulkCopy.BulkCopyTimeout = commandTimeout ?? 0;
					for (int i = 0; i < columns.Length; i++) {
						bulkCopy.ColumnMappings.Add(properties[i].Name, columns[i]);
					}
					bulkCopy.WriteToServer(dataReader);
				}
			}
		}

		public override int BulkUpdate(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			TableData<T>.CreateTable(connection, BulkTempStagingTable, transaction, UpdateColumns, commandTimeout);
			BulkInsert_(connection, objs, transaction, BulkTempStagingTable, UpdateColumns, UpdateProperties, commandTimeout);
			int count = connection.Execute(BulkUpdateQuery, null, transaction, commandTimeout);
			connection.Execute("DROP TABLE " + BulkTempStagingTable, null, transaction, commandTimeout);
			return count;
		}

		public override IEnumerable<T> BulkUpsert(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			TableData<T>.CreateTable(connection, BulkTempStagingTable, transaction, Columns, commandTimeout);
			BulkInsert_(connection, objs, transaction, BulkTempStagingTable, Columns, Properties, commandTimeout);
			int count2 = connection.Execute(BulkUpdateQuery, null, transaction, commandTimeout);
			int count3 = connection.Execute(BulkInsertNotExistsQuery, null, transaction, commandTimeout);
			connection.Execute("DROP TABLE " + BulkTempStagingTable, null, transaction, commandTimeout);
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

		public override IEnumerable<KeyType> BulkDeleteList<KeyType>(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			List<KeyType> keys = new List<KeyType>();
			foreach (IEnumerable<KeyType> Keys in Partition<KeyType>(objs.Select(obj => TableData<T>.GetKey<KeyType>(obj)), 2000)) {
				keys.AddRange(DeleteList<KeyType>(connection, "WHERE [" + KeyColumns[0] + "] in @Keys", new { Keys }, transaction, buffered, commandTimeout));
			}
			return keys;
		}
		#endregion // ITableQueries<T>

		public override int RemoveDuplicates(IDbConnection connection, IDbTransaction transaction, int? commandTimeout = null)
		{
			if (KeyProperties.Length == 1) {
				try {
					return RemoveDuplicatesKey_(connection, KeyColumns[0], transaction, commandTimeout);
				}
				catch { }
			}
			else if (KeyProperties.Length == 0) {
				try {
					connection.Execute(@"ALTER TABLE " + TableName + " ADD _TempIDColumn INT IDENTITY(1,1)", null, transaction, commandTimeout);
					int count = RemoveDuplicatesKey_(connection, "_TempIDColumn", transaction, commandTimeout);
					connection.Execute(@"ALTER TABLE " + TableName + " DROP COLUMN _TempIDColumn", null, transaction, commandTimeout);
					return count;
				}
				catch { }
			}
			return RemoveDuplicates_(connection, transaction, commandTimeout);
		}

		private int RemoveDuplicates_(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			//connection.Execute("DROP TABLE " + BulkTempStagingTable, null, transaction, commandTimeout);
			string sql = "SELECT DISTINCT [" + string.Join("],[", GetColumnNames(Properties.Where(x => !KeyProperties.Contains(x)).ToArray())) + "]\nINTO " + BulkTempStagingTable + " FROM " + TableName;
			int currCount = RecordCount(connection, "", null, transaction, commandTimeout);
			connection.Execute(sql, null, transaction, commandTimeout);
			connection.Execute("TRUNCATE TABLE " + TableName, null, transaction, commandTimeout);
			int count = connection.Execute("INSERT INTO " + TableName + " SELECT * FROM " + BulkTempStagingTable, null, transaction, commandTimeout);
			connection.Execute("DROP TABLE " + BulkTempStagingTable, null, transaction, commandTimeout);
			return currCount - count;
		}

		private int RemoveDuplicatesKey_(IDbConnection connection, string keyColumn, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.Execute(@"WHILE EXISTS (SELECT COUNT(*) FROM [" + TableName + "] GROUP BY [" + keyColumn + "] HAVING COUNT(*) > 1" + @")
BEGIN
    DELETE FROM " + TableName + @" WHERE " + keyColumn + @" IN 
    (
        SELECT MIN([" + keyColumn + @"]) as [DeleteID]
        " + " FROM " + TableName + " GROUP BY [" + keyColumn + "] HAVING COUNT(*) > 1" + @"
	)
END", null, transaction, commandTimeout);
		}

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
