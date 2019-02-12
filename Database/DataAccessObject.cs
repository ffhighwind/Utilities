using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension;
using Dapper.Extension.Interfaces;

namespace Dapper
{
	public class DataAccessObject<T> : IDataAccessObject<T> where T : class
	{
		public DataAccessObject(string connString, ITableData<T> queries = null)
		{
			ConnectionString = connString;
			Queries = queries ?? TableData<T>.Queries;
		}

		public PropertyInfo[] Properties => Queries.Properties;
		public PropertyInfo[] KeyProperties => Queries.KeyProperties;

		public ITableData<T> Queries { get; private set; }

		public string ConnectionString { get; private set; }
		public override IDbConnection Connection()
		{
			return new SqlConnection(ConnectionString);
		}


		#region IDataAccessObjectSync<T>
		public override bool Delete(object key, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Delete(conn, key, null, commandTimeout);
			}
		}

		public override bool Delete(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Delete(conn, obj, null, commandTimeout);
			}
		}

		public override int Delete(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Delete(conn, objs, null, commandTimeout);
			}
		}

		public override int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Delete(conn, whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public override List<T> DeleteList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.DeleteList(conn, whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public override T Get(object key, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Get(conn, key, null, commandTimeout);
			}
		}

		public override T Get(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Get(conn, obj, null, commandTimeout);
			}
		}

		public override List<T> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.GetKeys(conn, whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public override List<T> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.GetList(conn, whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public override T Insert(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Insert(conn, obj, null, commandTimeout);
			}
		}

		public override IEnumerable<T> Insert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Insert(conn, objs, null, commandTimeout);
			}
		}

		public override int RecordCount(string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.RecordCount(conn, whereCondition, param, null, commandTimeout);
			}
		}

		public override bool Update(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Update(conn, obj, null, commandTimeout);
			}
		}

		public override int Update(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Update(conn, objs, null, commandTimeout);
			}
		}

		public override T Upsert(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Upsert(conn, obj, null, commandTimeout);
			}
		}

		public override IEnumerable<T> Upsert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Upsert(conn, objs, null, commandTimeout);
			}
		}
		#endregion IDataAccessObjectSync<T>


		#region ITransactionQueriesSync<T>
		public override bool Delete(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return Queries.Delete(transaction.Connection, key, transaction, commandTimeout);
		}

		public override bool Delete(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Delete(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override int Delete(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Queries.Delete(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.Delete(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override List<T> DeleteList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.DeleteList(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override T Get(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return Queries.Get(transaction.Connection, key, transaction, commandTimeout);
		}

		public override T Get(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Get(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override List<T> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.GetKeys(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override List<T> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.GetList(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override T Insert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Insert(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override IEnumerable<T> Insert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Queries.Insert(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return Queries.RecordCount(transaction.Connection, whereCondition, param, transaction, commandTimeout);
		}

		public override bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Update(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override int Update(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Queries.Update(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override T Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Upsert(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override IEnumerable<T> Upsert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Queries.Upsert(transaction.Connection, objs, transaction, commandTimeout);
		}
		#endregion // ITransactionQueriesSync<T>
	}



	public class DataAccessObject<T, KeyType> : IDataAccessObject<T, KeyType, T>
		where T : class
	{
		public DataAccessObject(string connString, ITableData<T> queries = null)
		{
			if (TableData<T>.KeyProperties.Length != 1) {
				throw new InvalidOperationException(typeof(T).Name + " must have one KeyAttribute.");
			}
			ConnectionString = connString;
			Queries = queries ?? TableData<T>.Queries;
		}

		public PropertyInfo[] Properties => Queries.Properties;
		public PropertyInfo[] KeyProperties => Queries.KeyProperties;

		public ITableData<T> Queries { get; private set; }

		public string ConnectionString { get; private set; }
		public override IDbConnection Connection()
		{
			return new SqlConnection(ConnectionString);
		}

		#region IDataAccessObjectSync<T, KeyType, Ret>
		public override bool Delete(KeyType key, int? commandTimeout = null)
		{
			return Delete(TableData<T>.CreateKey<KeyType>(key), commandTimeout);
		}

		public override int Delete(IEnumerable<KeyType> keys, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Delete(conn, keys, null, commandTimeout);
			}
		}

		public override bool Delete(object key, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Delete(conn, key, null, commandTimeout);
			}
		}

		public override bool Delete(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Delete(conn, obj, null, commandTimeout);
			}
		}

		public override int Delete(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Delete(conn, objs, null, commandTimeout);
			}
		}

		public override int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Delete(conn, whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public override List<KeyType> DeleteList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.DeleteList<KeyType>(conn, whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public override T Get(KeyType key, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Get<KeyType>(conn, key, null, commandTimeout);
			}
		}

		public override T Get(object key, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Get(conn, key, null, commandTimeout);
			}
		}

		public override T Get(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Get(conn, obj, null, commandTimeout);
			}
		}

		public override List<KeyType> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.GetKeys<KeyType>(conn, whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public override List<T> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.GetList(conn, whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public override T Insert(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Insert(conn, obj, null, commandTimeout);
			}
		}

		public override IEnumerable<T> Insert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Insert(conn, objs, null, commandTimeout);
			}
		}

		public override int RecordCount(string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.RecordCount(conn, whereCondition, param, null, commandTimeout);
			}
		}

		public override bool Update(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Update(conn, obj, null, commandTimeout);
			}
		}

		public override int Update(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Update(conn, objs, null, commandTimeout);
			}
		}

		public override T Upsert(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Upsert(conn, obj, null, commandTimeout);
			}
		}

		public override IEnumerable<T> Upsert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Upsert(conn, objs, null, commandTimeout);
			}
		}
		#endregion // IDataAccessObjectSync<T, KeyType, Ret>


		#region ITransactionQueriesSync<T, KeyType, Ret>
		public override List<KeyType> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.GetKeys<KeyType>(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override bool Delete(IDbTransaction transaction, KeyType key, int? commandTimeout = null)
		{
			return Queries.Delete(transaction.Connection, key, transaction, commandTimeout);
		}

		public override int Delete(IDbTransaction transaction, IEnumerable<KeyType> objs, int? commandTimeout = null)
		{
			return Queries.Delete<KeyType>(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override T Get(IDbTransaction transaction, KeyType key, int? commandTimeout = null)
		{
			return Queries.Get(transaction.Connection, key, transaction, commandTimeout);
		}

		public override List<KeyType> DeleteList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.DeleteList<KeyType>(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override bool Delete(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return Queries.Delete(transaction.Connection, key, transaction, commandTimeout);
		}

		public override bool Delete(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Delete(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override int Delete(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Queries.Delete(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.Delete(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override T Insert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Insert(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override IEnumerable<T> Insert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Queries.Insert(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Update(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override int Update(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Queries.Update(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override T Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Upsert(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override IEnumerable<T> Upsert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Queries.Upsert(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override T Get(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return Queries.Get(transaction.Connection, key, transaction, commandTimeout);
		}

		public override T Get(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Get(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override List<T> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.GetList(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return Queries.RecordCount(transaction.Connection, whereCondition, param, transaction, commandTimeout);
		}
		#endregion // ITransactionQueriesSync<T, KeyType, Ret>
	}
}
