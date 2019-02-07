using Dapper;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public class TableCache<T, KeyType, CacheBaseT> : TableCache<T, CacheBaseT>
		where T : class
		where KeyType : struct
		where CacheBaseT : class, ICacheBase<T>, new()
	{
		public TableCache(DataAccessObject<T> dao) : base(dao) { }

		public bool Delete(KeyType key, int? commandTimeout = null)
		{
			return Delete(TableData<T>.CreateObject<KeyType>(key), commandTimeout);
		}

		public Task<bool> DeleteAsync(KeyType key, int? commandTimeout = null)
		{
			return DeleteAsync(TableData<T>.CreateObject<KeyType>(key), commandTimeout);
		}

		public CacheBaseT Get(KeyType key, int? commandTimeout = null)
		{
			return Get(TableData<T>.CreateObject<KeyType>(key), commandTimeout);
		}

		public Task<CacheBaseT> GetAsync(KeyType key, int? commandTimeout = null)
		{
			return GetAsync(TableData<T>.CreateObject<KeyType>(key), commandTimeout);
		}

		public CacheBaseT Find(KeyType key, int? commandTimeout = null)
		{
			return Find(TableData<T>.CreateObject<KeyType>(key), commandTimeout);
		}

		public Task<CacheBaseT> FindAsync(KeyType key, int? commandTimeout = null)
		{
			return FindAsync(TableData<T>.CreateObject<KeyType>(key), commandTimeout);
		}
	}

	public class TableCache<T, CacheBaseT> : IDataAccessObject<T, CacheBaseT>, IDataAccessObjectAsync<T, CacheBaseT>, IEnumerable<CacheBaseT>
		where T : class
		where CacheBaseT : class, ICacheBase<T>, new()
	{
		protected IDictionary<T, CacheBaseT> Map = null;

		public IReadOnlyDictionary<T, CacheBaseT> Items;

		public IEnumerable<CacheBaseT> Values => Items.Values;

		public IEnumerator<CacheBaseT> GetEnumerator()
		{
			return Items.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Values.GetEnumerator();
		}

		public TableCache(DataAccessObject<T> dao)
		{
			if (TableData<T>.KeyProperties.Length != 1) {
				throw new InvalidOperationException("Cannot cache objects without a KeyAttribute.");
			}
			Dictionary<T, CacheBaseT> map = new Dictionary<T, CacheBaseT>(TableEqualityComparer<T>.Default);
			Map = map;
			DAO = dao;
			Items = map;
		}

		public DataAccessObject<T> DAO { get; private set; }

		protected CacheBaseT UpsertItem(T obj)
		{
			if (Map.TryGetValue(obj, out CacheBaseT cache))
				cache.Value = obj;
			else {
				cache = new CacheBaseT() { Value = obj };
				Map[obj] = cache;
			}
			return cache;
		}

		protected List<CacheBaseT> UpsertItems(IEnumerable<T> objs)
		{
			List<CacheBaseT> list = new List<CacheBaseT>();
			foreach (T obj in objs) {
				list.Add(UpsertItem(obj));
			}
			return list;
		}

		public bool Delete(object key, int? commandTimeout = null)
		{
			bool deleted = DAO.Delete(key, commandTimeout);
			T obj = TableData<T>.CreateObject(key);
			Map.Remove(obj);
			return deleted;
		}

		public bool Delete(T obj, int? commandTimeout = null)
		{
			bool deleted = DAO.Delete(obj, commandTimeout);
			Map.Remove(obj);
			return deleted;
		}

		public int Delete(IEnumerable<T> objs, int? commandTimeout = null)
		{
			int count = DAO.Delete(objs, commandTimeout);
			foreach (T obj in objs) {
				Map.Remove(obj);
			}
			return count;
		}

		public int Delete(IEnumerable<CacheBaseT> objs, int? commandTimeout = null)
		{
			return Delete(objs.Select(o => o.Value), commandTimeout);
		}

		public int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			List<T> list = DAO.DeleteList(whereCondition, param, buffered, commandTimeout);
			foreach (T obj in list) {
				Map.Remove(obj);
			}
			return list.Count;
		}

		public CacheBaseT Insert(T obj, int? commandTimeout = null)
		{
			DAO.Insert(obj, commandTimeout);
			return UpsertItem(obj);
		}

		public IEnumerable<CacheBaseT> Insert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			DAO.Insert(objs, commandTimeout);
			return UpsertItems(objs);
		}

		public bool Update(T obj, int? commandTimeout = null)
		{
			return DAO.Update(obj, commandTimeout);
		}

		public int Update(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return DAO.Update(objs, commandTimeout);
		}

		public int Update(IEnumerable<CacheBaseT> objs, int? commandTimeout = null)
		{
			return Update(objs.Select(o => o.Value), commandTimeout);
		}

		public CacheBaseT Upsert(T obj, int? commandTimeout = null)
		{
			DAO.Upsert(obj, commandTimeout);
			return UpsertItem(obj);
		}

		public IEnumerable<CacheBaseT> Upsert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			DAO.Upsert(objs, commandTimeout);
			return UpsertItems(objs);
		}

		public CacheBaseT Get(object key, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Get(key, commandTimeout));
		}

		public CacheBaseT Get(T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Get(obj, commandTimeout));
		}

		public List<CacheBaseT> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return UpsertItems(DAO.GetList(whereCondition, param, buffered, commandTimeout));
		}

		public int RecordCount(string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return DAO.RecordCount(whereCondition, param, commandTimeout);
		}

		public CacheBaseT Find(T obj, int? commandTimeout = null)
		{
			return Map.TryGetValue(obj, out CacheBaseT value)
				? value : Get(obj, commandTimeout);
		}

		public List<T> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return DAO.GetKeys(whereCondition, param, buffered, commandTimeout);
		}

		public List<T> DeleteList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			List<T> objs = DAO.DeleteList(whereCondition, param, buffered, commandTimeout);
			foreach(T obj in objs) {
				Map.Remove(obj);
			}
			return objs;
		}

		public Task<List<T>> GetKeysAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return DAO.GetKeysAsync(whereCondition, param, buffered, commandTimeout);
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
			return await Task.Run(() => { return Delete(whereCondition, param, buffered, commandTimeout); });
		}

		public async Task<CacheBaseT> InsertAsync(T obj, int? commandTimeout = null)
		{
			await DAO.InsertAsync(obj, commandTimeout);
			return UpsertItem(obj);
		}

		public async Task<IEnumerable<CacheBaseT>> InsertAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			await DAO.InsertAsync(objs, commandTimeout);
			return UpsertItems(objs);
		}

		public async Task<bool> UpdateAsync(T obj, int? commandTimeout = null)
		{
			return await DAO.UpdateAsync(obj, commandTimeout);
		}

		public async Task<int> UpdateAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await DAO.UpdateAsync(objs, commandTimeout);
		}

		public async Task<CacheBaseT> UpsertAsync(T obj, int? commandTimeout = null)
		{
			await DAO.UpsertAsync(obj, commandTimeout);
			return UpsertItem(obj);
		}

		public async Task<IEnumerable<CacheBaseT>> UpsertAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			await DAO.UpsertAsync(objs, commandTimeout);
			return UpsertItems(objs);
		}

		public async Task<CacheBaseT> GetAsync(object key, int? commandTimeout = null)
		{
			T obj = await DAO.GetAsync(key, commandTimeout);
			return obj == null ? null : UpsertItem(obj);
		}

		public async Task<CacheBaseT> GetAsync(T obj, int? commandTimeout = null)
		{
			obj = await DAO.GetAsync(obj, commandTimeout);
			return obj == null ? null : UpsertItem(obj);
		}

		public async Task<List<CacheBaseT>> GetListAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			List<T> list = await DAO.GetListAsync(whereCondition, param, buffered, commandTimeout);
			return UpsertItems(list);
		}

		public async Task<int> RecordCountAsync(string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return await DAO.RecordCountAsync(whereCondition, param, commandTimeout);
		}

		public async Task<CacheBaseT> FindAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => Find(obj, commandTimeout));
		}

		public async Task<List<T>> DeleteListAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => DeleteList(whereCondition, param, buffered, commandTimeout));
		}
	}
}
