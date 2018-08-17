using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class SchemaTableColumn
    {
        /// <summary>
        /// SchemaTableColumn exists in .NET 4.7.2
        /// https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-schema-collections?view=netframework-4.7.2
        /// https://docs.microsoft.com/en-us/sql/relational-databases/system-information-schema-views/system-information-schema-views-transact-sql?view=sql-server-2017
        /// https://docs.microsoft.com/en-us/dotnet/api/system.data.datatablereader.getschematable?redirectedfrom=MSDN&view=netframework-4.7.2#System_Data_DataTableReader_GetSchemaTable
        /// </summary>
        public SchemaTableColumn(DataTable schema, int rowIndex)
        {
            DataRow r = schema.Rows[rowIndex];
            Name = r["ColumnName"] as string;
            Ordinal = (int) r["ColumnOrdinal"];
            Size = CastDBNull<int?>(r["ColumnSize"], null);
            NumericPrecision = CastDBNull<short?>(r["NumericPrecision"], null);
            NumericScale = CastDBNull<short?>(r["NumericScale"], null);
            IsUnique = (bool) r["IsUnique"];
            IsKey = (bool) r["IsKey"];
            IsIdentity = (bool) r["IsIdentity"];
            IsHidden = CastDBNull(r["IsHidden"], false);
            DataType = (Type) r["DataType"];
            AllowDBNull = (bool) r["AllowDBNull"];
            ProviderType = (SqlDbType) r["ProviderType"];
            ProviderDataType = (Type) r["ProviderSpecificDataType"];
            //IsExpression = CastDBNull<bool>(r["IsExpression"], false);
            IsAliased = CastDBNull(r["IsAliased"], Name != null && BaseName != null && Name != BaseName);
            IsAutoIncrement = (bool) r["IsAutoIncrement"];
            IsRowVersion = CastDBNull(r["IsRowVersion"], false);
            IsHidden = CastDBNull(r["IsHidden"], false);
            IsLongBlob = (bool) r["IsLong"]; //Blob type with very long data
            IsReadOnly = (bool) r["IsReadOnly"];
            NonVersionedProviderType = (SqlDbType) r["NonVersionedProviderType"];
            ProviderTypeName = r["DataTypeName"] as string;
            IsColumnSet = (bool) r["IsColumnSet"];
            ProviderTypeFullName = ProviderTypeName;
            if (Size != null)
                ProviderTypeFullName += "(" + (Size > 4000 || Size < 1 ? "MAX" : Size.ToString()) + ")";
            else if (NumericPrecision != null)
                ProviderTypeFullName += "(" + NumericPrecision + "," + NumericScale + ")";
        }

        public string Name { get; private set; }
        public string BaseName { get; private set; }
        public int Ordinal { get; private set; }
        public int? Size { get; private set; }
        public short? NumericPrecision { get; private set; }
        public short? NumericScale { get; private set; }
        public bool IsUnique { get; private set; }
        public bool IsKey { get; private set; }
        public Type DataType { get; private set; } //System.Int32
        public bool AllowDBNull { get; private set; }
        public SqlDbType ProviderType { get; private set; }
        //public bool IsExpression { get; private set; }
        public bool IsAliased { get; private set; }
        public bool IsAutoIncrement { get; private set; }
        public bool IsRowVersion { get; private set; }
        public bool IsHidden { get; private set; }
        public bool IsLongBlob { get; private set; }
        public bool IsReadOnly { get; private set; }
        public Type ProviderDataType { get; private set; } //System.Data.SqlTypes.SqlInt32
        public string ProviderTypeName { get; private set; } // SQL specific datatype
        public bool IsIdentity { get; private set; }
        //public string XmlSchemaCollectionDatabase { get; private set; }
        //public string XmlSchemaCollectionOwningSchema { get; private set; }
        //public string XmlSchemaCollectionName { get; private set; }
        //public string UdtAssemblyQualifiedName { get; private set; }
        public SqlDbType NonVersionedProviderType { get; private set; }
        public bool IsColumnSet { get; private set; }

        public string ProviderTypeFullName { get; private set; }

        public override string ToString()
        {
            return ProviderTypeFullName;
        }

        private static T CastDBNull<T>(object obj, T defaultVal)
        {
            return (obj == DBNull.Value) ? defaultVal : (T) obj;
        }
    }

    public class SchemaTable
    {
        private SchemaTableColumn[] columns;
        private string createTable = null;
        private string pkeyConstraint = null;
        public List<string> PrimaryKey { get; private set; } = new List<string>();
        public List<string> UniqueColumns { get; private set; } = new List<string>();

        public IReadOnlyList<SchemaTableColumn> Columns { get { return columns; } }

        public SchemaTable(DataTable tableSchema)
        {
            columns = new SchemaTableColumn[tableSchema.Rows.Count];
            for (int i = 0; i < columns.Length; i++) {
                columns[i] = new SchemaTableColumn(tableSchema, i);
            }

            StringBuilder sql = new StringBuilder(" (");
            //https://stackoverflow.com/questions/7818701/generate-create-table-script-from-select-statement
            for (int i = 0; i < columns.Length; i++) {
                SchemaTableColumn col = columns[i];
                sql.AppendFormat("\n\t[{0}] {1}{2}{3}{4},",
                    col.Name,
                    col.ProviderTypeFullName,
                    col.IsIdentity ? " IDENTITY" : "",
                    col.IsAutoIncrement ? "(1,1)" : "",
                    !col.AllowDBNull ? " NOT NULL" : "");
                if (col.IsKey)
                    PrimaryKey.Add(col.Name);
                if (col.IsUnique)
                    UniqueColumns.Add(col.Name);
            }
            for (int i = 0; i < UniqueColumns.Count; i++) {
                sql.AppendFormat(" CONSTRAINT AK_{0} UNIQUE ({1})," + UniqueColumns[i], UniqueColumns[i]);
            }
            if (PrimaryKey.Count > 0) {
                sql.Append("\r\n CONSTRAINT PK_");
                pkeyConstraint = string.Format(" PRIMARY KEY ({0})\n);", string.Join(",", PrimaryKey));
            }
            else
                sql.Remove(sql.Length - 1, 1).Append("\n);");
            createTable = sql.ToString();
        }

        public string CreateTableScript(string tablename)
        {
            string result = "CREATE TABLE " + tablename + createTable;
            if (pkeyConstraint != null)
                result += tablename + pkeyConstraint;
            return result;
        }

        /*
        private static Dictionary<string, bool> hasSizeDict = new Dictionary<string, bool>() {
            { "bigint",  false },
            { "binary",  true },
            { "bit",  false },
            { "char",  true },
            { "date",  false },
            { "datetime",  false },
            { "datetime2",  false },
            { "datetimeoffset",  false },
            { "decimal",  false },
            { "float",  false },
            { "geography",  false },
            { "geometry",  false },
            { "hierarchyid",  false },
            { "image",  true },
            { "int",  false },
            { "money",  false },
            { "nchar",  true },
            { "ntext",  true },
            { "numeric",  false },
            { "nvarchar",  true },
            { "real",  false },
            { "smalldatetime",  false },
            { "smallint",  false },
            { "smallmoney",  false },
            { "sql_variant",  false },
            { "sysname",  false },
            { "text",  true },
            { "time",  false },
            { "timestamp",  false },
            { "tinyint",  false },
            { "uniqueidentifier",  false },
            { "varbinary",  true },
            { "varchar",  true },
            { "xml",  false },
        };

        private static Dictionary<string, bool> hasPrecisionScaleDict = new Dictionary<string, bool>() {
            { "bigint", false },
            { "binary", false },
            { "bit",  false },
            { "char",  false },
            { "date",  false },
            { "datetime",  false },
            { "datetime2",  false },
            { "datetimeoffset",  false },
            { "decimal",  true },
            { "float",  true },
            { "geography",  false },
            { "geometry",  false },
            { "hierarchyid",  false },
            { "image",  false },
            { "int",  false },
            { "money",  false },
            { "nchar",  false },
            { "ntext",  false },
            { "numeric",  false },
            { "nvarchar",  false },
            { "real",  true },
            { "smalldatetime",  false },
            { "smallint",  false },
            { "smallmoney",  false },
            { "sql_variant",  false },
            { "sysname",  false },
            { "text",  false },
            { "time",  false },
            { "timestamp",  false },
            { "tinyint",  false },
            { "uniqueidentifier",  false },
            { "varbinary",  false },
            { "varchar",  false },
            { "xml",  false },
        };
        */
    }
}
