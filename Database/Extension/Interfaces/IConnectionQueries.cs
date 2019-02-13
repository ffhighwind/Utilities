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
		public abstract int Delete<KeyType>(IDbConnection connection, IEnumerable<KeyType> keys, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract bool Delete(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract bool Delete(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract int Delete(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null);
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
		public abstract IEnumerable<T> Insert(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction, int? commandTimeout);
		public abstract IEnumerable<T> Insert(SqlBulkCopy bulkCopy, IEnumerable<T> objs, int? commandTimeout);
		public abstract IEnumerable<T> Insert(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract int RecordCount(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract bool Update(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract int Update(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract T Upsert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract IEnumerable<T> Upsert(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null);
		public abstract List<KeyType> DeleteList<KeyType>(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		public abstract List<T> DeleteList(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		#endregion // IConnectionQueriesSync<T>


		#region IConnectionQueriesAsync<T>
		public async Task<bool> DeleteAsync<KeyType>(IDbConnection connection, KeyType key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, key, transaction, commandTimeout));
		}

		public async Task<int> DeleteAsync<KeyType>(IDbConnection connection, IEnumerable<KeyType> keys, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(connection, keys, transaction, commandTimeout));
		}

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

		public async Task<IEnumerable<T>> InsertAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Insert(connection, objs, transaction, commandTimeout));
		}

		public async Task<int> RecordCountAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => RecordCount(connection, whereCondition, param, transaction, commandTimeout));
		}

		public async Task<bool> UpdateAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(connection, obj, transaction, commandTimeout));
		}

		public async Task<int> UpdateAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(connection, objs, transaction, commandTimeout));
		}

		public async Task<T> UpsertAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Upsert(connection, obj, transaction, commandTimeout));
		}

		public async Task<IEnumerable<T>> UpsertAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Upsert(connection, objs, transaction, commandTimeout));
		}

		public async Task<List<KeyType>> DeleteListAsync<KeyType>(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => DeleteList<KeyType>(connection, objs, transaction, buffered, commandTimeout));
		}

		public async Task<List<T>> DeleteListAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => DeleteList(connection, objs, transaction, buffered, commandTimeout));
		}

		public async Task<IEnumerable<T>> InsertAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null)
		{
			return await Task.Run(() => Insert(connection, objs, transaction, commandTimeout));
		}

		public async Task<IEnumerable<T>> InsertAsync(SqlBulkCopy bulkCopy, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Insert(bulkCopy, objs, commandTimeout));
		}
		#endregion // IConnectionQueriesAsync<T>
	}
}
