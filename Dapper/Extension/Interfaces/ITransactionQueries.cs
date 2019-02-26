using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Interfaces
{
	public abstract class ITransactionQueries<T> : ITransactionQueries<T, T> where T : class { }

	public abstract class ITransactionQueries<T, Ret> : ITransactionQueriesSync<T, Ret>, ITransactionQueriesAsync<T, Ret> where T : class
	{
		#region ITransactionQueriesSync<T>
		public abstract bool Delete(IDbTransaction transaction, object key, int? commandTimeout = null);
		public abstract bool Delete(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<T> DeleteList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract Ret Get(IDbTransaction transaction, object key, int? commandTimeout = null);
		public abstract Ret Get(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract IEnumerable<T> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<Ret> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract Ret Insert(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null);
		//public abstract int Update(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null);
		public abstract Ret Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null);

		public abstract IEnumerable<Ret> BulkInsert(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		public abstract int BulkUpdate(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		public abstract int BulkDelete(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		public abstract int BulkUpsert(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);

		public abstract IEnumerable<Ret> BulkUpdateList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<T> BulkDeleteList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<Ret> BulkUpsertList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		#endregion // ITransactionQueriesSync<T>


		#region ITransactionQueriesAsync<T>
		public async Task<bool> DeleteAsync(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, key, commandTimeout));
		}

		public async Task<bool> DeleteAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, obj, commandTimeout));
		}

		public async Task<int> DeleteAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<IEnumerable<T>> DeleteListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => DeleteList(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<Ret> GetAsync(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(transaction, key, commandTimeout));
		}

		public async Task<Ret> GetAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(transaction, obj, commandTimeout));
		}

		public async Task<IEnumerable<T>> GetKeysAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetKeys(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> GetListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetList(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<Ret> InsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Insert(transaction, obj, commandTimeout));
		}

		public async Task<int> RecordCountAsync(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return await Task.Run(() => RecordCount(transaction, whereCondition, param, commandTimeout));
		}

		public async Task<bool> UpdateAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(transaction, obj, commandTimeout));
		}

		//public async Task<int> UpdateAsync(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		//{
		//	return await Task.Run(() => Update(transaction, whereCondition, param, commandTimeout));
		//}

		public async Task<Ret> UpsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Upsert(transaction, obj, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> BulkInsertAsync(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkInsert(transaction, objs, buffered, commandTimeout));
		}

		public async Task<int> BulkUpdateAsync(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpdate(transaction, objs, commandTimeout));
		}

		public async Task<int> BulkDeleteAsync(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDelete(transaction, objs, commandTimeout));
		}

		public async Task<int> BulkUpsertAsync(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpsert(transaction, objs, buffered, commandTimeout));
		}

		public async Task<IEnumerable<T>> BulkDeleteListAsync(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDeleteList(transaction, objs, buffered, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> BulkUpdateListAsync(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpdateList(transaction, objs, buffered, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> BulkUpsertListAsync(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpsertList(transaction, objs, buffered, commandTimeout));
		}

		#endregion // ITransactionQueriesAsync<T>
	}

	public abstract class ITransactionQueries<T, KeyType, Ret> : ITransactionQueriesSync<T, KeyType, Ret>, ITransactionQueriesAsync<T, KeyType, Ret> where T : class
	{
		#region ITransactionQueriesSync<T, KeyType, Ret>
		public abstract bool Delete(IDbTransaction transaction, KeyType key, int? commandTimeout = null);
		public abstract bool Delete(IDbTransaction transaction, object key, int? commandTimeout = null);
		public abstract bool Delete(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<KeyType> DeleteList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract Ret Insert(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null);
		//public abstract int Update(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null);
		public abstract Ret Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract IEnumerable<KeyType> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract Ret Get(IDbTransaction transaction, object key, int? commandTimeout = null);
		public abstract Ret Get(IDbTransaction transaction, KeyType key, int? commandTimeout = null);
		public abstract Ret Get(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract IEnumerable<Ret> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null);

		public abstract IEnumerable<Ret> BulkInsert(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		public abstract int BulkUpdate(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		public abstract int BulkDelete(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		public abstract int BulkDelete(SqlTransaction transaction, IEnumerable<KeyType> objs, int? commandTimeout = null);
		public abstract int BulkUpsert(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);

		public abstract IEnumerable<KeyType> BulkDeleteList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<Ret> BulkUpdateList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<Ret> BulkUpsertList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		#endregion // ITransactionQueriesSync<T, KeyType, Ret>


		#region ITransactionQueriesAsync<T, KeyType, Ret>
		public async Task<bool> DeleteAsync(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, key, commandTimeout));
		}

		public async Task<bool> DeleteAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, obj, commandTimeout));
		}

		public async Task<int> DeleteAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<Ret> GetAsync(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(transaction, key, commandTimeout));
		}

		public async Task<Ret> GetAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(transaction, obj, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> GetListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetList(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<Ret> InsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Insert(transaction, obj, commandTimeout));
		}

		public async Task<int> RecordCountAsync(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return await Task.Run(() => RecordCount(transaction, whereCondition, param, commandTimeout));
		}

		public async Task<bool> UpdateAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(transaction, obj, commandTimeout));
		}

		//public async Task<int> UpdateAsync(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		//{
		//	return await Task.Run(() => Update(transaction, whereCondition, param, commandTimeout));
		//}

		public async Task<Ret> UpsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Upsert(transaction, obj, commandTimeout));
		}

		public async Task<IEnumerable<KeyType>> GetKeysAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetKeys(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<bool> DeleteAsync(IDbTransaction transaction, KeyType key, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, key, commandTimeout));
		}

		public async Task<Ret> GetAsync(IDbTransaction transaction, KeyType key, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(transaction, key, commandTimeout));
		}

		public async Task<IEnumerable<KeyType>> DeleteListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => DeleteList(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<int> BulkDeleteAsync(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDelete(transaction, objs, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> BulkInsertAsync(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkInsert(transaction, objs, buffered, commandTimeout));
		}

		public async Task<int> BulkUpdateAsync(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpdate(transaction, objs, commandTimeout));
		}

		public async Task<int> BulkUpsertAsync(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpsert(transaction, objs, buffered, commandTimeout));
		}

		public async Task<int> BulkDeleteAsync(SqlTransaction transaction, IEnumerable<KeyType> keys, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDelete(transaction, keys, commandTimeout));
		}

		public async Task<IEnumerable<KeyType>> BulkDeleteListAsync(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDeleteList(transaction, objs, buffered, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> BulkUpdateListAsync(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpdateList(transaction, objs, buffered, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> BulkUpsertListAsync(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpsertList(transaction, objs, buffered, commandTimeout));
		}
		#endregion ITransactionQueriesAsync<T, KeyType Ret>
	}
}
