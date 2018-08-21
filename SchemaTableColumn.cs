using System;
using System.Data;

namespace Utilities
{
    /// <summary>
    /// SchemaTableColumn exists in .NET 4.7.2
    /// https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-schema-collections?view=netframework-4.7.2
    /// https://docs.microsoft.com/en-us/sql/relational-databases/system-information-schema-views/system-information-schema-views-transact-sql?view=sql-server-2017
    /// https://docs.microsoft.com/en-us/dotnet/api/system.data.datatablereader.getschematable?redirectedfrom=MSDN&view=netframework-4.7.2#System_Data_DataTableReader_GetSchemaTable
    /// </summary>
    public class SchemaTableColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaTableColumn"/> class.
        /// </summary>
        /// <param name="schema">The results from <see cref="System.Data.SqlClient.SqlDataReader.GetSchemaTable"/>.</param>
        /// <param name="rowIndex">The row index into the schema <see cref="DataTable"/> that represents this column.</param>
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
            DataType = (Type) r["DataType"];
            AllowDBNull = (bool) r["AllowDBNull"];
            DbType = (SqlDbType) r["ProviderType"];
            SqlType = (Type) r["ProviderSpecificDataType"];
            ////IsExpression = CastDBNull<bool>(r["IsExpression"], false);
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

        /// <summary>
        /// The name of the column. This can be an alias.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The name of the original column if applicable. This will be different than <see cref="Name"/> if the column is aliased.
        /// </summary>
        public string BaseName { get; private set; }

        /// <summary>
        /// The index of the column.
        /// </summary>
        public int Ordinal { get; private set; }

        /// <summary>
        /// The size of the datatype in bytes. This applies to variable length data such as date/time/string/blob.
        /// This is null if the column type does not have variable size.
        /// </summary>
        public int? Size { get; private set; }

        /// <summary>
        /// The numeric precision. This is null if the column type is not numeric.
        /// </summary>
        public short? NumericPrecision { get; private set; }

        /// <summary>
        /// The numeric scale. This is null if the column type is not numeric.
        /// </summary>
        public short? NumericScale { get; private set; }

        /// <summary>
        /// Signifies if the column is part of a unique constraint.
        /// </summary>
        public bool IsUnique { get; private set; }

        /// <summary>
        /// Signifies if the column is part of the primary key.
        /// </summary>
        public bool IsKey { get; private set; }

        /// <summary>
        /// The <see cref="RuntimeType"/> for this column.
        /// </summary>
        public Type DataType { get; private set; }

        /// <summary>
        /// Signifies if the column allows null values.
        /// </summary>
        public bool AllowDBNull { get; private set; }

        /// <summary>
        /// The <see cref="SqlDbType"/> enum for this column.
        /// </summary>
        public SqlDbType DbType { get; private set; }

        /// <summary>
        /// Signifies if the column is aliased.
        /// </summary>
        public bool IsAliased { get; private set; }

        /// <summary>
        /// Signifies if the column auto-increments. This can only be true if <see cref="IsIdentity"/> is true.
        /// </summary>
        public bool IsAutoIncrement { get; private set; }

        /// <summary>
        /// Signifies if the column is a row version.
        /// </summary>
        public bool IsRowVersion { get; private set; }

        /// <summary>
        /// Signifies if the column is hidden.
        /// </summary>
        public bool IsHidden { get; private set; }

        /// <summary>
        /// Signifies if the column is a large blob. The number of bytes that determines this depends on the database.
        /// </summary>
        public bool IsLongBlob { get; private set; }

        /// <summary>
        /// Signifies if the column is read-only.
        /// </summary>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// The <see cref="System.Data.SqlTypes."/><see cref="Type"/> that directly maps to <see cref="DbType"/>.
        /// </summary>
        public Type SqlType { get; private set; }

        /// <summary>
        /// The string representation of the <see cref="SqlDbType"/>.
        /// </summary>
        public string DbTypeName { get; private set; }

        /// <summary>
        /// Signifies if the column is an identity.
        /// </summary>
        public bool IsIdentity { get; private set; }

        /// <summary>
        /// The non-versioned <see cref="SqlDbType"/> enum.
        /// </summary>
        public SqlDbType NonVersionedDbType { get; private set; }

        /// <summary>
        /// Signifies if the column represents a column set.
        /// </summary>
        public bool IsColumnSet { get; private set; }

        /// <summary>
        /// The string representation of the <see cref="SqlDbType"/> including the <see cref="Size"/>, <see cref="NumericPrecision"/>, and <see cref="NumericScale"/>.
        /// </summary>
        public string DbTypeFullName { get; private set; } // the full string of the type including size and numeric precision/scale

        /// <summary>
        /// Equivalent to <see cref="DbTypeFullName"/>.
        /// </summary>
        /// <returns>The results from <see cref="DbTypeFullName"/>.</returns>
        public override string ToString()
        {
            return DbTypeFullName;
        }

        ////public bool IsExpression { get; private set; }
        ////public string XmlSchemaCollectionDatabase { get; private set; }
        ////public string XmlSchemaCollectionOwningSchema { get; private set; }
        ////public string XmlSchemaCollectionName { get; private set; }
        ////public string UdtAssemblyQualifiedName { get; private set; }

        private static T CastDBNull<T>(object obj, T defaultVal)
        {
            return (obj == DBNull.Value) ? defaultVal : (T) obj;
        }
    }
}
