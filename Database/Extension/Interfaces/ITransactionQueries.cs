using System;
using System.Collections.Generic;
using System.Data;
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
		public abstract int Delete(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		public abstract int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract List<T> DeleteList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract Ret Get(IDbTransaction transaction, object key, int? commandTimeout = null);
		public abstract Ret Get(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract List<T> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract List<Ret> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract Ret Insert(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract IEnumerable<Ret> Insert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		public abstract int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null);
		public abstract bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract int Update(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		public abstract Ret Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract IEnumerable<Ret> Upsert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		public abstract List<T> DeleteList(IDbTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
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

		public async Task<int> DeleteAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, objs, commandTimeout));
		}

		public async Task<int> DeleteAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<List<T>> DeleteListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
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

		public async Task<List<T>> GetKeysAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetKeys(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<List<Ret>> GetListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetList(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<Ret> InsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Insert(transaction, obj, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> InsertAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Insert(transaction, objs, commandTimeout));
		}

		public async Task<int> RecordCountAsync(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return await Task.Run(() => RecordCount(transaction, whereCondition, param, commandTimeout));
		}

		public async Task<bool> UpdateAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(transaction, obj, commandTimeout));
		}

		public async Task<int> UpdateAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(transaction, objs, commandTimeout));
		}

		public async Task<Ret> UpsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Upsert(transaction, obj, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> UpsertAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Upsert(transaction, objs, commandTimeout));
		}

		public async Task<List<T>> DeleteListAsync(IDbTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => DeleteList(transaction, objs, buffered, commandTimeout));
		}
		#endregion // ITransactionQueriesAsync<T>
	}

	public abstract class ITransactionQueries<T, KeyType, Ret> : ITransactionQueriesSync<T, KeyType, Ret>, ITransactionQueriesAsync<T, KeyType, Ret> where T : class
	{
		#region ITransactionQueriesSync<T, KeyType, Ret>
		public abstract List<KeyType> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract bool Delete(IDbTransaction transaction, KeyType key, int? commandTimeout = null);
		public abstract int Delete(IDbTransaction transaction, IEnumerable<KeyType> objs, int? commandTimeout = null);
		public abstract Ret Get(IDbTransaction transaction, KeyType key, int? commandTimeout = null);
		public abstract List<KeyType> DeleteList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract bool Delete(IDbTransaction transaction, object key, int? commandTimeout = null);
		public abstract bool Delete(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract int Delete(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		public abstract int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract Ret Insert(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract IEnumerable<Ret> Insert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		public abstract bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract int Update(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		public abstract Ret Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract IEnumerable<Ret> Upsert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		public abstract Ret Get(IDbTransaction transaction, object key, int? commandTimeout = null);
		public abstract Ret Get(IDbTransaction transaction, T obj, int? commandTimeout = null);
		public abstract List<Ret> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null);
		public abstract List<KeyType> DeleteList(IDbTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
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

		public async Task<int> DeleteAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, objs, commandTimeout));
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

		public async Task<List<Ret>> GetListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetList(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<Ret> InsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Insert(transaction, obj, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> InsertAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Insert(transaction, objs, commandTimeout));
		}

		public async Task<int> RecordCountAsync(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return await Task.Run(() => RecordCount(transaction, whereCondition, param, commandTimeout));
		}

		public async Task<bool> UpdateAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(transaction, obj, commandTimeout));
		}

		public async Task<int> UpdateAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(transaction, objs, commandTimeout));
		}

		public async Task<Ret> UpsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Upsert(transaction, obj, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> UpsertAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Upsert(transaction, objs, commandTimeout));
		}

		public async Task<List<KeyType>> GetKeysAsync(IDbTransaction transaction, string whereCondition, object param, bool buffered, int? commandTimeout)
		{
			return await Task.Run(() => GetKeys(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<bool> DeleteAsync(IDbTransaction transaction, KeyType key, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, key, commandTimeout));
		}

		public async Task<int> DeleteAsync(IDbTransaction transaction, IEnumerable<KeyType> keys, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(transaction, keys, commandTimeout));
		}

		public async Task<Ret> GetAsync(IDbTransaction transaction, KeyType key, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(transaction, key, commandTimeout));
		}

		public async Task<List<KeyType>> DeleteListAsync(IDbTransaction transaction, string whereCondition, object param, bool buffered, int? commandTimeout)
		{
			return await Task.Run(() => DeleteList(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public async Task<List<KeyType>> DeleteListAsync(IDbTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => DeleteList(transaction, objs, buffered, commandTimeout));
		}
		#endregion ITransactionQueriesAsync<T, KeyType Ret>
	}
}
