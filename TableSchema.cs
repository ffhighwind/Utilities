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
            IsHidden = CastDBNull(r["IsHidden"], false);
            DataType = (Type) r["DataType"];
            AllowDBNull = (bool) r["AllowDBNull"];
            ProviderType = (SqlDbType) r["ProviderType"];
            ProviderDataType = (Type) r["ProviderSpecificDataType"];
            //IsExpression = CastDBNull<bool>(r["IsExpression"], false);
            IsAliased = CastDBNull(r["IsAliased"], Name != null && Name != BaseName);
            IsAutoIncrement = (bool) r["IsAutoIncrement"];
            IsRowVersion = CastDBNull(r["IsRowVersion"], false);
            IsHidden = CastDBNull(r["IsHidden"], false);
            IsLongBlob = (bool) r["IsLong"]; //Blob type with very long data
            IsReadOnly = (bool) r["IsReadOnly"];
            NonVersionedProviderType = (SqlDbType) r["NonVersionedProviderType"];
            ProviderTypeName = r["DataTypeName"] as string;
            IsColumnSet = (bool) r["IsColumnSet"];
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
        //public string XmlSchemaCollectionDatabase { get; private set; }
        //public string XmlSchemaCollectionOwningSchema { get; private set; }
        //public string XmlSchemaCollectionName { get; private set; }
        //public string UdtAssemblyQualifiedName { get; private set; }
        public SqlDbType NonVersionedProviderType { get; private set; }
        public bool IsColumnSet { get; private set; }

        private static T CastDBNull<T>(object obj, T defaultVal)
        {
            return (obj == DBNull.Value) ? defaultVal : (T) obj;
        }
    }

    public class SchemaTable
    {
        private SchemaTableColumn[] columns;

        public SchemaTable(DataTable tableSchema)
        {
            columns = new SchemaTableColumn[tableSchema.Columns.Count];
            for (int i = 0; i < columns.Length; i++) {
                columns[i] = new SchemaTableColumn(tableSchema, i);
            }
        }

        public IReadOnlyList<SchemaTableColumn> Columns { get { return Columns; } }
    }
}
