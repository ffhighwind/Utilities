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
		public override SqlConnection Connection()
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

		public override int BulkDelete(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.BulkDelete(conn, objs, null, commandTimeout);
			}
		}

		public override int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Delete(conn, whereCondition, param, null, true, commandTimeout);
			}
		}

		public override IEnumerable<T> DeleteList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<T> list = Queries.DeleteList(conn, whereCondition, param, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return list;
			}
			return new ConnectedEnumerable<T>(list, conn);
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

		public override IEnumerable<T> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<T> list = Queries.GetKeys(conn, whereCondition, param, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return list;
			}
			return new ConnectedEnumerable<T>(list, conn);
		}

		public override IEnumerable<T> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<T> list = Queries.GetList(conn, whereCondition, param, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return list;
			}
			return new ConnectedEnumerable<T>(list, conn);
		}

		public override T Insert(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Insert(conn, obj, null, commandTimeout);
			}
		}

		public override IEnumerable<T> BulkInsert(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<T> list = Queries.BulkInsert(conn, objs, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return list;
			}
			return new ConnectedEnumerable<T>(list, conn);
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

		public override int BulkUpdate(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.BulkUpdate(conn, objs, null, commandTimeout);
			}
		}

		public override T Upsert(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Upsert(conn, obj, null, commandTimeout);
			}
		}

		public override int BulkUpsert(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			return Queries.BulkUpsert(conn, objs, null, true, commandTimeout);
		}

		public override IEnumerable<T> BulkDeleteList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<T> list = Queries.BulkDeleteList(conn, objs, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return list;
			}
			return new ConnectedEnumerable<T>(list, conn);
		}

		public override IEnumerable<T> BulkUpdateList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<T> list = Queries.BulkUpdateList(conn, objs, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return list;
			}
			return new ConnectedEnumerable<T>(list, conn);
		}

		public override IEnumerable<T> BulkUpsertList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<T> list = Queries.BulkUpsertList(conn, objs, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return list;
			}
			return new ConnectedEnumerable<T>(list, conn);
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

		public override int BulkDelete(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Queries.BulkDelete(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.Delete(transaction.Connection, whereCondition, param, transaction, true, commandTimeout);
		}

		public override IEnumerable<T> DeleteList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
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

		public override IEnumerable<T> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.GetKeys(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override IEnumerable<T> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.GetList(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override T Insert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Insert(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override IEnumerable<T> BulkInsert(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.BulkInsert(transaction.Connection, objs, transaction, buffered, commandTimeout);
		}

		public override int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return Queries.RecordCount(transaction.Connection, whereCondition, param, transaction, commandTimeout);
		}

		public override bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Update(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override int BulkUpdate(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Queries.BulkUpdate(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override T Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Upsert(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override int BulkUpsert(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.BulkUpsert(transaction.Connection, objs, transaction, buffered, commandTimeout);
		}

		public override IEnumerable<T> BulkDeleteList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.BulkDeleteList<T>(transaction.Connection, objs, transaction, buffered, commandTimeout);
		}

		public override IEnumerable<T> BulkUpdateList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.BulkUpdateList(transaction.Connection, objs, transaction, buffered, commandTimeout);
		}

		public override IEnumerable<T> BulkUpsertList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.BulkUpsertList(transaction.Connection, objs, transaction, buffered, commandTimeout);
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
		public override SqlConnection Connection()
		{
			return new SqlConnection(ConnectionString);
		}

		#region IDataAccessObjectSync<T, KeyType, Ret>
		public override bool Delete(KeyType key, int? commandTimeout = null)
		{
			return Delete(TableData<T>.CreateKey<KeyType>(key), commandTimeout);
		}

		public override int BulkDelete(IEnumerable<KeyType> keys, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.BulkDelete(conn, keys, null, commandTimeout);
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

		public override int BulkDelete(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.BulkDelete(conn, objs, null, commandTimeout);
			}
		}

		public override int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Delete(conn, whereCondition, param, null, true, commandTimeout);
			}
		}

		public override IEnumerable<KeyType> DeleteList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<KeyType> keys = Queries.DeleteList<KeyType>(conn, whereCondition, param, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return keys;
			}
			return new ConnectedEnumerable<KeyType>(keys, conn);
		}

		public override T Get(KeyType key, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Get(conn, key, null, commandTimeout);
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

		public override IEnumerable<KeyType> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<KeyType> keys = Queries.GetKeys<KeyType>(conn, whereCondition, param, null, buffered, commandTimeout);
			if(buffered) {
				conn.Dispose(); // conn.Close();
				return keys;
			}
			return new ConnectedEnumerable<KeyType>(keys, conn);
		}

		public override IEnumerable<T> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<T> objs = Queries.GetList(conn, whereCondition, param, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return objs;
			}
			return new ConnectedEnumerable<T>(objs, conn);
		}

		public override T Insert(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Insert(conn, obj, null, commandTimeout);
			}
		}

		public override IEnumerable<T> BulkInsert(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<T> list = Queries.BulkInsert(conn, objs, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return list;
			}
			return new ConnectedEnumerable<T>(list, conn);
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

		public override int BulkUpdate(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.BulkUpdate(conn, objs, null, commandTimeout);
			}
		}

		public override T Upsert(T obj, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.Upsert(conn, obj, null, commandTimeout);
			}
		}

		public override int BulkUpsert(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString)) {
				return Queries.BulkUpsert(conn, objs, null, true, commandTimeout);
			}
		}

		public override IEnumerable<KeyType> BulkDeleteList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<KeyType> keys = Queries.BulkDeleteList<KeyType>(conn, objs, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return keys;
			}
			return new ConnectedEnumerable<KeyType>(keys, conn);
		}


		public override IEnumerable<T> BulkUpdateList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<T> list = Queries.BulkUpdateList(conn, objs, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return list;
			}
			return new ConnectedEnumerable<T>(list, conn);
		}

		public override IEnumerable<T> BulkUpsertList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			IEnumerable<T> list = Queries.BulkUpsertList(conn, objs, null, buffered, commandTimeout);
			if (buffered) {
				conn.Dispose(); // conn.Close();
				return list;
			}
			return new ConnectedEnumerable<T>(list, conn);
		}
		#endregion // IDataAccessObjectSync<T, KeyType, Ret>


		#region ITransactionQueriesSync<T, KeyType, Ret>
		public override IEnumerable<KeyType> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.GetKeys<KeyType>(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override bool Delete(IDbTransaction transaction, KeyType key, int? commandTimeout = null)
		{
			return Queries.Delete(transaction.Connection, key, transaction, commandTimeout);
		}

		public override int BulkDelete(SqlTransaction transaction, IEnumerable<KeyType> objs, int? commandTimeout = null)
		{
			return Queries.BulkDelete<KeyType>(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override T Get(IDbTransaction transaction, KeyType key, int? commandTimeout = null)
		{
			return Queries.Get(transaction.Connection, key, transaction, commandTimeout);
		}

		public override IEnumerable<KeyType> DeleteList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
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

		public override int BulkDelete(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Queries.BulkDelete(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.Delete(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override T Insert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Insert(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override IEnumerable<T> BulkInsert(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.BulkInsert(transaction.Connection, objs, transaction, buffered, commandTimeout);
		}

		public override bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Update(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override int BulkUpdate(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Queries.BulkUpdate(transaction.Connection, objs, transaction, commandTimeout);
		}

		public override T Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Upsert(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override int BulkUpsert(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.BulkUpsert(transaction.Connection, objs, transaction, buffered, commandTimeout);
		}

		public override T Get(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return Queries.Get(transaction.Connection, key, transaction, commandTimeout);
		}

		public override T Get(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Queries.Get(transaction.Connection, obj, transaction, commandTimeout);
		}

		public override IEnumerable<T> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.GetList(transaction.Connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public override int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return Queries.RecordCount(transaction.Connection, whereCondition, param, transaction, commandTimeout);
		}

		public override IEnumerable<KeyType> BulkDeleteList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.BulkDeleteList<KeyType>(transaction.Connection, objs, transaction, buffered, commandTimeout);
		}

		public override IEnumerable<T> BulkUpdateList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.BulkUpdateList(transaction.Connection, objs, transaction, buffered, commandTimeout);
		}

		public override IEnumerable<T> BulkUpsertList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return Queries.BulkUpsertList(transaction.Connection, objs, transaction, buffered, commandTimeout);
		}
		#endregion // ITransactionQueriesSync<T, KeyType, Ret>
	}
}
