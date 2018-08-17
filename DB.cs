using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;


namespace Utilities
{
    public static class DB
    {
        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance;
        private const SqlBulkCopyOptions bulkCopyOptions = SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.UseInternalTransaction | SqlBulkCopyOptions.TableLock;
        private static readonly SqlBulkCopyColumnMapping[] emptyColumnMappings = Array.Empty<SqlBulkCopyColumnMapping>();

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
            //SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder("Integrated Security=SSPI;");
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder {
                DataSource = server,
                InitialCatalog = database,
                ConnectTimeout = timeoutSecs
            };
            if (username == null || password == null) {
                sb.IntegratedSecurity = true;
            }
            else {
                sb.UserID = username;
                sb.Password = password;
            }
            if (testConnection) {
                string connStr = sb.ConnectionString;
                try {
                    using (SqlConnection conn = new SqlConnection(connStr)) {
                        conn.Open();
                    }
                }
                catch (Exception ex) {
                    PrintError(ex, "DB.ConnString", (server ?? "null") + "." + (database ?? "null"));
                    return null;
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
            try {
                DataTable dt;
                conn.Open();
                using (var adapter = new SqlDataAdapter("SELECT TOP 0 * FROM [" + tablename + "]", conn)) {
                    dt = new DataTable();
                    adapter.FillSchema(dt, SchemaType.Source);
                }
                dt.TableName = tablename;
                return dt;
            }
            catch (Exception ex) {
                PrintError(ex, "DB.TableSchema", conn, tablename);
            }
            return null;
        }

        /// <summary>
        /// Returns a DataTable with schema information of an SQL table.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <returns>A DataTable with the schema information of an SQL table, or null on error.</returns>
        public static SchemaTable SelectSchema(SqlConnection conn, string selectCmd)
        {
            try {
                DataTable tableSchema;
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(selectCmd, conn))
                using (var reader = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly)) {
                    tableSchema = reader.GetSchemaTable();
                }
                return new SchemaTable(tableSchema);
            }
            catch (Exception ex) {
                PrintError(ex, "DB.SelectSchema", conn, selectCmd);
            }
            return null;
        }

        /// <summary>
        /// Returns a DataTable with schema information of an SQL table.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <returns>A DataTable with the schema information of an SQL table, or null on error.</returns>
        public static SchemaTable TableSchema(SqlConnection conn, string tablename)
        {
            //try {
            DataTable tableSchema;
            conn.Open();
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 0 * FROM " + tablename, conn))
            using (var reader = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly)) {
                tableSchema = reader.GetSchemaTable();
            }
            conn.Close();
            return new SchemaTable(tableSchema);
            //}
            //catch (Exception ex) {
            //    PrintError(ex, "DB.TableSchema", conn, tablename);
            //}
            //return null;
        }

        /// <summary>
        /// Gets a list of table names.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="catalog">The catalog to search. Null means the DataSource of the connection is used.</param>
        /// <returns>A list of table names, or null on error.</returns>
        public static List<string> GetTableNames(SqlConnection conn, string catalog = null)
        {
            try {
                return GetObjectNames(conn, catalog, "BASE TABLE");
            }
            catch (Exception ex) {
                PrintError(ex, "DB.GetTableNames", conn, null);
            }
            return null;
        }

        /// <summary>
        /// Gets a list of view names.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="catalog">The catalog to search. Null means the DataSource of the connection is used.</param>
        /// <returns>A list of view names, or null on error.</returns>
        public static List<string> GetViewNames(SqlConnection conn, string catalog = null)
        {
            try {
                return GetObjectNames(conn, catalog, "VIEW");
            }
            catch (Exception ex) {
                PrintError(ex, "DB.GetViewNames", conn, null);
            }
            return null;
        }

        private static List<string> GetObjectNames(SqlConnection conn, string catalog = null, string restriction3 = null)
        {
            string[] restrictions = new string[4];
            restrictions[0] = catalog;
            restrictions[3] = restriction3;
            conn.Open();
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
            Exception e = null;
            for (int i = 0; i < maxRetries; i++) {
                try {
                    return conn.Query<T>(cmd, param, null, true, timeoutSecs);
                }
                catch (Exception ex) {
                    e = ex;
                }
            }
            PrintError(e, "DB.Query", conn);
            return null;
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
            Exception e = null;
            for (int i = 0; i < maxRetries; i++) {
                try {
                    using (IDataReader reader = conn.ExecuteReader(cmd, param, null, timeoutSecs)) {
                        DataTable table = new DataTable();
                        table.Load(reader);
                        return table;
                    }
                }
                catch (Exception ex) {
                    e = ex;
                }
            }
            PrintError(e, "DB.Query", conn);
            return null;
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
            Exception e = null;
            for (int i = 0; i < maxRetries; i++) {
                try {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction()) {
                        int count = conn.Execute(cmd, param, trans, timeoutSecs);
                        if (count > 0)
                            trans.Commit();
                        return count;
                    }
                }
                catch (Exception ex) {
                    e = ex;
                }
            }
            PrintError(e, "DB.Execute", conn);
            return -1;
        }

        /// <summary>
        /// Uploads a collection to a database.
        /// </summary>
        /// <typeparam name="T">The type of object in the collection.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename"></param>
        /// <param name="list">The List to upload.</param>
        /// <param name="columns">The column names.</param>
        /// <returns>True if the upload was successful. False otherwise.</returns>
        public static bool BulkUpload<T>(SqlConnection conn, string tablename, IEnumerable<T> list, params string[] columns) where T : class
        {
            return BulkInsert<T>(conn, tablename, list, 0, CreateMappings(columns));
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
        /// <returns>True if the upload was successful. False otherwise.</returns>
        public static bool BulkUpload<T>(SqlConnection conn, string tablename, IEnumerable<T> list, int timeoutSecs, params string[] columns) where T : class
        {
            return BulkInsert<T>(conn, tablename, list, timeoutSecs, CreateMappings(columns));
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
        /// <returns>True if the upload was successful. False otherwise.</returns>
        public static bool BulkInsert<T>(SqlConnection conn, string tablename, IEnumerable<T> list, int timeoutSecs = 0, params SqlBulkCopyColumnMapping[] mappings) where T : class
        {
            try {
                using (GenericDataReader<T> reader = new GenericDataReader<T>(list))
                using (SqlBulkCopy bulkCpy = new SqlBulkCopy(conn, bulkCopyOptions, null)) {
                    bulkCpy.DestinationTableName = tablename;
                    bulkCpy.BulkCopyTimeout = timeoutSecs;
                    if (mappings.Length == 0)
                        mappings = CreateMappings<T>();
                    foreach (SqlBulkCopyColumnMapping mapping in mappings) {
                        bulkCpy.ColumnMappings.Add(mapping);
                    }
                    conn.Open();
                    bulkCpy.WriteToServer(reader);
                }
                return true;
            }
            catch (Exception ex) {
                PrintError(ex, "DB.BulkInsert", conn, tablename + " " + list.Count().ToString() + " " + typeof(T).Name);
            }
            return false;
        }

        private static SqlBulkCopyColumnMapping[] CreateMappings(params string[] columns)
        {
            if (columns.Length == 0)
                return emptyColumnMappings;

            SqlBulkCopyColumnMapping[] mappings = new SqlBulkCopyColumnMapping[columns.Length];
            for (int i = 0; i < columns.Length; i++) {
                mappings[i] = new SqlBulkCopyColumnMapping(columns[i], columns[i]);
            }
            return mappings;
        }

        private static SqlBulkCopyColumnMapping[] CreateMappings<T>(params string[] columns)
        {
            SqlBulkCopyColumnMapping[] mappings;
            if (columns.Length == 0)
                mappings = CreateMappings<T>();
            else {
                mappings = new SqlBulkCopyColumnMapping[columns.Length];
                for (int i = 0; i < columns.Length; i++) {
                    mappings[i] = new SqlBulkCopyColumnMapping(columns[i], columns[i]);
                }
            }
            return mappings;
        }

        private static SqlBulkCopyColumnMapping[] CreateMappings<T>()
        {
            PropertyInfo[] pinfos = typeof(T).GetProperties(bindingFlags);
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
        /// <param name="options">The bulk copy options. If you do not wish to lock the database table then you will need to change this.</param>
        /// <param name="mappings">The column mappings. If no mappings are given then all columns are mapped.</param>
        /// <returns>True if the upload was successful, false otherwise.</returns>
        public static bool BulkInsert(SqlConnection conn, DataTable table, int timeoutSecs = 0, params SqlBulkCopyColumnMapping[] mappings)
        {
            try {
                using (SqlBulkCopy bulkCpy = new SqlBulkCopy(conn, bulkCopyOptions, null)) {
                    bulkCpy.DestinationTableName = table.TableName;
                    bulkCpy.BulkCopyTimeout = timeoutSecs;
                    if (mappings.Length == 0)
                        mappings = CreateMappings(table);
                    foreach (SqlBulkCopyColumnMapping mapping in mappings) {
                        bulkCpy.ColumnMappings.Add(mapping);
                    }
                    conn.Open();
                    bulkCpy.WriteToServer(table);
                    return true;
                }
            }
            catch (Exception ex) {
                PrintError(ex, "DB.BulkInsert", conn, table.TableName);
            }
            return false;
        }

        /// <summary>
        /// Inserts and optionally updates rows in a database.
        /// </summary>
        /// <typeparam name="T">The Type of object to upload.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the database table to upload data to.</param>
        /// <param name="list">The List to upload.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command. A value of 0 means no timeout.</param>
        /// <param name="mappings">The column mappings.</param>
        /// <param name="update">Determines if duplicate rows should be updated.</param>
        /// <returns>True if the upload was successful. False otherwise.</returns>
        public static bool BulkUpsert<T>(SqlConnection conn, string tablename, IEnumerable<T> list, int timeoutSecs = 0, bool update = true, params SqlBulkCopyColumnMapping[] mappings) where T : class
        {
            if (!list.Any())
                return true;
            if (mappings.Length == 0)
                mappings = CreateMappings<T>();
            using (GenericDataReader<T> reader = new GenericDataReader<T>(list)) {
                return BulkUpsert(conn, tablename, (bc) => bc.WriteToServer(reader), timeoutSecs, update, mappings);
            }
        }

        /// <summary>
        /// Inserts and optionally updates rows in a database.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="table">The DataTable to upload.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command. A value of 0 means no timeout.</param>
        /// <param name="mappings">The column mappings.</param>
        /// <param name="update">Determines if duplicate rows should be updated.</param>
        /// <returns>True if the upload was successful. False otherwise.</returns>
        public static bool BulkUpsert(SqlConnection conn, DataTable table, int timeoutSecs = 0, bool update = true, params SqlBulkCopyColumnMapping[] mappings)
        {
            if (table.Rows.Count == 0)
                return true;
            if (mappings.Length == 0)
                mappings = CreateMappings(table);
            return BulkUpsert(conn, table.TableName, (bc) => bc.WriteToServer(table), timeoutSecs, update, mappings);
        }

        /// <summary>
        /// Inserts and optionally updates rows in a database.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename"></param>
        /// <param name="writeAction">A function for bulk uploading to the temporary table.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command. A value of 0 means no timeout.</param>
        /// <param name="update">Determines if duplicate rows should be updated.</param>
        /// <param name="mappings">The column mappings.</param>
        /// <returns>True if the upload was successful. False otherwise.</returns>
        private static bool BulkUpsert(SqlConnection conn, string tablename, Action<SqlBulkCopy> writeAction, int timeoutSecs, bool update, SqlBulkCopyColumnMapping[] mappings)
        {
            try {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                using (SqlCommand cmd = conn.CreateCommand()) {
                    cmd.Transaction = trans;
                    cmd.Parameters.AddWithValue("@tablename", tablename);
                    cmd.CommandText = @"SELECT * INTO @tablename FROM #ATempTable WHERE 1 = 0";
                    if (cmd.ExecuteNonQuery() < 1)
                        return false;

                    using (SqlBulkCopy bulkCpy = new SqlBulkCopy(conn, SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.TableLock, trans)) {
                        bulkCpy.DestinationTableName = "#ATempTable";
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
                    bool mergeSuccess = !MergeTables(conn, tablename, "#ATempTable", trans, update, bulkMappings);
                    cmd.Parameters.Clear();
                    cmd.CommandText = "DROP TABLE #ATempTable";
                    cmd.ExecuteNonQuery();
                    if (mergeSuccess)
                        trans.Commit();
                    conn.Close();
                    return mergeSuccess;
                }
            }
            catch (Exception ex) {
                PrintError(ex, "DB.BulkUpsert", conn, tablename);
            }
            return false;
        }

        /// <summary>
        /// Merges two tables together using the column mappings.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="source">The source table.</param>
        /// <param name="target">The destination table.</param>
        /// <param name="update">Determines if duplicate rows should be updated.</param>
        /// <param name="mappings">The column mappings between the tables.</param>
        /// <returns>True if the tables were merged. False otherwise.</returns>
        public static bool MergeTables(SqlConnection conn, string source, string target, bool update, SqlBulkCopyColumnMapping[] mappings)
        {
            using (SqlTransaction trans = conn.BeginTransaction()) {
                bool result = MergeTables(conn, source, target, update, mappings);
                if (result == true)
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
        /// <returns>True if the tables were merged. False otherwise.</returns>
        public static bool MergeTables(SqlConnection conn, string source, string target, SqlTransaction trans, bool update, SqlBulkCopyColumnMapping[] mappings)
        {
            try {
                using (SqlCommand cmd = conn.CreateCommand()) {
                    cmd.Transaction = trans;
                    cmd.Parameters.AddWithValue("@SourceT", source);
                    cmd.Parameters.AddWithValue("@TargetT", target);
                    StringBuilder onStringBuilder = new StringBuilder("MERGE INTO @TargetT AS Target\nUSING @SourceT AS Source\n\tON");
                    StringBuilder updateStringBuilder = new StringBuilder("WHEN MATCHED THEN UPDATE SET\n\t");
                    StringBuilder insertStringBuilder = new StringBuilder("WHEN NOT MATCHED THEN\tINSERT (");
                    StringBuilder valuesStringBuilder = new StringBuilder("\tVALUES (");
                    for (int i = 0; i < mappings.Length; i++) {
                        string targetP = "@target" + (i + 1).ToString();
                        string sourceP = "@source" + (i + 1).ToString();
                        cmd.Parameters.AddWithValue(targetP, mappings[i].DestinationColumn);
                        cmd.Parameters.AddWithValue(sourceP, mappings[i].SourceColumn);
                        onStringBuilder.Append(" Target.[").Append(targetP).Append("] = Source.[").Append(sourceP).Append("]\n\tAND");
                        updateStringBuilder.Append(" Target.[").Append(targetP).Append("] = Source.[").Append(sourceP).Append("],\n\t");
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
                    conn.Open();
                    return cmd.ExecuteNonQuery() >= 0;
                }
            }
            catch (Exception ex) {
                PrintError(ex, "DB.MergeTables", conn, "source: " + source ?? "null" + ", target: " + target ?? "null");
            }
            return false;
        }

        /// <summary>
        /// Deletes all rows from a database table.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command. A value of 0 means no timeout.</param>
        /// <returns>The number of rows that were deleted from the database. -1 on error.</returns>
        public static int DeleteTable(SqlConnection conn, string tablename, int timeoutSecs = 0)
        {
            try {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction()) {
                    int result = conn.Execute("DELETE FROM " + tablename, null, trans, timeoutSecs);
                    if(result > 0)
                        trans.Commit();
                    return result;
                }
            }
            catch (Exception ex) {
                PrintError(ex, "DB.DeleteTable", conn, tablename);
            }
            return -1;
        }

        /// <summary>
        /// Creates a command string that can be used to generate a table. It is imperfect because
        /// it doesn't copy the constraints/indexes.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="newTableName">The name of the table to create. If this is null then the same name is used.</param>
        /// <returns>The SQL command to create a table with the name and schema of the input DataTable.</returns>
        public static string CloneTableScript(SqlConnection conn, string tableName, string newTableName = null)
        {
            try {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 0 * FROM " + tableName, conn))
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly)) {
                    using (DataTable schema = reader.GetSchemaTable()) {
                        new SchemaTable(schema).CreateTableScript(newTableName ?? tableName);
                    }
                }
            }
            catch (Exception ex) {
                PrintError(ex, "DB.CloneTableScript", conn, newTableName);
            }
            return null;
        }

        /// <summary>
        /// Creates a command string that can be used to generate a table. This is imperfect 
        /// as it creates an nvarchar in all cases for strings (never nchar(n)/varchar). It also
        /// doesn't copy the constraints/indexes.
        /// </summary>
        /// <param name="table">The table representation to create a command string from.</param>
        /// <returns>The SQL command to create a table with the name and schema of the input DataTable.</returns>
        public static string CreateTableScript(DataTable table)
        {
            StringBuilder sql = new StringBuilder();
            StringBuilder alterSql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE [{0}] (", table.TableName);
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
                    sql.AppendFormat(" IDENTITY({0},{1})",
                        table.Columns[i].AutoIncrementSeed,
                        table.Columns[i].AutoIncrementStep);
                }
                else {
                    // DataColumns will add a blank DefaultValue for any AutoIncrement column. 
                    // We only want to create an ALTER statement for those columns that are not set to AutoIncrement. 
                    DefaultValue(table.TableName, table.Columns[i], data.IsNumeric, alterSql);
                }
                if (!table.Columns[i].AllowDBNull)
                    sql.Append(" NOT NULL");
                sql.Append(",");
            }
            sql.Remove(sql.Length - 1, 1);

            if (table.PrimaryKey.Length > 0) {
                sql.AppendFormat(",\n\tCONSTRAINT PK_{0} PRIMARY KEY ({1})",
                    table.TableName,
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
            }
            public string Name { get; set; }
            public bool HasDefault { get; set; }
            public bool IsNumeric { get; set; }
            public bool HasMaxLength { get; set; }
        }

        private static Dictionary<Type, DefaultData> usesDefault = new Dictionary<Type, DefaultData>() {
            { typeof(Boolean) , new DefaultData("bit", false, true) },
            { typeof(Char) , new DefaultData("nchar(1)", false, false) },
            { typeof(Byte) , new DefaultData("tinyint", false, true) },
            { typeof(SByte) , new DefaultData("tinyint", false, true) },
            { typeof(Int16) , new DefaultData("smallint", false, true) },
            { typeof(UInt16) , new DefaultData("smallint", false, true) },
            { typeof(Int32) , new DefaultData("int", false, true) },
            { typeof(UInt32) , new DefaultData("int", false, true) },
            { typeof(Int64) , new DefaultData("bigint", false, true) },
            { typeof(UInt64) , new DefaultData("bigint", false, true) },
            { typeof(Single) , new DefaultData("float", false, true) },
            { typeof(Double) , new DefaultData("float", false, true) },
            { typeof(Decimal) , new DefaultData("decimal(18, 6)", false, true) },
            { typeof(DateTime) , new DefaultData("datetime2", false, false, false) },
            { typeof(DateTimeOffset) , new DefaultData("datetimeoffset", false, false, false) },
            { typeof(byte[]), new DefaultData("varbinary({0})", true, false, false) },
            { typeof(TimeSpan) , new DefaultData("time", false, false, false) },
            { typeof(string) , new DefaultData("nvarchar({0})", true, false, true) },
        };

        private static void DefaultValue(string tablename, DataColumn col, bool isNumeric, StringBuilder alterSql)
        {
            if (!col.AutoIncrement && col.DefaultValue != null) {
                string defaultVal = col.DefaultValue.ToString();
                alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
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
                    alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
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
            try {
                conn.Execute("DROP TABLE " + tablename);
            }
            catch (Exception ex) {
                PrintError(ex, "DB.DropTable", conn, tablename);
                return false;
            }
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
        /// <param name="table">The name of the table to remove duplicates from.</param>
        /// <param name="distinctColumns">The columns to select on to check for duplicates. 
        /// If no columns are input them all columns are used.</param>
        /// <returns>The number of rows affected by the query.</returns>
        public static int RemoveDuplicates(SqlConnection conn, string tablename, params string[] distinctColumns)
        {
            try {
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
END", new { tablename = tablename, columns = distinctColumns.Length > 0 ? string.Join(",", distinctColumns) : "*" });
            }
            catch (Exception ex) {
                PrintError(ex, "DB.RemoveDuplicates", conn, tablename);
            }
            return -1;
        }

        private static void PrintError(Exception ex, string methodName, SqlConnection conn, string tablename = null)
        {
            string argStr = string.Format("{0}.{1}{2}", conn.DataSource ?? "null", conn.Database ?? "null", tablename == null ? "" : "." + tablename);
            PrintError(ex, methodName, argStr);
        }

        private static void PrintError(Exception ex, string methodName, string arg = null)
        {
            string innerMsg = ex.InnerException == null ? "" : "\n  " + ex.InnerException.Message;
            string argStr = arg == null ? "" : "(" + arg + ")";
            Console.Error.WriteLine("Error {0}{1}: {2}\n{3}{4}", methodName ?? "", argStr, ex.GetType(), ex.Message, ex.Message, innerMsg);
        }
    }
}
