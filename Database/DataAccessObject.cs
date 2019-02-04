using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Utilities.Database
{
	public class DataAccessObject<T> : ITableQueries<T>, ITableQueriesAsync<T>, IDataAccessObject<T>, IDataAccessObjectAsync<T> where T : class
	{
		public DataAccessObject(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public string ConnectionString { get; private set; }
		public IDbConnection GetConnection() => new SqlConnection(ConnectionString);

		public async Task<List<T>> GetKeysAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetKeys(whereCondition, param, buffered, commandTimeout));
		}

		public List<T> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return GetKeys(conn, whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public async Task<List<T>> GetKeysAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetKeys(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public List<T> GetKeys(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(TableData<T>.SelectListKeysQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		#region IDataAccessObject<T>
		public bool Delete(object key, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return Delete(conn, key, null, commandTimeout);
			}
		}

		public bool Delete(T obj, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return Delete(conn, obj, null, commandTimeout);
			}
		}

		public int Delete(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return Delete(conn, objs, null, commandTimeout);
			}
		}

		public int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return Delete(conn, whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public void Insert(T obj, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				Insert(conn, obj, null, commandTimeout);
			}
		}

		public void Insert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				Insert(conn, objs, null, commandTimeout);
			}
		}

		public bool Update(T obj, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return Update(conn, obj, null, commandTimeout);
			}
		}

		public int Update(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return Update(conn, objs, null, commandTimeout);
			}
		}

		public void Upsert(T obj, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				Upsert(conn, obj, null, commandTimeout);
			}
		}

		public void Upsert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				Upsert(conn, objs, null, commandTimeout);
			}
		}

		public T Get(object key, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return Get(conn, key, null, commandTimeout);
			}
		}

		public T Get(T obj, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return Get(conn, obj, null, commandTimeout);
			}
		}

		public List<T> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return GetList(conn, whereCondition, param, null, buffered, commandTimeout);
			}
		}

		public int RecordCount(string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			using (IDbConnection conn = GetConnection()) {
				return RecordCount(conn, whereCondition, param, null, commandTimeout);
			}
		}
		#endregion // IDataAccessObject<T>

		#region IDataAccessObjectAsync<T>
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

		#region ITableQueries<T>
		public bool Delete(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return 0 < connection.Execute(TableData<T>.DeleteSingleQuery, key, transaction, commandTimeout);
		}

		public bool Delete(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return 0 < connection.Execute(TableData<T>.DeleteSingleQuery, obj, transaction, commandTimeout);
		}

		public int Delete(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			using (IDbTransaction tempTransaction = transaction != null ? null : connection.BeginTransaction()) {
				transaction = transaction ?? tempTransaction;
				int count = 0;
				// TODO: Partition<> with keys
				foreach (T obj in objs) {
					if (Delete(connection, obj, transaction, commandTimeout)) {
						count++;
					}
				}
				tempTransaction?.Commit();
				return count;
			}
		}

		public int Delete(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Execute(TableData<T>.DeleteQuery + whereCondition, param, transaction, commandTimeout);
		}

		public void Insert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			connection.Execute(TableData<T>.InsertQuery, obj, transaction, commandTimeout);
		}

		public void Insert(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			using (IDbTransaction tempTransaction = transaction != null ? null : connection.BeginTransaction()) {
				transaction = transaction ?? tempTransaction;
				foreach (T obj in objs) {
					Insert(connection, obj, transaction, commandTimeout);
				}
				tempTransaction?.Commit();
			}
		}

		public bool Update(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return 0 < connection.ExecuteScalar<int>(TableData<T>.UpdateQuery, obj, transaction, commandTimeout);
		}

		public int Update(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			using (IDbTransaction tempTransaction = transaction != null ? null : connection.BeginTransaction()) {
				transaction = transaction ?? tempTransaction;
				int count = 0;
				foreach (T obj in objs) {
					if (Update(connection, obj, transaction, commandTimeout)) {
						count++;
					}
				}
				tempTransaction?.Commit();
				return count;
			}
		}

		public void Upsert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			connection.Execute(TableData<T>.UpsertQuery, obj, transaction, commandTimeout);
		}

		public void Upsert(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			using (IDbTransaction tempTransaction = transaction != null ? null : connection.BeginTransaction()) {
				transaction = transaction ?? tempTransaction;
				foreach (T obj in objs) {
					Upsert(connection, obj, transaction, commandTimeout);
				}
				tempTransaction?.Commit();
			}
		}

		public T Get(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.Query<T>(TableData<T>.SelectSingleQuery, key, transaction, true, commandTimeout).SingleOrDefault();
		}

		public T Get(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.Query<T>(TableData<T>.SelectSingleQuery, obj, transaction, true, commandTimeout).SingleOrDefault();
		}

		public List<T> GetList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(TableData<T>.SelectListQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public int RecordCount(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.ExecuteScalar<int>(TableData<T>.CountQuery + whereCondition, param, transaction, commandTimeout);
		}
		#endregion // ITableQueries<T>

		#region ITableQueriesAsync<T>
		public async Task<bool> DeleteAsync(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, key, transaction, commandTimeout));
		}

		public async Task<bool> DeleteAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, obj, transaction, commandTimeout));
		}

		public async Task<int> DeleteAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, objs, transaction, commandTimeout));
		}

		public async Task<int> DeleteAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public async Task InsertAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			await Task.Run(() => Insert(connection, obj, transaction, commandTimeout));
		}

		public async Task InsertAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			await Task.Run(() => Insert(connection, objs, transaction, commandTimeout));
		}

		public async Task<bool> UpdateAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(connection, obj, transaction, commandTimeout));
		}

		public async Task<int> UpdateAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(connection, objs, transaction, commandTimeout));
		}

		public async Task UpsertAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			await Task.Run(() => Upsert(connection, obj, transaction, commandTimeout));
		}

		public async Task UpsertAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			await Task.Run(() => Upsert(connection, objs, transaction, commandTimeout));
		}

		public async Task<T> GetAsync(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(connection, key, transaction, commandTimeout));
		}

		public async Task<T> GetAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(connection, obj, transaction, commandTimeout));
		}

		public async Task<List<T>> GetListAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetList(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public async Task<int> RecordCountAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => RecordCount(connection, whereCondition, param, transaction, commandTimeout));
		}
		#endregion // ITableQueriesAsync<T>
	}
}
