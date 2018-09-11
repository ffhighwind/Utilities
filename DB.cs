using System;
using System.Collections.Generic;
using System.Data;
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
    public static class DB
    {
        private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        private const SqlBulkCopyOptions BulkCopyOptions = SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.UseInternalTransaction | SqlBulkCopyOptions.TableLock;
        private static readonly SqlBulkCopyColumnMapping[] EmptyColumnMappings = Array.Empty<SqlBulkCopyColumnMapping>();

        /// <summary>
        /// Creates a connection string based on the server/database.
        /// </summary>
        /// <param name="server">The server to connect to.</param>
        /// <param name="database">The initial database to connect to.</param>
        /// <param name="username">The username to log into the database. If null then integrated security is used.</param>
        /// <param name="password">The password to log into the server. If null then integrated security is used.</param>
        /// <param name="testConnection">Determines if the connection should be tested before being returned.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the connection.</param>
        /// <returns>The connection string builder or null if the connection failed.</returns>
        public static SqlConnectionStringBuilder ConnString(string server, string database, string username, string password, bool testConnection = false, int timeoutSecs = 15)
        {
            ////SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder("Integrated Security=SSPI;");
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder {
                DataSource = server,
                InitialCatalog = database,
                ConnectTimeout = timeoutSecs
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
        /// Creates a connection string based on the server/database using integrated security.
        /// </summary>
        /// <param name="server">The server to connect to.</param>
        /// <param name="database">The initial database to connect to.</param>
        /// <param name="testConnection">Determines if the connection should be tested before being returned.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the connection.</param>
        /// <returns>The connection string builder or null if the connection failed.</returns>
        public static SqlConnectionStringBuilder ConnString(string server, string database, bool testConnection = false, int timeoutSecs = 15)
        {
            return ConnString(server, database, null, null, testConnection, timeoutSecs);
        }

        /// <summary>
        /// Returns an empty table representing the database.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <returns>An empty table representing the database, or null on error.</returns>
        public static DataTable CreateDataTable(SqlConnection conn, string tablename)
        {
            using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT TOP 0 * FROM " + tablename, conn)) {
                DataTable table = adapter.FillSchema(new DataTable(tablename), SchemaType.Source);
                table.TableName = tablename;
                return table;
            }
        }

        /// <summary>
        /// Returns a DataTable with schema information of an SQL table returned by a select command.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="selectCmd">The select command.</param>
        /// <returns>A DataTable with the schema information of an SQL table, or null on error.</returns>
        public static SchemaTable SelectSchema(SqlConnection conn, string selectCmd)
        {
            using (SqlCommand cmd = new SqlCommand(selectCmd, conn))
            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly)) {
                DataTable tableSchema = reader.GetSchemaTable();
                return new SchemaTable(tableSchema);
            }
        }

        /// <summary>
        /// Returns a DataTable with schema information of an SQL table.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <returns>A DataTable with the schema information of an SQL table, or null on error.</returns>
        public static SchemaTable TableSchema(SqlConnection conn, string tablename)
        {
            return SelectSchema(conn, "SELECT TOP 0 * FROM " + tablename);
        }

        /// <summary>
        /// Gets a list of table names.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="catalog">The catalog to search. Null means the DataSource of the connection is used.</param>
        /// <returns>A list of table names, or null on error.</returns>
        public static List<string> GetTableNames(SqlConnection conn, string catalog = null)
        {
            return GetObjectNames(conn, catalog, "BASE TABLE");
        }

        /// <summary>
        /// Gets a list of view names.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="catalog">The catalog to search. Null means the DataSource of the connection is used.</param>
        /// <returns>A list of view names, or null on error.</returns>
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
        /// Executes an SQL query.
        /// </summary>
        /// <typeparam name="T">The type of object to return from the query.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="cmd">The command to execute.</param>
        /// <param name="param">The parameters to pass to the command.</param>
        /// <param name="timeoutSecs">The timeout in seconds for the command. A value of 0 means no timeout.</param>
        /// <param name="maxRetries">The number of attempts to retry the command.</param>
        /// <returns>The results from the query, or null on error.</returns>
        public static IEnumerable<T> Query<T>(SqlConnection conn, string cmd, object param = null, int timeoutSecs = 0, int maxRetries = 5)
        {
            for (int i = 0; i < maxRetries; i++) {
                try {
                    return conn.Query<T>(cmd, param, null, true, timeoutSecs);
                }
                catch {
                    if (i == maxRetries - 1)
                        throw; // keeps StackTrace
                }
            }
            throw new ArgumentOutOfRangeException("maxRetries: " + maxRetries);
        }

        /// <summary>
        /// Executes an SQL query asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of object to return from the query.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="cmd">The command to execute.</param>
        /// <param name="param">The parameters to pass to the command.</param>
        /// <param name="timeoutSecs">The timeout in seconds for the command. A value of 0 means no timeout.</param>
        /// <param name="maxRetries">The number of attempts to retry the command.</param>
        /// <returns>The results from the query, or null on error.</returns>
        public static Task<IEnumerable<T>> QueryAsync<T>(SqlConnection conn, string cmd, object param = null, int timeoutSecs = 0, int maxRetries = 5)
        {
            return Task.Run(() => {
                return Query<T>(conn, cmd, param, timeoutSecs, maxRetries);
            });
        }

        /// <summary>
        /// Executes an SQL query.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="cmd">The command to execute.</param>
        /// <param name="param">The parameters and values.</param>
        /// <param name="timeoutSecs">The command timeout in seconds. A value of 0 means no timeout.</param>
        /// <param name="maxRetries">The maximum number of attempts to retry the command on failure.</param>
        /// <returns>The results from the query, or null on error.</returns>
        public static DataTable Query(SqlConnection conn, string cmd, object param = null, int timeoutSecs = 0, int maxRetries = 5)
        {
            for (int i = 0; i < maxRetries; i++) {
                try {
                    using (IDataReader reader = conn.ExecuteReader(cmd, param, null, timeoutSecs)) {
                        DataTable table = new DataTable();
                        table.Load(reader);
                        return table;
                    }
                }
                catch {
                    if (i == maxRetries - 1)
                        throw; // keeps StackTrace
                }
            }
            throw new ArgumentOutOfRangeException("maxRetries: " + maxRetries);
        }

        /// <summary>
        /// Executes an SQL query asynchronously.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="cmd">The command to execute.</param>
        /// <param name="param">The parameters and values.</param>
        /// <param name="timeoutSecs">The command timeout in seconds. A value of 0 means no timeout.</param>
        /// <param name="maxRetries">The maximum number of attempts to retry the command on failure.</param>
        /// <returns>The results from the query, or null on error.</returns>
        public static Task<DataTable> QueryAsync(SqlConnection conn, string cmd, object param = null, int timeoutSecs = 0, int maxRetries = 5)
        {
            return Task.Run(() => {
                return Query(conn, cmd, param, timeoutSecs, maxRetries);
            });
        }

        /// <summary>
        /// Executes an SQL command.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="cmd">The command to execute.</param>
        /// <param name="param">The parameters and values.</param>
        /// <param name="timeoutSecs">The command timeout in seconds. A value of 0 means no timeout.</param>
        /// <param name="maxRetries">The maximum number of attempts to retry the command on failure.</param>
        /// <returns>The number of rows affected, or -1 on error.</returns>
        public static int Execute(SqlConnection conn, string cmd, object param = null, int timeoutSecs = 0, int maxRetries = 5)
        {
            for (int i = 0; i < maxRetries; i++) {
                try {
                    using (SqlTransaction trans = conn.BeginTransaction()) {
                        int count = conn.Execute(cmd, param, trans, timeoutSecs);
                        if (count > 0)
                            trans.Commit();
                        return count;
                    }
                }
                catch {
                    if (i == maxRetries - 1)
                        throw; // keeps StackTrace
                }
            }
            throw new ArgumentOutOfRangeException("maxRetries: " + maxRetries);
        }

        /// <summary>
        /// Executes an SQL command asynchronously.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="cmd">The command to execute.</param>
        /// <param name="param">The parameters and values.</param>
        /// <param name="timeoutSecs">The command timeout in seconds. A value of 0 means no timeout.</param>
        /// <param name="maxRetries">The maximum number of attempts to retry the command on failure.</param>
        /// <returns>The number of rows affected, or -1 on error.</returns>
        public static Task<int> ExecuteAsync(SqlConnection conn, string cmd, object param = null, int timeoutSecs = 0, int maxRetries = 5)
        {
            return Task.Run(() => {
                return Execute(conn, cmd, param, timeoutSecs, maxRetries);
            });
        }

        /// <summary>
        /// Uploads a collection to a database.
        /// </summary>
        /// <typeparam name="T">The type of object in the collection.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the database table to upload data to.</param>
        /// <param name="list">The List to upload.</param>
        /// <param name="columns">The column names.</param>
        public static void BulkInsert<T>(SqlConnection conn, string tablename, IEnumerable<T> list, params string[] columns) where T : class
        {
            BulkInsert<T>(conn, tablename, list, 0, CreateMappings(columns));
        }

        /// <summary>
        /// Uploads a collection to a database.
        /// </summary>
        /// <typeparam name="T">The type of object in the collection.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the database table to upload data to.</param>
        /// <param name="list">The List to upload.</param>
        /// <param name="timeoutSecs">The timeout in seconds for the command. A value of 0 means no timeout.</param>
        /// <param name="columns">The columns names. If no columns are given then all columns are mapped automatically by name using reflection.</param>
        public static void BulkInsert<T>(SqlConnection conn, string tablename, IEnumerable<T> list, int timeoutSecs, params string[] columns) where T : class
        {
            BulkInsert<T>(conn, tablename, list, timeoutSecs, CreateMappings(columns));
        }

        /// <summary>
        /// Uploads a collection to a database.
        /// </summary>
        /// <typeparam name="T">The type of object in the collection.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the database table to upload data to.</param>
        /// <param name="list">The List to upload.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command.</param>
        /// <param name="mappings">The column mappings.</param>
        public static void BulkInsert<T>(SqlConnection conn, string tablename, IEnumerable<T> list, int timeoutSecs = 0, params SqlBulkCopyColumnMapping[] mappings) where T : class
        {
            using (GenericDataReader<T> reader = new GenericDataReader<T>(list))
            using (SqlBulkCopy bulkCpy = new SqlBulkCopy(conn, BulkCopyOptions, null)) {
                bulkCpy.DestinationTableName = tablename;
                bulkCpy.BulkCopyTimeout = timeoutSecs;
                if (mappings.Length == 0)
                    mappings = CreateMappings<T>();
                foreach (SqlBulkCopyColumnMapping mapping in mappings) {
                    bulkCpy.ColumnMappings.Add(mapping);
                }
                bulkCpy.WriteToServer(reader);
            }
        }

        private static SqlBulkCopyColumnMapping[] CreateMappings(params string[] columns)
        {
            if (columns.Length == 0)
                return EmptyColumnMappings;

            SqlBulkCopyColumnMapping[] mappings = new SqlBulkCopyColumnMapping[columns.Length];
            for (int i = 0; i < columns.Length; i++) {
                mappings[i] = new SqlBulkCopyColumnMapping(columns[i], columns[i]);
            }
            return mappings;
        }

        private static SqlBulkCopyColumnMapping[] CreateMappings<T>()
        {
            PropertyInfo[] pinfos = typeof(T).GetProperties(DefaultBindingFlags);
            SqlBulkCopyColumnMapping[] mappings = new SqlBulkCopyColumnMapping[pinfos.Length];
            for (int i = 0; i < pinfos.Length; i++) {
                mappings[i] = new SqlBulkCopyColumnMapping(pinfos[i].Name, pinfos[i].Name);
            }
            return mappings;
        }

        private static SqlBulkCopyColumnMapping[] CreateMappings(DataTable table)
        {
            SqlBulkCopyColumnMapping[] mappings = new SqlBulkCopyColumnMapping[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++) {
                mappings[i] = new SqlBulkCopyColumnMapping(table.Columns[i].ColumnName, table.Columns[i].ColumnName);
            }
            return mappings;
        }

        /// <summary>
        /// Uploads a DataTable to a database.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="table">The DataTable to upload.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command. A value of 0 means no timeout.</param>
        /// <param name="mappings">The column mappings. If no mappings are given then all columns are mapped.</param>
        public static void BulkInsert(SqlConnection conn, DataTable table, int timeoutSecs = 0, params SqlBulkCopyColumnMapping[] mappings)
        {
            using (SqlBulkCopy bulkCpy = new SqlBulkCopy(conn, BulkCopyOptions, null)) {
                bulkCpy.DestinationTableName = table.TableName;
                bulkCpy.BulkCopyTimeout = timeoutSecs;
                if (mappings.Length == 0)
                    mappings = CreateMappings(table);
                foreach (SqlBulkCopyColumnMapping mapping in mappings) {
                    bulkCpy.ColumnMappings.Add(mapping);
                }
                bulkCpy.WriteToServer(table);
            }
        }

        /// <summary>
        /// Inserts and optionally updates rows in a database.
        /// </summary>
        /// <typeparam name="T">The Type of object to upload.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the database table to upload data to.</param>
        /// <param name="list">The List to upload.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command. A value of 0 means no timeout.</param>
        /// <param name="update">Determines if duplicate rows should be updated.</param>
        /// <param name="mappings">The column mappings.</param>
        public static void BulkUpsert<T>(SqlConnection conn, string tablename, IEnumerable<T> list, int timeoutSecs = 0, bool update = true, params SqlBulkCopyColumnMapping[] mappings) where T : class
        {
            if (!list.Any())
                return;
            if (mappings.Length == 0)
                mappings = CreateMappings<T>();
            using (GenericDataReader<T> reader = new GenericDataReader<T>(list)) {
                BulkUpsert(conn, tablename, (bc) => bc.WriteToServer(reader), timeoutSecs, update, mappings);
            }
        }

        /// <summary>
        /// Inserts and optionally updates rows in a database.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="table">The DataTable to upload.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command. A value of 0 means no timeout.</param>
        /// <param name="update">Determines if duplicate rows should be updated.</param>
        /// <param name="mappings">The column mappings.</param>
        public static void BulkUpsert(SqlConnection conn, DataTable table, int timeoutSecs = 0, bool update = true, params SqlBulkCopyColumnMapping[] mappings)
        {
            if (table.Rows.Count == 0)
                return;
            if (mappings.Length == 0)
                mappings = CreateMappings(table);
            BulkUpsert(conn, table.TableName, (bc) => bc.WriteToServer(table), timeoutSecs, update, mappings);
        }

        /// <summary>
        /// Inserts and optionally updates rows in a database.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the database table to upload data to.</param>
        /// <param name="writeAction">A function for bulk uploading to the temporary table.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command. A value of 0 means no timeout.</param>
        /// <param name="update">Determines if duplicate rows should be updated.</param>
        /// <param name="mappings">The column mappings.</param>
        private static void BulkUpsert(SqlConnection conn, string tablename, Action<SqlBulkCopy> writeAction, int timeoutSecs, bool update, SqlBulkCopyColumnMapping[] mappings)
        {
            using (SqlTransaction trans = conn.BeginTransaction())
            using (SqlCommand cmd = conn.CreateCommand()) {
                string tempTableName = "#Tmp_" + tablename;
                cmd.Transaction = trans;
                cmd.CommandText = @"SELECT * INTO " + tablename + " FROM " + tempTableName + " WHERE 1 = 0";
                if (cmd.ExecuteNonQuery() < 1)
                    return;

                using (SqlBulkCopy bulkCpy = new SqlBulkCopy(conn, SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.TableLock, trans)) {
                    bulkCpy.DestinationTableName = tempTableName;
                    bulkCpy.BulkCopyTimeout = timeoutSecs;
                    foreach (SqlBulkCopyColumnMapping mapping in mappings) {
                        bulkCpy.ColumnMappings.Add(mapping);
                    }
                    writeAction(bulkCpy);
                }
                SqlBulkCopyColumnMapping[] bulkMappings = new SqlBulkCopyColumnMapping[mappings.Length];
                for (int i = 0; i < bulkMappings.Length; i++) {
                    bulkMappings[i] = new SqlBulkCopyColumnMapping(mappings[i].DestinationColumn, mappings[i].DestinationColumn);
                }
                if (MergeTables(conn, tablename, tempTableName, trans, update, bulkMappings) >= 0) {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "DROP TABLE " + tempTableName;
                    if (cmd.ExecuteNonQuery() >= 0)
                        trans.Commit();
                }
            }
        }

        /// <summary>
        /// Merges two tables together using the column mappings.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="source">The source table.</param>
        /// <param name="target">The destination table.</param>
        /// <param name="update">Determines if duplicate rows should be updated.</param>
        /// <param name="mappings">The column mappings between the tables.</param>
        /// <returns>The number of rows affected by the merge command.</returns>
        public static int MergeTables(SqlConnection conn, string source, string target, bool update, SqlBulkCopyColumnMapping[] mappings)
        {
            using (SqlTransaction trans = conn.BeginTransaction()) {
                int result = MergeTables(conn, source, target, trans, update, mappings);
                if (result >= 0)
                    trans.Commit();
                return result;
            }
        }

        /// <summary>
        /// Merges two tables together using the column mappings.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="source">The source table.</param>
        /// <param name="target">The destination table.</param>
        /// /<param name="trans">The database transaction.</param>
        /// <param name="update">Determines if duplicate rows should be updated.</param>
        /// <param name="mappings">The column mappings between the tables.</param>
        /// <returns>The number of rows affected by the merge command.</returns>
        public static int MergeTables(SqlConnection conn, string source, string target, SqlTransaction trans, bool update, SqlBulkCopyColumnMapping[] mappings)
        {
            using (SqlCommand cmd = conn.CreateCommand()) {
                cmd.Transaction = trans;
                StringBuilder onStringBuilder = new StringBuilder("MERGE INTO ").Append(target)
                    .Append(" AS Target\nUSING ").Append(source).Append(" AS Source\n\tON");
                StringBuilder updateStringBuilder = new StringBuilder("WHEN MATCHED THEN UPDATE SET\n\t");
                StringBuilder insertStringBuilder = new StringBuilder("WHEN NOT MATCHED THEN\tINSERT (");
                StringBuilder valuesStringBuilder = new StringBuilder("\tVALUES (");
                for (int i = 0; i < mappings.Length; i++) {
                    string targetP = "@Target" + (i + 1).ToString();
                    string sourceP = "@Source" + (i + 1).ToString();
                    cmd.Parameters.AddWithValue(targetP, mappings[i].DestinationColumn);
                    cmd.Parameters.AddWithValue(sourceP, mappings[i].SourceColumn);
                    onStringBuilder.Append(" Target.[").Append(targetP).Append("] = Source.[").Append(sourceP).Append("]\n\tAND");
                    updateStringBuilder.Append(" Target.[").Append(targetP).Append("] = Source.").Append(sourceP).Append(",\n\t");
                    insertStringBuilder.Append(targetP).Append(", ");
                    valuesStringBuilder.Append(" Source.[").Append(sourceP).Append("], ");
                }
                onStringBuilder.Remove(updateStringBuilder.Length - 4, 4);
                updateStringBuilder.Remove(updateStringBuilder.Length - 3, 3);
                insertStringBuilder.Remove(updateStringBuilder.Length - 2, 2).Append(")\n");
                valuesStringBuilder.Remove(updateStringBuilder.Length - 2, 2).Append(")");
                if (update)
                    onStringBuilder.Append(updateStringBuilder);
                cmd.CommandText = onStringBuilder.Append(insertStringBuilder).Append(valuesStringBuilder).ToString();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Deletes all rows from a database table.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command. A value of 0 means no timeout.</param>
        /// <returns>The number of rows that were deleted from the database.</returns>
        public static int DeleteTable(SqlConnection conn, string tablename, int timeoutSecs = 0)
        {
            using (SqlTransaction trans = conn.BeginTransaction()) {
                int result = conn.Execute("DELETE FROM " + tablename, null, trans, timeoutSecs);
                if (result > 0)
                    trans.Commit();
                return result;
            }
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
                    tableName,
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
                    // Handle
                }
            }
        }

        /// <summary>
        /// Drops a table from a database.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <returns>True if the table was dropped. False otherwise.</returns>
        public static bool DropTable(SqlConnection conn, string tablename)
        {
            conn.Execute("DROP TABLE " + tablename);
            return true;
        }

        /// <summary>
        /// Determines if a table exists.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <returns>True if the table exists. False otherwise.</returns>
        public static bool TableExists(SqlConnection conn, string tablename)
        {
            try {
                // ANSI SQL way.  Works in PostgreSQL, MSSQL, MySQL.
                return 1 == conn.ExecuteScalar<int>(
@"IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = @tablename
)
SELECT 1 ELSE SELECT 0", new { tablename = tablename });
            }
            catch (Exception ex) {
                try {
                    // Other RDBMS.  Graceful degradation
                    return 1 == conn.ExecuteScalar<int>("SELECT 1 FROM @tablename WHERE 1 = 0", new { tablename = tablename });
                }
                catch {
                    PrintError(ex, "DB.TableExists", conn, tablename);
                }
            }
            return false;
        }

        /// <summary>
        /// Removes duplicate rows from a database.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table to remove duplicates from.</param>
        /// <param name="distinctColumns">The columns to select on to check for duplicates.
        /// If no columns are input them all columns are used.</param>
        /// <returns>The number of rows affected by the query.</returns>
        public static int RemoveDuplicates(SqlConnection conn, string tablename, params string[] distinctColumns)
        {
            using (SqlTransaction trans = conn.BeginTransaction()) {
                return conn.Execute(
@"WHILE 1=1
BEGIN
WITH CTE AS (
    SELECT ROW_NUMBER() OVER (PARTITION BY @columns ORDER BY (SELECT NULL)) AS DuplicateRows
    FROM @tablename
)
DELETE TOP (4000)
FROM @tablename
WHERE DuplicateRows > 1
IF @@ROWCOUNT < 4000
    BREAK;
END", new { tablename = tablename, columns = distinctColumns.Length > 0 ? string.Join(",", distinctColumns) : "*" }, trans);
            }
        }

        private static void PrintError(Exception ex, string methodName, SqlConnection conn, string tablename = null)
        {
            string arg = string.Format("{0}.{1}{2}", conn.DataSource ?? "null", conn.Database ?? "null", tablename == null ? "" : "." + tablename);
            Console.Error.WriteLine(
                "Error {0}{1}: {2}\n{3}{4}",
                methodName ?? "null",
                arg == null ? "" : "(" + arg + ")",
                ex.GetType().ToString(),
                ex.Message,
                ex.InnerException == null ? "" : "\n  " + ex.InnerException.Message);
        }
    }
}