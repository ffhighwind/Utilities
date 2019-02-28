﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Interfaces
{
	public abstract class IDataAccessObject<T> : IDataAccessObject<T, T> where T : class { }

	public abstract class IDataAccessObject<T, Ret> : ITransactionQueries<T, Ret>, IDataAccessObjectSync<T, Ret>, IDataAccessObjectAsync<T, Ret> where T : class
	{
		public IDataAccessObject() { }
		public string TableName => TableData<T>.TableName;
		public abstract SqlConnection Connection();

		#region IDataAccessObjectSync<T>
		public abstract bool Delete(IDictionary<string, object> key, int? commandTimeout = null);
		public abstract bool Delete(T obj, int? commandTimeout = null);
		public abstract int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<T> DeleteList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract Ret Get(IDictionary<string, object> key, int? commandTimeout = null);
		public abstract Ret Get(T obj, int? commandTimeout = null);
		public abstract IEnumerable<T> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<Ret> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract Ret Insert(T obj, int? commandTimeout = null);
		public abstract bool Update(T obj, int? commandTimeout = null);
		public abstract Ret Upsert(T obj, int? commandTimeout = null);
		public abstract int RecordCount(string whereCondition = "", object param = null, int? commandTimeout = null);

		public abstract IEnumerable<Ret> BulkInsert(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		public abstract int BulkUpdate(IEnumerable<T> objs, int? commandTimeout = null);
		public abstract int BulkDelete(IEnumerable<T> objs, int? commandTimeout = null);
		public abstract int BulkUpsert(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);

		public abstract IEnumerable<Ret> BulkUpdateList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<T> BulkDeleteList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<Ret> BulkUpsertList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		#endregion IDataAccessObjectSync<T>


		#region IDataAccessObjectAsync<T>
		public async Task<bool> DeleteAsync(IDictionary<string, object> key, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(key, commandTimeout));
		}

		public async Task<bool> DeleteAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(obj, commandTimeout));
		}

		public async Task<int> BulkDeleteAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDelete(objs, commandTimeout));
		}

		public async Task<int> DeleteAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(whereCondition, param, buffered, commandTimeout));
		}

		public async Task<IEnumerable<T>> DeleteListAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => DeleteList(whereCondition, param, buffered, commandTimeout));
		}

		public async Task<Ret> GetAsync(IDictionary<string, object> key, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(key, commandTimeout));
		}

		public async Task<Ret> GetAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(obj, commandTimeout));
		}

		public async Task<IEnumerable<T>> GetKeysAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetKeys(whereCondition, param, buffered, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> GetListAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetList(whereCondition, param, buffered, commandTimeout));
		}

		public async Task<Ret> InsertAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Insert(obj, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> BulkInsertAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkInsert(objs, buffered, commandTimeout));
		}

		public async Task<int> RecordCountAsync(string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return await Task.Run(() => RecordCount(whereCondition, param, commandTimeout));
		}

		public async Task<bool> UpdateAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(obj, commandTimeout));
		}

		public async Task<int> BulkUpdateAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpdate(objs, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> BulkUpdateListAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpdateList(objs, buffered, commandTimeout));
		}


		public async Task<Ret> UpsertAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Upsert(obj, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> BulkUpsertListAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpsertList(objs, buffered, commandTimeout));
		}

		public async Task<int> BulkUpsertAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpsert(objs, buffered, commandTimeout));
		}

		public async Task<IEnumerable<T>> BulkDeleteListAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDeleteList(objs, buffered, commandTimeout));
		}
		#endregion // IDataAccessObjectAsync<T>
	}


	public abstract class IDataAccessObject<T, KeyType, Ret> : ITransactionQueries<T, KeyType, Ret>, IDataAccessObjectSync<T, KeyType, Ret>, IDataAccessObjectAsync<T, KeyType, Ret> where T : class
	{
		public IDataAccessObject()
		{
			if (TableData<T>.KeyProperties.Length != 1) {
				throw new InvalidOperationException(typeof(T).Name + " must have one KeyAttribute.");
			}
		}

		public string TableName => TableData<T>.TableName;
		public abstract SqlConnection Connection();

		#region IDataAccessObjectSync<T, KeyType, Ret>
		public abstract bool Delete(KeyType key, int? commandTimeout = null);
		public abstract bool Delete(IDictionary<string, object> key, int? commandTimeout = null);
		public abstract bool Delete(T obj, int? commandTimeout = null);
		public abstract int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<KeyType> DeleteList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract Ret Get(KeyType key, int? commandTimeout = null);
		public abstract Ret Get(IDictionary<string, object> key, int? commandTimeout = null);
		public abstract Ret Get(T obj, int? commandTimeout = null);
		public abstract IEnumerable<KeyType> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<Ret> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		public abstract Ret Insert(T obj, int? commandTimeout = null);
		public abstract bool Update(T obj, int? commandTimeout = null);
		public abstract Ret Upsert(T obj, int? commandTimeout = null);
		public abstract int RecordCount(string whereCondition = "", object param = null, int? commandTimeout = null);

		public abstract IEnumerable<Ret> BulkInsert(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		public abstract int BulkUpdate(IEnumerable<T> objs, int? commandTimeout = null);
		public abstract int BulkDelete(IEnumerable<KeyType> keys, int? commandTimeout = null);
		public abstract int BulkDelete(IEnumerable<T> objs, int? commandTimeout = null);
		public abstract int BulkUpsert(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);

		public abstract IEnumerable<Ret> BulkUpdateList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<KeyType> BulkDeleteList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		public abstract IEnumerable<Ret> BulkUpsertList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		#endregion // IDataAccessObjectSync<T, KeyType, Ret>


		#region IDataAccessObjectAsync<T, KeyType, Ret>
		public async Task<bool> DeleteAsync(KeyType key, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(key, commandTimeout));
		}

		public async Task<int> BulkDeleteAsync(IEnumerable<KeyType> keys, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDelete(keys, commandTimeout));
		}

		public async Task<bool> DeleteAsync(IDictionary<string, object> key, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(key, commandTimeout));
		}

		public async Task<bool> DeleteAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(obj, commandTimeout));
		}

		public async Task<int> BulkDeleteAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDelete(objs, commandTimeout));
		}

		public async Task<int> DeleteAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(whereCondition, param, buffered, commandTimeout));
		}

		public async Task<IEnumerable<KeyType>> DeleteListAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => DeleteList(whereCondition, param, buffered, commandTimeout));
		}

		public async Task<Ret> GetAsync(KeyType key, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(key, commandTimeout));
		}

		public async Task<Ret> GetAsync(IDictionary<string, object> key, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(key, commandTimeout));
		}

		public async Task<Ret> GetAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Get(obj, commandTimeout));
		}

		public async Task<IEnumerable<KeyType>> GetKeysAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetKeys(whereCondition, param, buffered, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> GetListAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => GetList(whereCondition, param, buffered, commandTimeout));
		}

		public async Task<Ret> InsertAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Insert(obj, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> BulkInsertAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkInsert(objs, buffered, commandTimeout));
		}

		public async Task<int> RecordCountAsync(string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return await Task.Run(() => RecordCount(whereCondition, param, commandTimeout));
		}

		public async Task<bool> UpdateAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Update(obj, commandTimeout));
		}

		public async Task<int> BulkUpdateAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpdate(objs, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> BulkUpdateListAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpdateList(objs, buffered, commandTimeout));
		}

		public async Task<Ret> UpsertAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Upsert(obj, commandTimeout));
		}

		public async Task<IEnumerable<Ret>> BulkUpsertListAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpsertList(objs, buffered, commandTimeout));
		}

		public async Task<int> BulkUpsertAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkUpsert(objs, buffered, commandTimeout));
		}

		public async Task<IEnumerable<KeyType>> BulkDeleteListAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => BulkDeleteList(objs, buffered, commandTimeout));
		}
		#endregion // IDataAccessObjectAsync<T, KeyType, Ret>
	}
}