using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Interfaces
{
	public abstract class IConnectionQueries<T> : IConnectionQueriesSync<T>, IConnectionQueriesAsync<T> where T : class
	{
		#region IConnectionQueriesSync<T>
		public abstract bool Delete<KeyType>(IDbConnection connection, KeyType key, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract bool Delete(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract bool Delete(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract int Delete(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		public abstract List<KeyType> DeleteList<KeyType>(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		public abstract List<T> DeleteList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		public abstract T Get<KeyType>(IDbConnection connection, KeyType key, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract T Get(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract T Get(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract List<KeyType> GetKeys<KeyType>(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		public abstract List<T> GetKeys(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		public abstract List<T> GetList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		public abstract T Insert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract int RecordCount(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract bool Update(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract T Upsert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);

		public abstract int BulkDelete<KeyType>(SqlConnection connection, IEnumerable<KeyType> keys, SqlTransaction transaction = null, int? commandTimeout = null);
		public abstract int BulkDelete(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null);
		public abstract IEnumerable<T> BulkInsert(SqlBulkCopy bulkCopy, IEnumerable<T> objs, int? commandTimeout);
		public abstract IEnumerable<T> BulkInsert(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null);
		public abstract int BulkUpdate(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null);
		public abstract IEnumerable<T> BulkUpsert(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null);
		public abstract List<KeyType> BulkDeleteList<KeyType>(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		public abstract List<T> BulkDeleteList(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		#endregion // IConnectionQueriesSync<T>


		#region IConnectionQueriesAsync<T>
		public async Task<bool> DeleteAsync<KeyType>(IDbConnection connection, KeyType key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, key, transaction, commandTimeout));
		}

		public async Task<int> BulkDeleteAsync<KeyType>(SqlConnection connection, IEnumerable<KeyType> keys, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDelete(connection, keys, transaction, commandTimeout));
		}

		public async Task<bool> DeleteAsync(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, key, transaction, commandTimeout));
		}

		public async Task<bool> DeleteAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, obj, transaction, commandTimeout));
		}

		public async Task<int> BulkDeleteAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDelete(connection, objs, transaction, commandTimeout));
		}

		public async Task<int> DeleteAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public async Task<List<KeyType>> DeleteListAsync<KeyType>(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => DeleteList<KeyType>(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public async Task<List<T>> DeleteListAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => DeleteList(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public async Task<T> GetAsync<KeyType>(IDbConnection connection, KeyType key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(connection, key, transaction, commandTimeout));
		}

		public async Task<T> GetAsync(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(connection, key, transaction, commandTimeout));
		}

		public async Task<T> GetAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(connection, obj, transaction, commandTimeout));
		}

		public async Task<List<KeyType>> GetKeysAsync<KeyType>(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetKeys<KeyType>(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public async Task<List<T>> GetKeysAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetKeys(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public async Task<List<T>> GetListAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetList(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public async Task<T> InsertAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Insert(connection, obj, transaction, commandTimeout));
		}

		public async Task<IEnumerable<T>> BulkInsertAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkInsert(connection, objs, transaction, commandTimeout));
		}

		public async Task<int> RecordCountAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => RecordCount(connection, whereCondition, param, transaction, commandTimeout));
		}

		public async Task<bool> UpdateAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(connection, obj, transaction, commandTimeout));
		}

		public async Task<int> BulkUpdateAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpdate(connection, objs, transaction, commandTimeout));
		}

		public async Task<T> UpsertAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Upsert(connection, obj, transaction, commandTimeout));
		}

		public async Task<IEnumerable<T>> BulkUpsertAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpsert(connection, objs, transaction, commandTimeout));
		}

		public async Task<List<KeyType>> BulkDeleteListAsync<KeyType>(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDeleteList<KeyType>(connection, objs, transaction, buffered, commandTimeout));
		}

		public async Task<List<T>> BulkDeleteListAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDeleteList(connection, objs, transaction, buffered, commandTimeout));
		}

		public async Task<IEnumerable<T>> BulkInsertAsync(SqlBulkCopy bulkCopy, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkInsert(bulkCopy, objs, commandTimeout));
		}
		#endregion // IConnectionQueriesAsync<T>
	}
}
