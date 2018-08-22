using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Utilities
{
    /// <summary>
    /// A class representation of the result from <see cref="SqlDataReader.GetSchemaTable"/>.
    /// </summary>
    //// https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-schema-collections?view=netframework-4.7.2
    //// https://docs.microsoft.com/en-us/sql/relational-databases/system-information-schema-views/system-information-schema-views-transact-sql?view=sql-server-2017
    //// https://docs.microsoft.com/en-us/dotnet/api/system.data.datatablereader.getschematable?redirectedfrom=MSDN&view=netframework-4.7.2#System_Data_DataTableReader_GetSchemaTable
    public class SchemaTable
    {
        /// <summary>
        /// The start of the string returned by CreateTableScript.
        /// </summary>
        private string createTable = null;

        /// <summary>
        /// The primary key constraint to append to createTable when calling CreateTableScript.
        /// </summary>
        private string pkeyConstraint = null;

        /// <summary>
        /// The primary key columns for the input schema.
        /// </summary>
        public IReadOnlyList<SchemaTableColumn> PrimaryKey { get; private set; }

        /// <summary>
        /// The unique columns for the input schema.
        /// </summary>
        public IReadOnlyList<SchemaTableColumn> UniqueColumns { get; private set; }

        /// <summary>
        /// The columns for the input schema.
        /// </summary>
        public IReadOnlyList<SchemaTableColumn> Columns { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaTable"/> class.
        /// </summary>
        /// <param name="schema">The results from <see cref="SqlDataReader.GetSchemaTable"/>.</param>
        public SchemaTable(DataTable schema)
        {
            SchemaTableColumn[] columns = new SchemaTableColumn[schema.Rows.Count];
            List<SchemaTableColumn> uniqueCols = new List<SchemaTableColumn>();
            List<SchemaTableColumn> pkey = new List<SchemaTableColumn>();
            for (int i = 0; i < schema.Rows.Count; i++) {
                columns[i] = new SchemaTableColumn(schema, i);
                if (columns[i].IsKey)
                    pkey.Add(columns[i]);
                if (columns[i].IsUnique)
                    uniqueCols.Add(columns[i]);
            }
            Columns = columns;
            UniqueColumns = uniqueCols;
            PrimaryKey = pkey;
        }

        /// <summary>
        /// Creates a command string creating a table with the given schema.
        /// </summary>
        /// <param name="tablename">The name of the new table to create.</param>
        /// <returns>A command string for creating a table with the given schema.</returns>
        public string CreateTableScript(string tablename)
        {
            if (createTable == null) {
                // lazy evaluation
                StringBuilder sql = new StringBuilder(" (");
                for (int i = 0; i < Columns.Count; i++) {
                    SchemaTableColumn col = Columns[i];
                    sql.AppendFormat(
                        "\n\t[{0}] {1}{2}{3}{4},",
                        col.Name,
                        col.DbTypeFullName,
                        col.IsIdentity ? " IDENTITY" : "",
                        col.IsAutoIncrement ? "(1,1)" : "",
                        !col.AllowDBNull ? " NOT NULL" : "");
                }
                if (UniqueColumns.Count > 0) {
                    sql.AppendFormat("\r\n CONSTRAINT AK_{0} UNIQUE ({0})," + UniqueColumns[0], string.Join(",", UniqueColumns));
                }
                if (PrimaryKey.Count > 0) {
                    sql.Append("\r\n CONSTRAINT PK_");
                    pkeyConstraint = string.Format(" PRIMARY KEY ({0})\n);", string.Join(",", PrimaryKey.Select(col => col.Name)));
                }
                else
                    sql.Remove(sql.Length - 1, 1).Append("\n);"); // remove last comma
                createTable = sql.ToString();
            }

            string result = "CREATE TABLE " + tablename + createTable;
            if (pkeyConstraint != null)
                result += tablename + pkeyConstraint;
            return result;
        }
    }
}