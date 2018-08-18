using System;
using System.Collections.Generic;
using System.Data;
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
            DbType = (SqlDbType) r["ProviderType"];
            SqlType = (Type) r["ProviderSpecificDataType"];
            //IsExpression = CastDBNull<bool>(r["IsExpression"], false);
            IsAliased = CastDBNull(r["IsAliased"], Name != null && BaseName != null && Name != BaseName);
            IsAutoIncrement = (bool) r["IsAutoIncrement"];
            IsRowVersion = CastDBNull(r["IsRowVersion"], false);
            IsHidden = CastDBNull(r["IsHidden"], false);
            IsLongBlob = (bool) r["IsLong"];
            IsReadOnly = (bool) r["IsReadOnly"];
            NonVersionedDbType = (SqlDbType) r["NonVersionedProviderType"];
            DbTypeName = r["DataTypeName"] as string;
            IsColumnSet = (bool) r["IsColumnSet"];
            DbTypeFullName = DbTypeName;
            if (Size != null)
                DbTypeFullName += "(" + (Size > 4000 || Size < 1 ? "MAX" : Size.ToString()) + ")";
            else if (NumericPrecision != null)
                DbTypeFullName += "(" + NumericPrecision + "," + NumericScale + ")";
        }

        public string Name { get; private set; } // the name of the column (can be aliased)
        public string BaseName { get; private set; } //the name of the original column if applicable (will be different if aliased)
        public int Ordinal { get; private set; } // the index of the column
        public int? Size { get; private set; } // the size of the datatype which applies to variable length data such as date/time/string/blob (null if not applicable)
        public short? NumericPrecision { get; private set; } // the numeric precision which applies to numeric types (null if not applicable)
        public short? NumericScale { get; private set; } // the numeric scale which applies to numeric types (null if not applicable)
        public bool IsUnique { get; private set; } // signifies that the column has a unique constraint (multiple rows cannot have the same value)
        public bool IsKey { get; private set; } // signifies that the column participates as part of the primary key
        public Type DataType { get; private set; } // the C# storage type for this column
        public bool AllowDBNull { get; private set; } // signifies if the column allows null values
        public SqlDbType DbType { get; private set; } // the database type 
        //public bool IsExpression { get; private set; }
        public bool IsAliased { get; private set; } // signifies if the column name is aliased
        public bool IsAutoIncrement { get; private set; } // signifies if the column auto-increments (only possible if IsIdentity is true)
        public bool IsRowVersion { get; private set; } // signifies if the column is a row version
        public bool IsHidden { get; private set; } // signifies if the column is hidden
        public bool IsLongBlob { get; private set; } // signifies if the column is a large blob
        public bool IsReadOnly { get; private set; } // signifies if the column is read-only
        public Type SqlType { get; private set; } // System.Data.SqlType which directly maps to the DbType
        public string DbTypeName { get; private set; } // SQL specific datatype
        public bool IsIdentity { get; private set; } // signifies if the column is an identity
        //public string XmlSchemaCollectionDatabase { get; private set; }
        //public string XmlSchemaCollectionOwningSchema { get; private set; }
        //public string XmlSchemaCollectionName { get; private set; }
        //public string UdtAssemblyQualifiedName { get; private set; }
        public SqlDbType NonVersionedDbType { get; private set; }
        public bool IsColumnSet { get; private set; }

        public string DbTypeFullName { get; private set; } // the full string of the type including size and numeric precision/scale

        public override string ToString()
        {
            return DbTypeFullName;
        }

        private static T CastDBNull<T>(object obj, T defaultVal)
        {
            return (obj == DBNull.Value) ? defaultVal : (T) obj;
        }
    }
}
