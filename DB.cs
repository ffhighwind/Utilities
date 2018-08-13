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
        /// <param name="password">The password to log into the server. If null the, integrated security is used.</param>
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
        /// Returns an empty table with columns representing that of the database and table in the query.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <returns>An empty table representing the database, or null on error.</returns>
        public static DataTable TableSchema(SqlConnection conn, string tablename)
        {
            try {
                string[] restrictions = new string[4];
                restrictions[2] = tablename;
                return conn.GetSchema("Tables", restrictions);
            }
            catch (Exception ex) {
                PrintError(ex, "DB.TableSchema", conn, tablename);
            }
            return null;
        }

        /// <summary>
        /// Executes an SQL query.
        /// </summary>
        /// <typeparam name="T">The type of object to return from the query.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="cmd">The command to execute.</param>
        /// <param name="param">The parameters to pass to the command.</param>
        /// <param name="timeoutSecs">The timeout in seconds for the command.</param>
        /// <param name="maxRetries">The number of attempts to retry the command.</param>
        /// <returns>The results from the query, or null on error.</returns>
        public static IEnumerable<T> Query<T>(SqlConnection conn, string cmd, object param = null, int? timeoutSecs = null, int maxRetries = 5)
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
        /// <param name="cmd">The command to execute.</param>
        /// <param name="maxRetries">The number of attempts to retry the command.</param>
        /// <returns>The results from the query, or null on error.</returns>
        public static DataTable Query(SqlConnection conn, string cmd, object param = null, int? timeoutSecs = null, int maxRetries = 5)
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
        /// <param name="cmd">The command to execute.</param>
        /// <param name="timeout">The command timeout in seconds.</param>
        /// <param name="maxRetries">The maximum number of attempts to retry the command on failure.</param>
        /// <returns>The number of rows affected, or -1 on error.</returns>
        public static int Execute(SqlConnection conn, string cmd, object param = null, int? timeout = null, int maxRetries = 5)
        {
            Exception e = null;
            for (int i = 0; i < maxRetries; i++) {
                try {
                    return conn.Execute(cmd, param, null, timeout);
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
            return BulkInsert<T>(conn, tablename, list, 600, CreateMappings(columns));
        }

        /// <summary>
        /// Uploads a collection to a database.
        /// </summary>
        /// <typeparam name="T">The type of object in the collection.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the database table to upload data to.</param>
        /// <param name="list">The List to upload.</param>
        /// <param name="timeoutSecs">The timeout in seconds for the command.</param>
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
        public static bool BulkInsert<T>(SqlConnection conn, string tablename, IEnumerable<T> list, int timeoutSecs = 600, params SqlBulkCopyColumnMapping[] mappings) where T : class
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
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command.</param>
        /// <param name="options">The bulk copy options. If you do not wish to lock the database table then you will need to change this.</param>
        /// <param name="mappings">The column mappings. If no mappings are given then all columns are mapped.</param>
        /// <returns>True if the upload was successful, false otherwise.</returns>
        public static bool BulkInsert(SqlConnection conn, DataTable table, int timeoutSecs = 600, params SqlBulkCopyColumnMapping[] mappings)
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
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command.</param>
        /// <param name="mappings">The column mappings.</param>
        /// <param name="update">Determines if duplicate rows should be updated.</param>
        /// <returns>True if the upload was successful. False otherwise.</returns>
        public static bool BulkUpsert<T>(SqlConnection conn, string tablename, IEnumerable<T> list, int timeoutSecs = 600, bool update = true, params SqlBulkCopyColumnMapping[] mappings) where T : class
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
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command.</param>
        /// <param name="mappings">The column mappings.</param>
        /// <param name="update">Determines if duplicate rows should be updated.</param>
        /// <returns>True if the upload was successful. False otherwise.</returns>
        public static bool BulkUpsert(SqlConnection conn, DataTable table, int timeoutSecs = 600, bool update = true, params SqlBulkCopyColumnMapping[] mappings)
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
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command.</param>
        /// <param name="update">Determines if duplicate rows should be updated.</param>
        /// <param name="mappings">The column mappings.</param>
        /// <returns>True if the upload was successful. False otherwise.</returns>
        private static bool BulkUpsert(SqlConnection conn, string tablename, Action<SqlBulkCopy> writeAction, int timeoutSecs, bool update, SqlBulkCopyColumnMapping[] mappings)
        {
            try {
                using (SqlTransaction trans = conn.BeginTransaction())
                using (SqlCommand cmd = conn.CreateCommand()) {
                    cmd.Parameters.AddWithValue("@tablename", tablename);
                    cmd.CommandText = @"SELECT * INTO @tablename FROM #ATempTable WHERE 1 = 0";
                    if (cmd.ExecuteNonQuery() < 1)
                        return false;

                    using (SqlBulkCopy bulkCpy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, trans)) {
                        bulkCpy.DestinationTableName = "#ATempTable";
                        bulkCpy.BulkCopyTimeout = timeoutSecs;
                        foreach (SqlBulkCopyColumnMapping mapping in mappings) {
                            bulkCpy.ColumnMappings.Add(mapping);
                        }
                        conn.Open();
                        writeAction(bulkCpy);
                    }
                    SqlBulkCopyColumnMapping[] bulkMappings = new SqlBulkCopyColumnMapping[mappings.Length];
                    for (int i = 0; i < bulkMappings.Length; i++) {
                        bulkMappings[i] = new SqlBulkCopyColumnMapping(mappings[i].DestinationColumn, mappings[i].DestinationColumn);
                    }
                    bool mergeSuccess = !MergeTables(conn, tablename, "#ATempTable", update, bulkMappings);
                    cmd.Parameters.Clear();
                    cmd.CommandText = "DROP TABLE #ATempTable";
                    cmd.ExecuteNonQuery();
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
            try {
                using (SqlCommand cmd = conn.CreateCommand()) {
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
                    return cmd.ExecuteNonQuery() >= 0;
                }
            }
            catch (Exception ex) {
                PrintError(ex, "DB.MergeTables", conn, "source: " + source ?? "null" + ", target: " + target ?? "null");
            }
            return false;
        }

        /// <summary>
        /// Creates a database table.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <returns>True if the table was created. False otherwise.</returns>
        public static bool CreateTable(SqlConnection conn, DataTable table)
        {
            try {
                conn.Execute(CreateTableString(table));
                return true;
            }
            catch (Exception ex) {
                PrintError(ex, "DB.CreateTable", conn, table.TableName);
            }
            return false;
        }

        /// <summary>
        /// Deletes all rows from a database table.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command.</param>
        /// <returns>True if the database was deleted. False otherwise.</returns>
        public static bool DeleteTable(SqlConnection conn, string tablename, int timeoutSecs = 600)
        {
            try {
                return conn.Execute("DELETE FROM @tablename", new { tablename = tablename }, null, timeoutSecs) > 0;
            }
            catch (Exception ex) {
                PrintError(ex, "DB.DeleteTable", conn, tablename);
            }
            return false;
        }

        /// <summary>
        /// Recreates a database table.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <returns>True if the table was recreated. False otherwise.</returns>
        public static bool RecreateTable(SqlConnection conn, string tablename)
        {
            try {
                using (DataTable table = TableSchema(conn, tablename)) {
                    if (table != null) {
                        return conn.Execute("DROP TABLE [@tablename]", new { tablename = tablename }) > 0
                            && conn.Execute(CreateTableString(table)) > 0;
                    }
                }
            }
            catch (Exception ex) {
                PrintError(ex, "DB.RecreateTable", conn, tablename);
            }
            return false;
        }

        /// <summary>
        /// Creates a command string that can be used to generate a table.
        /// </summary>
        /// <param name="table">The table name and schema to create a command string from.</param>
        /// <param name="varCharLen">The default length of strings.</param>
        /// <returns>The SQL command to create a table with the name and schema of the input DataTable.</returns>
        public static string CreateTableString(DataTable table, int varCharLen = 100)
        {
            StringBuilder sql = new StringBuilder();
            StringBuilder alterSql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE [{0}] (", table.TableName);

            for (int i = 0; i < table.Columns.Count; i++) {
                bool isNumeric = false;
                bool usesColumnDefault = true;

                sql.AppendFormat("\n\t[{0}]", table.Columns[i].ColumnName);
                switch (Type.GetTypeCode(table.Columns[i].DataType)) {
                    case TypeCode.Boolean:
                        sql.Append(" bit");
                        isNumeric = true;
                        break;
                    case TypeCode.Char:
                        sql.Append(" nchar(1)");
                        break;
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                        sql.Append(" tinyint");
                        isNumeric = true;
                        break;
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                        sql.Append(" smallint");
                        isNumeric = true;
                        break;
                    case TypeCode.UInt32:
                    case TypeCode.Int32:
                        sql.Append(" int");
                        isNumeric = true;
                        break;
                    case TypeCode.UInt64:
                    case TypeCode.Int64:
                        sql.Append(" bigint");
                        isNumeric = true;
                        break;
                    case TypeCode.DateTime:
                        sql.Append(" datetime2");
                        usesColumnDefault = false;
                        break;
                    case TypeCode.String:
                        sql.AppendFormat(" nvarchar({0})", (table.Columns[i].MaxLength <= 0) ? varCharLen : table.Columns[i].MaxLength);
                        break;
                    case TypeCode.Single:
                    case TypeCode.Double:
                        sql.Append(" float");
                        isNumeric = true;
                        break;
                    case TypeCode.Decimal:
                        sql.AppendFormat(" decimal(18, 6)");
                        isNumeric = true;
                        break;
                    default:
                        int maxLen = table.Columns[i].MaxLength;
                        sql.AppendFormat(" nvarchar({0}) ", maxLen <= 0 || maxLen > 4000 ? "max" : maxLen.ToString());
                        break;
                }

                if (table.Columns[i].AutoIncrement) {
                    sql.AppendFormat(" IDENTITY({0},{1})",
                        table.Columns[i].AutoIncrementSeed,
                        table.Columns[i].AutoIncrementStep);
                }
                else {
                    // DataColumns will add a blank DefaultValue for any AutoIncrement column. 
                    // We only want to create an ALTER statement for those columns that are not set to AutoIncrement. 
                    if (table.Columns[i].DefaultValue != null) {
                        if (usesColumnDefault) {
                            string defaultVal = table.Columns[i].DefaultValue.ToString();
                            alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
                                table.TableName,
                                table.Columns[i].ColumnName,
                                isNumeric ? defaultVal : "'" + defaultVal + "'");
                        }
                        else {
                            // Default values on Date columns, e.g., "DateTime.Now" will not translate to SQL.
                            // This inspects the caption for a simple XML string to see if there is a SQL compliant default value, e.g., "GETDATE()".
                            try {
                                System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
                                xml.LoadXml(table.Columns[i].Caption);
                                alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
                                    table.TableName,
                                    table.Columns[i].ColumnName,
                                    xml.GetElementsByTagName("defaultValue")[0].InnerText);
                            }
                            catch {
                                // Handle
                            }
                        }
                    }
                }
                if (!table.Columns[i].AllowDBNull)
                    sql.Append(" NOT NULL");
                sql.Append(",");
            }

            if (table.PrimaryKey.Length > 0) {
                StringBuilder primaryKeySql = new StringBuilder();

                primaryKeySql.AppendFormat("\n\tCONSTRAINT PK_{0} PRIMARY KEY (", table.TableName);

                for (int i = 0; i < table.PrimaryKey.Length; i++) {
                    primaryKeySql.AppendFormat("{0},", table.PrimaryKey[i].ColumnName);
                }
                primaryKeySql.Remove(primaryKeySql.Length - 1, 1);
                primaryKeySql.Append(")");

                sql.Append(primaryKeySql);
            }
            else
                sql.Remove(sql.Length - 1, 1);

            sql.AppendFormat("\n);\n{0}", alterSql.ToString());

            return sql.ToString();
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
                conn.Execute("DROP TABLE [@tablename]", new { tablename = tablename });
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
                return 1 == conn.Execute(
@"IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_NAME = @tablename
)
SELECT 1 ELSE SELECT 0", new { tablename = tablename });
            }
            catch (Exception ex) {
                try {
                    // Other RDBMS.  Graceful degradation
                    return 1 == conn.Execute("SELECT 1 FROM @tablename WHERE 1 = 0", new { tablename = tablename });
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
