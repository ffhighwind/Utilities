using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public class TableCache<T, CacheBaseT>
		where T : class
		where CacheBaseT : CacheBase<T>, new()
	{
		protected ConcurrentDictionary<T, CacheBaseT> Map = new ConcurrentDictionary<T, CacheBaseT>(new TableEqualityComparer<T>());

		public IReadOnlyDictionary<T, CacheBaseT> Items;

		public TableCache(DataAccessObject<T> dao)
		{
			if (TableData<T>.KeyProperties.Length == 0) {
				throw new InvalidOperationException("Cannot cache objects without a KeyAttribute.");
			}
			DAO = dao;
			Items = Map;
		}

		public DataAccessObject<T> DAO { get; private set; }

		protected CacheBaseT UpsertItem(T obj)
		{
			return Map.AddOrUpdate(obj, new CacheBaseT() { Value = obj }, (val, c) => { c.Value = obj; return c; });
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
			return Delete(TableData<T>.CreateObject(key), commandTimeout);
		}

		public bool Delete(T obj, int? commandTimeout = null)
		{
			bool deleted = DAO.Delete(obj, commandTimeout);
			Map.TryRemove(obj, out CacheBaseT value);
			return deleted;
		}

		public int Delete(IEnumerable<T> objs, int? commandTimeout = null)
		{
			int count = DAO.Delete(objs, commandTimeout);
			foreach (T obj in objs) {
				Map.TryRemove(obj, out CacheBaseT value);
			}
			return count;
		}

		public int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			List<T> list = DAO.DeleteList(whereCondition, param, buffered, commandTimeout);
			foreach (T obj in list) {
				Map.TryRemove(obj, out CacheBaseT value);
			}
			return list.Count;
		}

		public void Insert(T obj, int? commandTimeout = null)
		{
			DAO.Insert(obj, commandTimeout);
			UpsertItem(obj);
		}

		public void Insert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			DAO.Insert(objs, commandTimeout);
			UpsertItems(objs);
		}

		public bool Update(T obj, int? commandTimeout = null)
		{
			return DAO.Update(obj, commandTimeout);
		}

		public int Update(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return DAO.Update(objs, commandTimeout);
		}

		public void Upsert(T obj, int? commandTimeout = null)
		{
			DAO.Upsert(obj, commandTimeout);
			UpsertItem(obj);
		}

		public void Upsert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			DAO.Upsert(objs, commandTimeout);
			UpsertItems(objs);
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
			return Map.TryGetValue(obj, out CacheBaseT value) ? value : Get(obj, commandTimeout);
		}

		public async Task<List<T>> GetKeysAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await DAO.GetKeysAsync(whereCondition, param, buffered, commandTimeout);
		}

		public async Task<bool> DeleteAsync(object key, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(key, commandTimeout));
		}

		public async Task<bool> DeleteAsync(T obj, int? commandTimeout = null)
		{
			return await Task.Run(() => { return Delete(obj, commandTimeout); });
		}

		public async Task<int> DeleteAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return await Task.Run(() => Delete(objs, commandTimeout));
		}

		public async Task<int> DeleteAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return await Task.Run(() => { return Delete(whereCondition, param, buffered, commandTimeout); });
		}

		public async Task InsertAsync(T obj, int? commandTimeout = null)
		{
			await DAO.InsertAsync(obj, commandTimeout);
			UpsertItem(obj);
		}

		public async Task InsertAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			await DAO.InsertAsync(objs, commandTimeout);
			UpsertItems(objs);
		}

		public async Task<bool> UpdateAsync(T obj, int? commandTimeout = null)
		{
			return await DAO.UpdateAsync(obj, commandTimeout);
		}

		public async Task<int> UpdateAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			int count = await DAO.UpdateAsync(objs, commandTimeout);
			return count;
		}

		public async Task UpsertAsync(T obj, int? commandTimeout = null)
		{
			await DAO.UpsertAsync(obj, commandTimeout);
			UpsertItem(obj);
		}

		public async Task UpsertAsync(IEnumerable<T> objs, int? commandTimeout = null)
		{
			await DAO.UpsertAsync(objs, commandTimeout);
			UpsertItems(objs);
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
	}
}
