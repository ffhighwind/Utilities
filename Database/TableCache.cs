using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Database
{
	public class TableCache<T, CacheBaseT>
		where T : class
		where CacheBaseT : CacheBase<T>, new()
	{
		protected Dictionary<T, CacheBaseT> Map = new Dictionary<T, CacheBaseT>();

		public IReadOnlyDictionary<T, CacheBaseT> Items;

		public TableCache(DataAccessObject<T> dao, string whereConditions = "", object param = null)
		{
			if (TableData<T>.KeyProperties.Length == 0) {
				throw new InvalidOperationException("Cannot cache objects without a KeyAttribute.");
			}
			DAO = dao;
			Items = Map;
			WhereConditions = string.IsNullOrWhiteSpace(whereConditions) ? "" : whereConditions;
			Param = param;
		}

		public string WhereConditions { get; private set; }
		public object Param { get; private set; }
		public DataAccessObject<T> DAO { get; private set; }
		private DateTime PreviousPull { get; set; } = DateTime.MinValue;
		private TimeSpan _StaleTimeSpan { get; set; } = new TimeSpan(1, 0, 0);
		public DateTime StaleDateTime { get; private set; } = DateTime.MinValue;
		public TimeSpan StaleTimeSpan {
			get => _StaleTimeSpan;
			set {
				if (value <= TimeSpan.Zero) {
					throw new InvalidOperationException("StaleTimeSpan must be positive.");
				}
				_StaleTimeSpan = value;
				StaleDateTime = PreviousPull.Add(value);
			}
		}

		protected void UpsertItem(T obj)
		{
			if (Map.TryGetValue(obj, out CacheBaseT cache))
				cache.Value = obj;
			else
				Map[obj] = new CacheBaseT() { Value = obj };
		}

		public Action OnCacheUpdate { get; set; } = () => { };

		public bool IsStale => DateTime.Now > StaleDateTime;

		public void DoCacheUpdate()
		{
			if (IsStale) {
				foreach (T obj in DAO.GetList(WhereConditions, Param, true, null)) {
					UpsertItem(obj);
				}
				OnCacheUpdate();
			}
		}

		public bool Delete(object key, int? commandTimeout = null)
		{
			return Delete(TableData<T>.CreateObjectFromKey(key), commandTimeout);
		}

		public bool Delete(T obj, int? commandTimeout = null)
		{
			DoCacheUpdate();
			if (!Map.Remove(obj))
				return false;
			return DAO.Delete(obj, commandTimeout);
		}

		public int Delete(IEnumerable<T> objs, int? commandTimeout = null)
		{
			DoCacheUpdate();
			List<T> toRemove = new List<T>();
			foreach (T obj in objs) {
				if (Map.Remove(obj)) {
					toRemove.Add(obj);
				}
			}
			if (toRemove.Count == 0)
				return 0;
			return DAO.Delete(toRemove, commandTimeout);
		}

		public void Insert(T obj, int? commandTimeout = null)
		{
			DAO.Insert(obj, commandTimeout);
			Map[obj] = new CacheBaseT() { Value = obj };
		}

		public void Insert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			DAO.Insert(objs, commandTimeout);
			foreach (T obj in objs) {
				Map[obj] = new CacheBaseT() { Value = obj };
			}
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
			foreach (T obj in objs) {
				UpsertItem(obj);
			}
		}

		public CacheBaseT Get(object key, int? commandTimeout = null)
		{
			return Get(TableData<T>.CreateObjectFromKey(key), commandTimeout);
		}

		public CacheBaseT Get(T obj, int? commandTimeout = null)
		{
			DoCacheUpdate();
			return Map.TryGetValue(obj, out CacheBaseT cache) ? cache : null;
		}

		public ICollection<CacheBaseT> GetList()
		{
			DoCacheUpdate();
			return Map.Values;
		}

		public int RecordCount()
		{
			DoCacheUpdate();
			return Map.Count;
		}
	}
}
