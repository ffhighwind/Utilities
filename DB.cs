using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;
using FastMember;


namespace Utilities
{
    public static class DB
    {
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
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
            sb.DataSource = server;
            sb.InitialCatalog = database;
            sb.ConnectTimeout = timeoutSecs;
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
                        conn.Close();
                    }
                }
                catch (Exception ex) {
                    PrintError(ex, "ConnString", (server ?? "") + " " + (database ?? ""));
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
        /// <param name="conn">The database connection to perform the query on.</param>
        /// <param name="table">The table to query.</param>
        /// <returns>An empty table representing the database, or null on error.</returns>
        public static DataTable TableSchema(SqlConnection conn, string table)
        {
            try {
                string[] restrictions = new string[4];
                restrictions[2] = table;
                return conn.GetSchema("Tables", restrictions);
            }
            catch (Exception ex) {
                PrintError(ex);
            }
            return null;
        }

        /// <summary>
        /// Executes an SQL query.
        /// </summary>
        /// <typeparam name="T">The type of object to return from the query.</typeparam>
        /// <param name="conn">The database connection to execute the command on.</param>
        /// <param name="cmd">The command to execute.</param>
        /// <param name="param">The column mappings for Dapper.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command.</param>
        /// <param name="maxRetries">The maximum number of attempts to retry the command on failure.</param>
        /// <returns>The results from the query, or null on error.</returns>
        public static IEnumerable<T> Query<T>(SqlConnection conn, string cmd, object param = null, int maxRetries = 5)
        {
            Exception e = null;
            for (int i = 0; i < maxRetries; i++) {
                try {
                    return conn.Query<T>(cmd, param);
                }
                catch (Exception ex) {
                    e = ex;
                }
            }
            PrintError(e, "Query", conn.DataSource + " " + conn.DataSource);
            return null;
        }

        /// <summary>
        /// Executes an SQL query.
        /// </summary>
        /// <param name="cmd">The command to execute.</param>
        /// <param name="maxRetries">The maximum number of attempts to retry the command on failure.</param>
        /// <returns>The results from the query, or null on error.</returns>
        public static DataTable Query(SqlCommand cmd, int maxRetries = 5)
        {
            Exception e = null;
            for (int i = 0; i < maxRetries; i++) {
                try {
                    DataTable table = new DataTable();
                    cmd.Connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        table.Load(reader);
                    }
                    cmd.Connection.Close();
                    return table;
                }
                catch (Exception ex) {
                    e = ex;
                }
            }
            PrintError(e, "Query", cmd.Connection.DataSource + " " + cmd.Connection.Database);
            return null;
        }

        /// <summary>
        /// Executes an SQL command.
        /// </summary>
        /// <param name="cmd">The command to execute.</param>
        /// <param name="maxRetries">The maximum number of attempts to retry the command on failure.</param>
        /// <returns>The number of rows affected, or -1 on error.</returns>
        public static int Execute(SqlConnection conn, string cmd, object param = null, int maxRetries = 5)
        {
            Exception e = null;
            for (int i = 0; i < maxRetries; i++) {
                try {
                    conn.Open();
                    int count = conn.Execute(cmd, param);
                    conn.Close();
                    return count;
                }
                catch (Exception ex) {
                    e = ex;
                }
            }
            PrintError(e, "Execute", conn.DataSource + '.' + conn.Database);
            conn.Close();
            return -1;
        }

        /// <summary>
        /// Executes an SQL command.
        /// </summary>
        /// <param name="cmd">The command to execute.</param>
        /// <param name="param">The parameters to pass to the command.</param>
        /// <param name="maxRetries">The maximum number of attempts to retry the command on failure.</param>
        /// <returns>The number of rows affected, or -1 on error.</returns>
        public static int Execute(SqlCommand cmd, object param = null, int maxRetries = 5)
        {
            return Execute(cmd.Connection, cmd.CommandText, param, maxRetries);
        }

        /// <summary>
        /// Uploads a collection to a database.
        /// </summary>
        /// <typeparam name="T">The type of object in the collection.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the database table to upload data to.</param>
        /// <param name="list">The data to upload.</param>
        /// <param name="fields">The column mappings in order.</param>
        /// <returns>True if the upload was successful. False otherwise.</returns>
        public static bool BulkUpload<T>(SqlConnection conn, string tablename, IEnumerable<T> list, params string[] fields)
        {
            var members = TypeAccessor.Create(typeof(T)).GetMembers();
            return BulkUpload<T>(conn, tablename, list, 600, SqlBulkCopyOptions.FireTriggers
                | SqlBulkCopyOptions.UseInternalTransaction | SqlBulkCopyOptions.TableLock, members.Select(member => member.Name).ToArray());
        }

        /// <summary>
        /// Uploads a collection to a database.
        /// </summary>
        /// <typeparam name="T">The type of object in the collection.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the database table to upload data to.</param>
        /// <param name="list">The data to upload.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command.</param>
        /// <param name="fields">The column mappings in order.</param>
        /// <returns>True if the upload was successful. False otherwise.</returns>
        public static bool BulkUpload<T>(SqlConnection conn, string tablename, IEnumerable<T> list, int timeoutSecs, params string[] fields)
        {
            var members = TypeAccessor.Create(typeof(T)).GetMembers();
            return BulkUpload<T>(conn, tablename, list, timeoutSecs, SqlBulkCopyOptions.FireTriggers
                | SqlBulkCopyOptions.UseInternalTransaction | SqlBulkCopyOptions.TableLock, members.Select(member => member.Name).ToArray());
        }

        /// <summary>
        /// Uploads a collection to a database.
        /// </summary>
        /// <typeparam name="T">The type of object in the collection.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="tablename">The name of the database table to upload data to.</param>
        /// <param name="list">The data to upload.</param>
        /// <param name="timeoutSecs">The maximum timeout in seconds for the command.</param>
        /// <param name="options">The bulk upload options.</param>
        /// <param name="fields">The column mappings in order.</param>
        /// <returns>True if the upload was successful. False otherwise.</returns>
        public static bool BulkUpload<T>(SqlConnection conn, string tablename, IEnumerable<T> list, int timeoutSecs, SqlBulkCopyOptions options =
            SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.UseInternalTransaction | SqlBulkCopyOptions.TableLock, params string[] fields)
        {
            try {
                using (ObjectReader reader = ObjectReader.Create(list, fields))
                using (SqlBulkCopy bulkCpy = new SqlBulkCopy(conn, options, null)) {
                    bulkCpy.DestinationTableName = tablename;
                    bulkCpy.BulkCopyTimeout = timeoutSecs;
                    foreach (string member in fields) {
                        bulkCpy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(member, member));
                    }
                    conn.Open();
                    bulkCpy.WriteToServer(reader);
                }
                return true;
            }
            catch (Exception ex) {
                PrintError(ex, list == null ? "" : list.Count().ToString());
            }
            return false;
        }

        /// <summary>
        /// Uploads a DataTable to a database.
        /// </summary>
        /// <param name="conn">The SQL database connection.</param>
        /// <param name="dt">The DataTable to upload.</param>
        /// <param name="options">The bulk copy options. If you do not wish to lock the database table then you will need to change this.</param>
        /// <returns>True if the upload was successful, false otherwise.</returns>
        public static bool BulkUpload(SqlConnection conn, DataTable dt, int timeoutSecs = 600, SqlBulkCopyOptions options =
            SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.UseInternalTransaction | SqlBulkCopyOptions.TableLock)
        {
            try {
                using (SqlBulkCopy bulkCpy = new SqlBulkCopy(conn, options, null)) {
                    bulkCpy.DestinationTableName = dt.TableName;
                    bulkCpy.BulkCopyTimeout = timeoutSecs;
                    foreach (DataColumn col in dt.Columns) {
                        bulkCpy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    }
                    conn.Open();
                    bulkCpy.WriteToServer(dt);
                    conn.Close();
                    return true;
                }
            }
            catch (Exception ex) {
                PrintError(ex, "BulkUpload", conn.DataSource + " " + conn.Database);
            }
            return false;
        }

        public static string CreateTable(DataTable dt)
        {
            StringBuilder sql = new StringBuilder();
            StringBuilder alterSql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE [{0}] (", dt.TableName);

            for (int i = 0; i < dt.Columns.Count; i++) {
                bool isNumeric = false;
                bool usesColumnDefault = false;

                sql.AppendFormat("\n\t[{0}]", dt.Columns[i].ColumnName);

                switch (dt.Columns[i].DataType.ToString().ToUpper()) {
                    case "SYSTEM.INT16":
                        sql.Append(" smallint");
                        isNumeric = true;
                        break;
                    case "SYSTEM.INT32":
                        sql.Append(" int");
                        isNumeric = true;
                        break;
                    case "SYSTEM.INT64":
                        sql.Append(" bigint");
                        isNumeric = true;
                        break;
                    case "SYSTEM.DATETIME":
                        sql.Append(" datetime");
                        usesColumnDefault = false;
                        break;
                    case "SYSTEM.STRING":
                        int maxlength = (dt.Columns[i].MaxLength <= 0) ? 100 : dt.Columns[i].MaxLength;
                        //int maxlength = 100;
                        sql.AppendFormat(" nvarchar({0})", maxlength);
                        break;
                    case "SYSTEM.FLOAT":
                        sql.Append(" float");
                        isNumeric = true;
                        break;
                    //DOUBLES DO NOT EXIST IN SQL
                    //DON'T USE THIS
                    case "SYSTEM.DOUBLE":
                        //sql.Append(" double");
                        sql.Append(" float");
                        isNumeric = true;
                        break;
                    case "SYSTEM.DECIMAL":
                        sql.AppendFormat(" decimal(18, 6)");
                        isNumeric = true;
                        break;
                    default:
                        sql.AppendFormat(" nvarchar({0})", dt.Columns[i].MaxLength);
                        break;
                }

                if (dt.Columns[i].AutoIncrement) {
                    sql.AppendFormat(" IDENTITY({0},{1})",
                        dt.Columns[i].AutoIncrementSeed,
                        dt.Columns[i].AutoIncrementStep);
                }
                else {
                    // DataColumns will add a blank DefaultValue for any AutoIncrement column. 
                    // We only want to create an ALTER statement for those columns that are not set to AutoIncrement. 
                    if (dt.Columns[i].DefaultValue != null) {
                        if (usesColumnDefault) {
                            if (isNumeric) {
                                alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
                                    dt.TableName,
                                    dt.Columns[i].ColumnName,
                                    dt.Columns[i].DefaultValue);
                            }
                            else {
                                alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ('{2}') FOR [{1}];",
                                    dt.TableName,
                                    dt.Columns[i].ColumnName,
                                    dt.Columns[i].DefaultValue);
                            }
                        }
                        else {
                            // Default values on Date columns, e.g., "DateTime.Now" will not translate to SQL.
                            // This inspects the caption for a simple XML string to see if there is a SQL compliant default value, e.g., "GETDATE()".
                            try {
                                System.Xml.XmlDocument xml = new System.Xml.XmlDocument();

                                xml.LoadXml(dt.Columns[i].Caption);

                                alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
                                    dt.TableName,
                                    dt.Columns[i].ColumnName,
                                    xml.GetElementsByTagName("defaultValue")[0].InnerText);
                            }
                            catch {
                                // Handle
                            }
                        }
                    }
                }

                if (!dt.Columns[i].AllowDBNull) {
                    sql.Append(" NOT NULL");
                }

                sql.Append(",");
            }

            if (dt.PrimaryKey.Length > 0) {
                StringBuilder primaryKeySql = new StringBuilder();

                primaryKeySql.AppendFormat("\n\tCONSTRAINT PK_{0} PRIMARY KEY (", dt.TableName);

                for (int i = 0; i < dt.PrimaryKey.Length; i++) {
                    primaryKeySql.AppendFormat("{0},", dt.PrimaryKey[i].ColumnName);
                }

                primaryKeySql.Remove(primaryKeySql.Length - 1, 1);
                primaryKeySql.Append(")");

                sql.Append(primaryKeySql);
            }
            else {
                sql.Remove(sql.Length - 1, 1);
            }

            sql.AppendFormat("\n);\n{0}", alterSql.ToString());

            return sql.ToString();
        }

        private static bool DeleteTable(SqlConnection conn, string tablename)
        {
            string tabledropstring = String.Format(@"BEGIN Transaction DROP TABLE {0}.dbo.{1} COMMIT",
                conn.Database, tablename);

            try {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(tabledropstring, conn)) {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                PrintError(ex, "DeleteTable", conn.DataSource + "," + conn.Database + "," + tablename);
                return false;
            }
            return true;
        }

        public static void PrintError(Exception ex, string methodName = "", string arg = null)
        {
            string innerMsg = ex.InnerException == null ? "" : "\n  " + ex.InnerException.Message;
            string argStr = arg == null ? "" : "(" + arg + ")";
            Console.Error.WriteLine("Error {0}{1}: {2}\n{3}{4}", methodName, argStr, ex.GetType(), ex.Message, ex.Message, innerMsg);
        }

        public static bool Exists(string tablename)
        {
            bool exists;
            try {
                // ANSI SQL way.  Works in PostgreSQL, MSSQL, MySQL.  
                OdbcCommand cmd = new OdbcCommand("SELECT CASE WHEN EXISTS (SELECT * FROM information_schema.tables WHERE table_name = @tablename) THEN 1 ELSE 0 END");
                cmd.Parameters.AddWithValue("@tablename", tablename);

                exists = (int)cmd.ExecuteScalar() == 1;
            }
            catch {
                try {
                    // Other RDBMS.  Graceful degradation
                    exists = true;
                    OdbcCommand cmd = new OdbcCommand("SELECT 1 FROM @tablename WHERE 1 = 0");
                    cmd.Parameters.AddWithValue("@tablename", tablename);
                    cmd.ExecuteNonQuery();
                }
                catch {
                    exists = false;
                }
            }
            return exists;
        }
    }
}
