using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Utilities.Converters;

namespace Utilities
{
	/// <summary>
	/// Database utilities class.
	/// </summary>
	public static class Database
	{
		private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly;
		private const SqlBulkCopyOptions BulkCopyOptions = SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.UseInternalTransaction | SqlBulkCopyOptions.TableLock;
		private static readonly SqlBulkCopyColumnMapping[] EmptyColumnMappings = Array.Empty<SqlBulkCopyColumnMapping>();

		/// <summary>
		/// Creates a <see cref="SqlConnectionStringBuilder"/> based on the server/database.
		/// </summary>
		/// <param name="server">The server to connect to.</param>
		/// <param name="database">The initial database to connect to.</param>
		/// <param name="username">The username to log into the database. If null then integrated security is used.</param>
		/// <param name="password">The password to log into the server. If null then integrated security is used.</param>
		/// <param name="testConnection">Determines if the connection should be tested before being returned.</param>
		/// <param name="timeoutSecs">The maximum timeout in seconds for the connection.</param>
		/// <returns>The <see cref="SqlConnectionStringBuilder"/> or <see langword="null"/> if the connection failed.</returns>
		public static SqlConnectionStringBuilder ConnString(string server, string database, string username, string password, bool testConnection = false, int? commandTimeout = null)
		{
			////SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder("Integrated Security=SSPI;");
			SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder
			{
				DataSource = server,
				InitialCatalog = database,
				ConnectTimeout = commandTimeout ?? 15
			};
			if (username == null || password == null)
				sb.IntegratedSecurity = true;
			else {
				sb.UserID = username;
				sb.Password = password;
			}
			if (testConnection) {
				string connStr = sb.ConnectionString;
				using (SqlConnection conn = new SqlConnection(connStr)) {
					conn.Open();
				}
			}
			return sb;
		}

		/// <summary>
		/// Creates a <see cref="SqlConnectionStringBuilder"/> based on the server/database using integrated security.
		/// </summary>
		/// <param name="server">The server to connect to.</param>
		/// <param name="database">The initial database to connect to.</param>
		/// <param name="testConnection">Determines if the connection should be tested before being returned.</param>
		/// <param name="timeoutSecs">The maximum timeout in seconds for the connection.</param>
		/// <returns>The <see cref="SqlConnectionStringBuilder"/> or <see langword="null"/> if the connection failed.</returns>
		public static SqlConnectionStringBuilder ConnString(string server, string database, bool testConnection = false, int? commandTimeout = null)
		{
			return ConnString(server, database, null, null, testConnection, commandTimeout);
		}

		/// <summary>
		/// Returns an empty <see cref="DataTable"/> representing the database.
		/// </summary>
		/// <param name="conn">The database connection.</param>
		/// <param name="tablename">The name of the table.</param>
		/// <returns>An empty <see cref="DataTable"/> representing the database, or <see langword="null"/> on error.</returns>
		public static DataTable CreateDataTable(SqlConnection conn, string tablename, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			using (SqlCommand cmd = new SqlCommand($"SELECT * FROM {tablename} WHERE 0 = 1", conn, transaction))
			using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) {
				cmd.CommandTimeout = commandTimeout ?? 0;
				DataTable table = adapter.FillSchema(new DataTable(tablename), SchemaType.Source);
				table.TableName = tablename;
				return table;
			}
		}

		/// <summary>
		/// Returns a <see cref="SchemaTable"/> with information of an SQL query.
		/// </summary>
		/// <param name="conn">The database connection.</param>
		/// <param name="selectCmd">The select command.</param>
		/// <returns>A <see cref="SchemaTable"/> with the information of an SQL query, or <see langword="null"/> on error.</returns>
		public static SchemaTable SelectSchema(IDbConnection conn, string selectCmd, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			using (IDbCommand cmd = conn.CreateCommand()) {
				cmd.Transaction = transaction;
				cmd.CommandTimeout = commandTimeout ?? 0;
				using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly)) {
					DataTable tableSchema = reader.GetSchemaTable();
					return new SchemaTable(tableSchema);
				}
			}
		}

		/// <summary>
		/// Returns a <see cref="SchemaTable"/> with information of an SQL table.
		/// </summary>
		/// <param name="conn">The database connection.</param>
		/// <param name="tablename">The name of the table.</param>
		/// <returns>A <see cref="SchemaTable"/> with information of an SQL table, or <see langword="null"/> on error.</returns>
		public static SchemaTable TableSchema(SqlConnection conn, string tablename, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			return SelectSchema(conn, $"SELECT * FROM {tablename} WHERE 0 = 1", transaction, commandTimeout);
		}

		/// <summary>
		/// Gets a list of tables in a specified catalog.
		/// </summary>
		/// <param name="conn">The database connection.</param>
		/// <param name="catalog">The catalog to search. If this is <see langword="null"/> then the DataSource of the <see cref="SqlConnection"/> is used.</param>
		/// <returns>A list of tables in a specified catalog, or <see langword="null"/> on error.</returns>
		public static List<string> GetTableNames(SqlConnection conn, string catalog = null)
		{
			return GetObjectNames(conn, catalog, "BASE TABLE");
		}

		/// <summary>
		/// Gets a list of view names in a specified catalog.
		/// </summary>
		/// <param name="conn">The database connection.</param>
		/// <param name="catalog">The catalog to search. If this is <see langword="null"/> then the DataSource of the <see cref="SqlConnection"/> is used.</param>
		/// <returns>A list of view names in a specified catalog, or <see langword="null"/> on error.</returns>
		public static List<string> GetViewNames(SqlConnection conn, string catalog = null)
		{
			return GetObjectNames(conn, catalog, "VIEW");
		}

		private static List<string> GetObjectNames(SqlConnection conn, string catalog = null, string restriction3 = null)
		{
			string[] restrictions = new string[4];
			restrictions[0] = catalog;
			restrictions[3] = restriction3;
			using (DataTable table = conn.GetSchema("Tables", restrictions)) {
				List<string> tableNames = new List<string>();
				foreach (DataRow row in table.Rows) {
					tableNames.Add(string.Format("[{0}].[{1}].[{2}]", row[0], row[1], row[2]));
				}
				return tableNames;
			}
		}

		/// <summary>
		/// Deletes all rows from a database table.
		/// </summary>
		/// <param name="conn">The database connection.</param>
		/// <param name="tablename">The name of the table.</param>
		/// <param name="transaction">The database transaction.</param>
		/// <param name="commandTimeout">The maximum timeout in seconds for the command. A value of 0 means no timeout.</param>
		/// <returns>The number of rows that were deleted from the database.</returns>
		public static int DeleteTable(IDbConnection conn, string tablename, IDbTransaction transaction, int? commandTimeout = null)
		{
			return conn.Execute($"TRUNCATE TABLE {tablename}", null, transaction, commandTimeout);
		}

		/// <summary>
		/// Creates a command string that can be used to generate a table. It is imperfect because
		/// it doesn't copy the constraints/indexes.
		/// </summary>
		/// <param name="conn">The database connection.</param>
		/// <param name="tableName">The name of the table to create a clone script of.</param>
		/// <param name="newTableName">The name of the table to create. If this is null then the same name is used.</param>
		/// <returns>The SQL command to create a table with the name and schema of the input DataTable.</returns>
		public static string CloneTableScript(SqlConnection conn, string tableName, string newTableName = null)
		{
			return TableSchema(conn, tableName).CreateTableScript(newTableName ?? tableName);
		}

		/// <summary>
		/// Creates a command string that can be used to generate a table. This is imperfect
		/// as it creates an nvarchar in all cases for strings (never nchar(n)/varchar). It also
		/// doesn't copy the constraints/indexes.
		/// </summary>
		/// <param name="table">The table representation to create a command string from.</param>
		/// <param name="tableName">The name of the table to create. If null then the DataTable's name will be used.</param>
		/// <returns>The SQL command to create a table with the name and schema of the input DataTable.</returns>
		public static string CreateTableScript(DataTable table, string tableName = null)
		{
			StringBuilder sql = new StringBuilder();
			StringBuilder alterSql = new StringBuilder();

			tableName = tableName ?? table.TableName;
			sql.AppendFormat("CREATE TABLE [{0}] (", tableName);
			for (int i = 0; i < table.Columns.Count; i++) {
				sql.AppendFormat("\n\t[{0}] ", table.Columns[i].ColumnName);
				if (usesDefault.TryGetValue(table.Columns[i].DataType, out DefaultData data)) {
					if (data.HasMaxLength) {
						int maxlen = table.Columns[i].MaxLength;
						sql.AppendFormat(data.Name, maxlen <= 0 || maxlen > 4000 ? "MAX" : maxlen.ToString());
					}
					else
						sql.Append(data.Name);
				}
				else
					throw new InvalidOperationException("CreateTableString: invalid type '" + table.Columns[i].DataType.ToString() + "'");

				if (table.Columns[i].AutoIncrement) {
					sql.AppendFormat(
						" IDENTITY({0},{1})",
						table.Columns[i].AutoIncrementSeed,
						table.Columns[i].AutoIncrementStep);
				}
				else {
					// DataColumns will add a blank DefaultValue for any AutoIncrement column.
					// We only want to create an ALTER statement for those columns that are not set to AutoIncrement.
					DefaultValue(tableName, table.Columns[i], data.IsNumeric, alterSql);
				}
				if (!table.Columns[i].AllowDBNull)
					sql.Append(" NOT NULL");
				sql.Append(",");
			}
			sql.Remove(sql.Length - 1, 1);

			if (table.PrimaryKey.Length > 0) {
				sql.AppendFormat(
					",\n\tCONSTRAINT PK_{0} PRIMARY KEY ({1})",
					tableName + tableName.GetHashCode().ToString(),
					string.Join(",", table.PrimaryKey.Select(col => col.ColumnName)));
			}

			return sql.Append("\n);\n").Append(alterSql.ToString()).ToString();
		}

		private class DefaultData
		{
			public DefaultData(string name, bool hasMaxLen, bool isNumeric = false, bool hasDefault = true)
			{
				Name = name;
				HasDefault = hasDefault;
				IsNumeric = isNumeric;
				HasMaxLength = hasMaxLen;
			}
			public string Name { get; set; }
			public bool HasDefault { get; set; }
			public bool IsNumeric { get; set; }
			public bool HasMaxLength { get; set; }
		}

		private static Dictionary<Type, DefaultData> usesDefault = new Dictionary<Type, DefaultData>() {
			{ typeof(bool), new DefaultData("bit", false, true) },
			{ typeof(char), new DefaultData("nchar(1)", false, false) },
			{ typeof(byte), new DefaultData("tinyint", false, true) },
			{ typeof(sbyte), new DefaultData("tinyint", false, true) },
			{ typeof(short), new DefaultData("smallint", false, true) },
			{ typeof(ushort), new DefaultData("smallint", false, true) },
			{ typeof(int), new DefaultData("int", false, true) },
			{ typeof(uint), new DefaultData("int", false, true) },
			{ typeof(long), new DefaultData("bigint", false, true) },
			{ typeof(ulong), new DefaultData("bigint", false, true) },
			{ typeof(float), new DefaultData("float", false, true) },
			{ typeof(double), new DefaultData("float", false, true) },
			{ typeof(decimal), new DefaultData("decimal(18, 6)", false, true) },
			{ typeof(DateTime), new DefaultData("datetime2", false, false, false) },
			{ typeof(DateTimeOffset), new DefaultData("datetimeoffset", false, false, false) },
			{ typeof(byte[]), new DefaultData("varbinary({0})", true, false, false) },
			{ typeof(TimeSpan), new DefaultData("time", false, false, false) },
			{ typeof(string), new DefaultData("nvarchar({0})", true, false, true) },
		};

		private static void DefaultValue(string tablename, DataColumn col, bool isNumeric, StringBuilder alterSql)
		{
			if (!col.AutoIncrement && col.DefaultValue != null) {
				string defaultVal = col.DefaultValue.ToString();
				alterSql.AppendFormat(
					"\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
					tablename,
					col.ColumnName,
					isNumeric ? defaultVal : "'" + defaultVal + "'");
			}
			else {
				// Default values on Date columns, e.g., "DateTime.Now" will not translate to SQL.
				// This inspects the caption for a simple XML string to see if there is a SQL compliant default value, e.g., "GETDATE()".
				try {
					System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
					xml.LoadXml(col.Caption);
					alterSql.AppendFormat(
						"\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
						tablename,
						col.ColumnName,
						xml.GetElementsByTagName("defaultValue")[0].InnerText);
				}
				catch {
					// ignore
				}
			}
		}

		/// <summary>
		/// Drops a table from a database.
		/// </summary>
		/// <param name="conn">The database connection.</param>
		/// <param name="tablename">The name of the table.</param>
		/// <param name="transaction">The database transaction.</param>
		/// <param name="commandTimeout"></param>
		/// <returns>True if the table was dropped. False otherwise.</returns>
		public static void DropTable(IDbConnection conn, string tablename, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			conn.Execute($"DROP TABLE {tablename}", null, transaction, commandTimeout);
		}

		/// <summary>
		/// Determines if a table exists.
		/// </summary>
		/// <param name="conn">The database connection.</param>
		/// <param name="tablename">The name of the table.</param>
		/// <param name="transaction">The database transaction.</param>
		/// <param name="commandTimeout"></param>
		/// <returns>True if the table exists. False otherwise.</returns>
		public static bool TableExists(IDbConnection conn, string tablename, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			try {
				// ANSI SQL way.  Works in PostgreSQL, MSSQL, MySQL.
				return 1 == conn.ExecuteScalar<int>(
$@"IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = {tablename}
)
SELECT 1 ELSE SELECT 0", null, transaction, commandTimeout);
			}
			catch {
				try {
					// Other RDBMS.  Graceful degradation
					return 1 == conn.ExecuteScalar<int>($"SELECT 1 FROM {tablename} WHERE 1 = 0", null, transaction, commandTimeout);
				}
				catch {
					// ignore
				}
			}
			return false;
		}
	}
}