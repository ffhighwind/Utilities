using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Utilities
{
    /// <summary>
    /// Generic DbDataReader for use with ADO.NET SQL select commands.
    /// <see href="https://github.com/mgravell/fast-member/blob/master/FastMember/ObjectReader.cs"/>
    /// </summary>
    /// <typeparam name="T">The class type to read data into.</typeparam>
    public class GenericDataReader<T> : DbDataReader where T : class
    {
        private IEnumerator enumerator;
        private readonly PropertyInfo[] pinfos;
        private readonly string[] propertyNames;
        private readonly Type[] types;
        private readonly BitArray allowNull;
        private object current = null;
        private bool active = true;
        private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDataReader{T}"/> class.
        /// </summary>
        /// <param name="source">The list of objects to read.</param>
        /// <param name="flags">The BindingFlags for the type's properties.</param>
        /// <param name="members">The property names that will be read. If this is empty then all properties will be read.</param>
        public GenericDataReader(IEnumerable<T> source, BindingFlags flags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly, params string[] members)
        {
            pinfos = typeof(T).GetProperties(flags);
            if (members.Length > 0)
                pinfos = pinfos.Where(pinfo => members.Contains(pinfo.Name)).ToArray();
            enumerator = source.GetEnumerator();
            propertyNames = new string[pinfos.Length];
            types = new Type[pinfos.Length];
            allowNull = new BitArray(pinfos.Length);
            for (int i = 0; i < pinfos.Length; i++) {
                PropertyInfo pi = pinfos[i];
                propertyNames[i] = pi.Name;
                Type ty = Nullable.GetUnderlyingType(pi.PropertyType);
                allowNull[i] = ty != null;
                types[i] = ty ?? pi.PropertyType;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDataReader{T}"/> class.
        /// </summary>
        /// <param name="source">The list of objects to read.</param>
        /// <param name="members">The property names that will be read. If this is empty then all properties will be read.</param>
        public GenericDataReader(IEnumerable<T> source, params string[] members)
            : this(source, DefaultBindingFlags, members) { }

        public override int Depth => throw new NotImplementedException();

        public override DataTable GetSchemaTable()
        {
            DataTable table = new DataTable {
                Columns =
                {
                    { "ColumnOrdinal", typeof(int) },
                    { "ColumnName", typeof(string) },
                    { "DataType", typeof(Type) },
                    { "ColumnSize", typeof(int) },
                    { "AllowDBNull", typeof(bool) }
                }
            };
            object[] rowData = new object[5];
            for (int i = 0; i < propertyNames.Length; i++) {
                rowData[0] = i;
                rowData[1] = propertyNames[i];
                rowData[2] = types[i];
                rowData[3] = -1;
                rowData[4] = allowNull[i];
                table.Rows.Add(rowData);
            }
            return table;
        }

        public override void Close()
        {
            Shutdown();
        }

        public override bool HasRows => active;

        public override bool NextResult()
        {
            active = false;
            return false;
        }

        public override bool Read()
        {
            if (active) {
                if (enumerator != null && enumerator.MoveNext()) {
                    current = enumerator.Current;
                    return true;
                }
                active = false;
                current = null;
            }
            return false;
        }

        public override int RecordsAffected => 0;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                Shutdown();
        }

        private void Shutdown()
        {
            active = false;
            current = null;
            if (enumerator is IDisposable tmp)
                tmp.Dispose();
            enumerator = null;
        }

        public override int FieldCount => propertyNames.Length;

        public override bool IsClosed => enumerator == null;

        public override bool GetBoolean(int ordinal)
        {
            return (bool) this[ordinal];
        }

        public override byte GetByte(int ordinal)
        {
            return (byte) this[ordinal];
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            byte[] s = (byte[]) this[ordinal];
            int available = s.Length - (int) dataOffset;
            if (available <= 0)
                return 0;

            int count = Math.Min(length, available);
            Buffer.BlockCopy(s, (int) dataOffset, buffer, bufferOffset, count);
            return count;
        }

        public override char GetChar(int ordinal)
        {
            return (char) this[ordinal];
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            string s = (string) this[ordinal];
            int available = s.Length - (int) dataOffset;
            if (available <= 0)
                return 0;

            int count = Math.Min(length, available);
            s.CopyTo((int) dataOffset, buffer, bufferOffset, count);
            return count;
        }

        protected override DbDataReader GetDbDataReader(int ordinal)
        {
            throw new NotSupportedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            return types[ordinal].Name;
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return (DateTime) this[ordinal];
        }

        public override decimal GetDecimal(int ordinal)
        {
            return (decimal) this[ordinal];
        }

        public override double GetDouble(int ordinal)
        {
            return (double) this[ordinal];
        }

        public override Type GetFieldType(int ordinal)
        {
            return types[ordinal];
        }

        public override float GetFloat(int ordinal)
        {
            return (float) this[ordinal];
        }

        public override Guid GetGuid(int ordinal)
        {
            return (Guid) this[ordinal];
        }

        public override short GetInt16(int ordinal)
        {
            return (short) this[ordinal];
        }

        public override int GetInt32(int ordinal)
        {
            return (int) this[ordinal];
        }

        public override long GetInt64(int ordinal)
        {
            return (long) this[ordinal];
        }

        public override string GetName(int ordinal)
        {
            return propertyNames[ordinal];
        }

        public override int GetOrdinal(string name)
        {
            return Array.IndexOf(propertyNames, name);
        }

        public override string GetString(int ordinal)
        {
            return (string) this[ordinal];
        }

        public override object GetValue(int ordinal)
        {
            return this[ordinal];
        }

        public override IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this);
        }

        public override int GetValues(object[] values)
        {
            for (int i = 0; i < propertyNames.Length; i++)
                values[i] = pinfos[i].GetValue(current) ?? DBNull.Value;
            return propertyNames.Length;
        }

        public override bool IsDBNull(int ordinal)
        {
            return this[ordinal] == DBNull.Value;
        }

        public override object this[string name] => pinfos.First(prop => prop.Name == name).GetValue(current) ?? DBNull.Value;

        public override object this[int i] => pinfos[i].GetValue(current) ?? DBNull.Value;
    }
}