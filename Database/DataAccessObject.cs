using Dapper.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper
{
	public class DataAccessObject<T> : IDataAccessObject<T>, IDataAccessObjectAsync<T> where T : class
	{
		public DataAccessObject(string connString)
		{
			ConnectionString = connString;
		}

		public bool IsCachable => TableData<T>.IsCachable;
		public string TableName => TableData<T>.TableName;
		public PropertyInfo[] Properties => TableData<T>.Properties;
		public PropertyInfo[] KeyProperties => TableData<T>.KeyProperties;
		public string[] ColumnNames => TableData<T>.GetColumnNames(TableData<T>.Properties);
		public string[] KeyColumnNames => TableData<T>.GetColumnNames(TableData<T>.KeyProperties);

		public string ConnectionString { get; private set; }
		public IDbConnection GetConnection() => new SqlConnection(ConnectionString);


		#region IDataAccessObject<T>
		public List<T> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return conn.GetKeys<T>(whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public bool Delete(object key, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return conn.Delete<T>(key, null, commandTimeout);
			}
		}

		public bool Delete(T obj, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return conn.Delete<T>(obj, null, commandTimeout);
			}
		}

		public int Delete(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				using (IDbTransaction trans = conn.BeginTransaction()) {
					int count = conn.Delete(objs, trans, commandTimeout);
					trans.Commit();
					return count;
				}
			}
		}

		public int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				using (IDbTransaction trans = conn.BeginTransaction()) {
					int count = conn.Delete<T>(whereCondition, param, trans, buffered, commandTimeout);
					trans.Commit();
					return count;
				}
			}
		}

		public void Insert(T obj, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				conn.Insert<T>(obj, null, commandTimeout);
			}
		}

		public void Insert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				using (IDbTransaction trans = conn.BeginTransaction()) {
					conn.Insert<T>(objs, trans, commandTimeout);
					trans.Commit();
				}
			}
		}

		public bool Update(T obj, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return conn.Update<T>(obj, null, commandTimeout);
			}
		}

		public int Update(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				using (IDbTransaction trans = conn.BeginTransaction()) {
					int count = conn.Update(objs, trans, commandTimeout);
					trans.Commit();
					return count;
				}
			}
		}

		public void Upsert(T obj, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				conn.Upsert<T>(obj, null, commandTimeout);
			}
		}

		public void Upsert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				using (IDbTransaction trans = conn.BeginTransaction()) {
					conn.Upsert<T>(objs, trans, commandTimeout);
					trans.Commit();
				}
			}
		}

		public T Get(object key, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return conn.Get<T>(key, null, commandTimeout);
			}
		}

		public T Get(T obj, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return conn.Get<T>(obj, null, commandTimeout);
			}
		}

		public List<T> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return conn.GetList<T>(whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public int RecordCount(string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return conn.RecordCount<T>(whereCondition, param, null, commandTimeout);
			}
		}
		#endregion // IDataAccessObject<T>

		#region IDataAccessObjectAsync<T>
		public async Task<List<T>> GetKeysAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetKeys(whereCondition, param, buffered, commandTimeout));
		}

		public async Task<bool> DeleteAsync(object key, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(key, commandTimeout));
		}

		public async Task<bool> DeleteAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(obj, commandTimeout));
		}

		public async Task<int> DeleteAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(objs, commandTimeout));
		}

		public async Task<int> DeleteAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(whereCondition, param, buffered, commandTimeout));
		}

		public async Task InsertAsync(T obj, int? commandTimeout = null)
		{
			await Task.Run(() => Insert(obj, commandTimeout));
		}

		public async Task InsertAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			await Task.Run(() => Insert(objs, commandTimeout));
		}

		public async Task<bool> UpdateAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(obj, commandTimeout));
		}

		public async Task<int> UpdateAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(objs, commandTimeout));
		}

		public async Task UpsertAsync(T obj, int? commandTimeout = null)
		{
			await Task.Run(() => Upsert(obj, commandTimeout));
		}

		public async Task UpsertAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			await Task.Run(() => Upsert(objs, commandTimeout));
		}

		public async Task<T> GetAsync(object key, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(key, commandTimeout));
		}

		public async Task<T> GetAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(obj, commandTimeout));
		}

		public async Task<List<T>> GetListAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetList(whereCondition, param, buffered, commandTimeout));
		}

		public async Task<int> RecordCountAsync(string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return await Task.Run(() => RecordCount(whereCondition, param, commandTimeout));
		}
		#endregion // IDataAccessObjectAsync<T>

		#region ITransactionQueries<T>
		public List<T> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return transaction.Connection.GetKeys<T>(whereCondition, param, transaction, buffered, commandTimeout);
		}

		public bool Delete(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return transaction.Connection.Delete<T>(key, transaction, commandTimeout);
		}

		public bool Delete(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return transaction.Connection.Delete<T>(obj, transaction, commandTimeout);
		}

		public int Delete(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return transaction.Connection.Delete<T>(objs, transaction, commandTimeout);
		}

		public int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return transaction.Connection.Delete<T>(whereCondition, param, transaction, buffered, commandTimeout);
		}

		public void Insert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			transaction.Connection.Insert<T>(obj, transaction, commandTimeout);
		}

		public void Insert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			transaction.Connection.Insert<T>(objs, transaction, commandTimeout);
		}

		public bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return transaction.Connection.Update<T>(obj, transaction, commandTimeout);
		}

		public int Update(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return transaction.Connection.Update<T>(objs, transaction, commandTimeout);
		}

		public void Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			transaction.Connection.Upsert<T>(obj, transaction, commandTimeout);
		}

		public void Upsert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			transaction.Connection.Upsert<T>(objs, transaction, commandTimeout);
		}

		public T Get(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return transaction.Connection.Get<T>(key, transaction, commandTimeout);
		}

		public T Get(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return transaction.Connection.Get<T>(obj, transaction, commandTimeout);
		}

		public List<T> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return transaction.Connection.GetList<T>(whereCondition, param, transaction, buffered, commandTimeout);
		}

		public int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return transaction.Connection.RecordCount<T>(whereCondition, param, transaction, commandTimeout);
		}
		#endregion // ITableQueries<T>

		#region ITransactionQueriesAsync<T>
		public async Task<List<T>> GetKeysAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetKeys(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<bool> DeleteAsync(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, key, commandTimeout));
		}

		public async Task<bool> DeleteAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, obj, commandTimeout));
		}

		public async Task<int> DeleteAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, objs, commandTimeout));
		}

		public async Task<int> DeleteAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task InsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			await Task.Run(() => Insert(transaction, obj, commandTimeout));
		}

		public async Task InsertAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			await Task.Run(() => Insert(transaction, objs, commandTimeout));
		}

		public async Task<bool> UpdateAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(transaction, obj, commandTimeout));
		}

		public async Task<int> UpdateAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(transaction, objs, commandTimeout));
		}

		public async Task UpsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			await Task.Run(() => Upsert(transaction, obj, commandTimeout));
		}

		public async Task UpsertAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			await Task.Run(() => Upsert(transaction, objs, commandTimeout));
		}

		public async Task<T> GetAsync(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(transaction, key, commandTimeout));
		}

		public async Task<T> GetAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(transaction, obj, commandTimeout));
		}

		public async Task<List<T>> GetListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetList(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<int> RecordCountAsync(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return await Task.Run(() => RecordCount(transaction, whereCondition, param, commandTimeout));
		}
		#endregion // ITransactionQueriesAsync<T>
	}
}
