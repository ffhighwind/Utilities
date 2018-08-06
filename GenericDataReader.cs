using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Data.Common;
using System.Collections;

namespace Utilities
{
    /// <summary>
    /// https://github.com/mgravell/fast-member/blob/master/FastMember/ObjectReader.cs
    /// </summary>
    public class GenericDataReader<T> : DbDataReader where T : class
    {
        private IEnumerator enumerator;
        private readonly PropertyInfo[] pinfos;
        private readonly string[] propertyNames;
        private readonly Type[] types;
        private readonly BitArray allowNull;
        private object current;
        private bool active = true;
        private const BindingFlags defaultFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        /// <summary>
        /// Creates a new ObjectReader instance for reading the supplied data
        /// </summary>
        /// <param name="type">The expected Type of the information to be read</param>
        /// <param name="source">The sequence of objects to represent</param>
        /// <param name="members">The members that should be exposed to the reader</param>
        public GenericDataReader(IEnumerable<T> source, BindingFlags flags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly, params string[] members)
        {
            pinfos = typeof(T).GetProperties(flags);
            this.enumerator = source.GetEnumerator();
            this.current = null;
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
        /// Creates a new ObjectReader instance for reading the supplied data
        /// </summary>
        /// <param name="type">The expected Type of the information to be read</param>
        /// <param name="source">The sequence of objects to represent</param>
        /// <param name="members">The members that should be exposed to the reader</param>
        public GenericDataReader(IEnumerable<T> source, params string[] members)
        {
            pinfos = typeof(T).GetProperties(defaultFlags);
            this.enumerator = source.GetEnumerator();
            this.current = null;
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

        public override int Depth {
            get { throw new NotImplementedException(); }
        }

        public override DataTable GetSchemaTable()
        {
            // these are the columns used by DataTable load
            DataTable table = new DataTable {
                Columns =
                {
                    {"ColumnOrdinal", typeof(int)},
                    {"ColumnName", typeof(string)},
                    {"DataType", typeof(Type)},
                    {"ColumnSize", typeof(int)},
                    {"AllowDBNull", typeof(bool)}
                }
            };
            object[] rowData = new object[5];
            for (int i = 0; i < propertyNames.Length; i++) {
                rowData[0] = i;
                rowData[1] = propertyNames[i];
                rowData[2] = types == null ? typeof(object) : types[i];
                rowData[3] = -1;
                rowData[4] = allowNull == null ? true : allowNull[i];
                table.Rows.Add(rowData);
            }
            return table;
        }

        public override void Close()
        {
            Shutdown();
        }

        public override bool HasRows {
            get {
                return active;
            }
        }

        public override bool NextResult()
        {
            active = false;
            return false;
        }

        public override bool Read()
        {
            if (active) {
                var tmp = enumerator;
                if (tmp != null && tmp.MoveNext()) {
                    current = tmp.Current;
                    return true;
                }
                else {
                    active = false;
                }
            }
            current = null;
            return false;
        }

        public override int RecordsAffected {
            get { return 0; }
        }

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
            IDisposable tmp = enumerator as IDisposable;
            enumerator = null;
            if (tmp != null)
                tmp.Dispose();
        }

        public override int FieldCount {
            get { return propertyNames.Length; }
        }

        public override bool IsClosed {
            get {
                return enumerator == null;
            }
        }

        public override bool GetBoolean(int i)
        {
            return (bool) this[i];
        }

        public override byte GetByte(int i)
        {
            return (byte) this[i];
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            byte[] s = (byte[]) this[i];
            int available = s.Length - (int) fieldOffset;
            if (available <= 0)
                return 0;

            int count = Math.Min(length, available);
            Buffer.BlockCopy(s, (int) fieldOffset, buffer, bufferoffset, count);
            return count;
        }

        public override char GetChar(int i)
        {
            return (char) this[i];
        }

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            string s = (string) this[i];
            int available = s.Length - (int) fieldoffset;
            if (available <= 0)
                return 0;

            int count = Math.Min(length, available);
            s.CopyTo((int) fieldoffset, buffer, bufferoffset, count);
            return count;
        }

        protected override DbDataReader GetDbDataReader(int i)
        {
            throw new NotSupportedException();
        }

        public override string GetDataTypeName(int i)
        {
            return types[i].Name;
        }

        public override DateTime GetDateTime(int i)
        {
            return (DateTime) this[i];
        }

        public override decimal GetDecimal(int i)
        {
            return (decimal) this[i];
        }

        public override double GetDouble(int i)
        {
            return (double) this[i];
        }

        public override Type GetFieldType(int i)
        {
            return types[i];
        }

        public override float GetFloat(int i)
        {
            return (float) this[i];
        }

        public override Guid GetGuid(int i)
        {
            return (Guid) this[i];
        }

        public override short GetInt16(int i)
        {
            return (short) this[i];
        }

        public override int GetInt32(int i)
        {
            return (int) this[i];
        }

        public override long GetInt64(int i)
        {
            return (long) this[i];
        }

        public override string GetName(int i)
        {
            return propertyNames[i];
        }

        public override int GetOrdinal(string name)
        {
            return Array.IndexOf(propertyNames, name);
        }

        public override string GetString(int i)
        {
            return (string) this[i];
        }

        public override object GetValue(int i)
        {
            return this[i];
        }

        public override IEnumerator GetEnumerator() => new DbEnumerator(this);

        public override int GetValues(object[] values)
        {
            for (int i = 0; i < propertyNames.Length; i++)
                values[i] = pinfos[i].GetValue(current) ?? DBNull.Value;
            return propertyNames.Length;
        }

        public override bool IsDBNull(int i)
        {
            return this[i] is DBNull;
        }

        public override object this[string name] {
            get { return pinfos.First(prop => prop.Name == name).GetValue(current) ?? DBNull.Value; }

        }

        /// <summary>
        /// Gets the value of the current object in the member specified
        /// </summary>
        public override object this[int i] {
            get { return pinfos[i].GetValue(current) ?? DBNull.Value; }
        }
    }
}