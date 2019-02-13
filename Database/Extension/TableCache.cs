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
using Dapper;
using Dapper.Extension.Interfaces;

namespace Dapper.Extension
{
	public class TableCache<T, KeyType, Ret> : IDataAccessObject<T, KeyType, Ret>, IEnumerable<Ret>
		where T : class
	{
		public TableCache(DataAccessObject<T, KeyType> dao, Func<T, Ret> constructor, Func<T, Ret, Ret> update)
		{
			if (TableData<T>.KeyProperties.Length != 1) {
				throw new InvalidOperationException("Cache can only be used on objects with exactly one KeyAttribute.");
			}
			Dictionary<KeyType, Ret> map = new Dictionary<KeyType, Ret>();
			Map = map;
			DAO = dao;
			Items = map;
			UpdateRet = update;
			Constructor = constructor;
		}

		protected IDictionary<KeyType, Ret> Map = null;
		public IReadOnlyDictionary<KeyType, Ret> Items;

		public DataAccessObject<T, KeyType> DAO { get; private set; }

		private Func<T, Ret, Ret> UpdateRet { get; set; }
		private Func<T, Ret> Constructor { get; set; }

		public IEnumerator<Ret> GetEnumerator()
		{
			return Items.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Items.Values.GetEnumerator();
		}

		public override IDbConnection Connection()
		{
			return DAO.Connection();
		}

		protected Ret UpsertItem(T obj)
		{
			KeyType key = TableData<T>.GetKey<KeyType>(obj);
			Ret output = Map.TryGetValue(key, out Ret value) ? UpdateRet(obj, value) : Constructor(obj);
			Map[key] = output;
			return output;
		}

		protected List<Ret> UpsertItems(IEnumerable<T> objs)
		{
			List<Ret> list = new List<Ret>();
			foreach (T obj in objs) {
				list.Add(UpsertItem(obj));
			}
			return list;
		}


		#region IDataAccessObjectSync<T, KeyType, Ret>
		public override List<KeyType> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return DAO.GetKeys(whereCondition, param, buffered, commandTimeout);
		}

		public override bool Delete(KeyType key, int? commandTimeout = null)
		{
			bool deleted = DAO.Delete(key, commandTimeout);
			Map.Remove(key);
			return deleted;
		}

		public override int Delete(IEnumerable<KeyType> keys, int? commandTimeout = null)
		{
			int deleted = DAO.Delete(keys, commandTimeout);
			foreach (KeyType key in keys) {
				Map.Remove(key);
			}
			return deleted;
		}

		public override List<KeyType> DeleteList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			List<KeyType> keys = DAO.DeleteList(whereCondition, param, buffered, commandTimeout);
			foreach (KeyType key in keys) {
				Map.Remove(key);
			}
			return keys;
		}

		public override Ret Get(KeyType key, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Get(key, commandTimeout));
		}

		public override bool Delete(object key, int? commandTimeout = null)
		{
			return Delete(TableData<T>.GetKey<KeyType>(key), commandTimeout);
		}

		public override bool Delete(T obj, int? commandTimeout = null)
		{
			return Delete(TableData<T>.GetKey<KeyType>(obj), commandTimeout);
		}

		public override int Delete(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Delete(objs.Select(obj => TableData<T>.GetKey<KeyType>(obj)), commandTimeout);
		}

		public override int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return DeleteList(whereCondition, param, buffered, commandTimeout).Count;
		}

		public override Ret Insert(T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Insert(obj, commandTimeout));
		}

		public override IEnumerable<Ret> Insert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return UpsertItems(DAO.Insert(objs, commandTimeout));
		}

		public override bool Update(T obj, int? commandTimeout = null)
		{
			return DAO.Update(obj, commandTimeout);
		}

		public override int Update(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return DAO.Update(objs, commandTimeout);
		}

		public override Ret Upsert(T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Upsert(obj, commandTimeout));
		}

		public override IEnumerable<Ret> Upsert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return UpsertItems(DAO.Upsert(objs, commandTimeout));
		}

		public override Ret Get(object key, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Get(key, commandTimeout));
		}

		public override Ret Get(T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Get(obj, commandTimeout));
		}

		public override List<Ret> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return UpsertItems(DAO.GetList(whereCondition, param, buffered, commandTimeout));
		}

		public override int RecordCount(string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return DAO.RecordCount(whereCondition, param, commandTimeout);
		}
		#endregion // IDataAccessObjectSync<T, KeyType, Ret>


		#region ITransactionQueriesSync<T, KeyType, Ret>
		public override List<KeyType> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return DAO.GetKeys(transaction, whereCondition, param, buffered, commandTimeout);
		}

		public override bool Delete(IDbTransaction transaction, KeyType key, int? commandTimeout = null)
		{
			bool deleted = DAO.Delete(transaction, key, commandTimeout);
			Map.Remove(key);
			return deleted;
		}

		public override int Delete(IDbTransaction transaction, IEnumerable<KeyType> keys, int? commandTimeout = null)
		{
			int deleted = DAO.Delete(transaction, keys, commandTimeout);
			foreach (KeyType key in keys) {
				Map.Remove(key);
			}
			return deleted;
		}

		public override Ret Get(IDbTransaction transaction, KeyType key, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Get(transaction, key, commandTimeout));
		}

		public override List<KeyType> DeleteList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			List<KeyType> keys = DAO.DeleteList(transaction, whereCondition, param, buffered, commandTimeout);
			foreach (KeyType key in keys) {
				Map.Remove(key);
			}
			return keys;
		}

		public override bool Delete(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return Delete(transaction, TableData<T>.GetKey<KeyType>(key), commandTimeout);
		}

		public override bool Delete(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return Delete(transaction, TableData<T>.GetKey<KeyType>(obj), commandTimeout);
		}

		public override int Delete(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return Delete(transaction, objs.Select(obj => TableData<T>.GetKey<KeyType>(obj)), commandTimeout);
		}

		public override int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return DeleteList(transaction, whereCondition, param, buffered, commandTimeout).Count;
		}

		public override Ret Insert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Insert(transaction, obj, commandTimeout));
		}

		public override IEnumerable<Ret> Insert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return UpsertItems(DAO.Insert(transaction, objs, commandTimeout));
		}

		public override bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return DAO.Update(transaction, obj, commandTimeout);
		}

		public override int Update(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return DAO.Update(transaction, objs, commandTimeout);
		}

		public override Ret Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Upsert(transaction, obj, commandTimeout));
		}

		public override IEnumerable<Ret> Upsert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return UpsertItems(DAO.Upsert(transaction, objs, commandTimeout));
		}

		public override Ret Get(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Get(transaction, key, commandTimeout));
		}

		public override Ret Get(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Get(transaction, obj, commandTimeout));
		}

		public override List<Ret> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return UpsertItems(DAO.GetList(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public override int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return DAO.RecordCount(transaction, whereCondition, param, commandTimeout);
		}

		public override List<KeyType> DeleteList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			List<KeyType> keys = DAO.DeleteList(objs, buffered, commandTimeout);
			foreach (KeyType key in keys) {
				Map.Remove(key);
			}
			return keys;
		}

		public override List<KeyType> DeleteList(IDbTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			List<KeyType> keys = DAO.DeleteList(transaction, objs, buffered, commandTimeout);
			foreach (KeyType key in keys) {
				Map.Remove(key);
			}
			return keys;
		}
		#endregion // ITransactionQueriesSync<T, KeyType, Ret>
	}



	public class TableCache<T, Ret> : IDataAccessObject<T, Ret>, IEnumerable<Ret>
		where T : class
	{
		protected IDictionary<T, Ret> Map = null;

		public IReadOnlyDictionary<T, Ret> Items;

		private Func<T, Ret, Ret> UpdateRet { get; set; }
		private Func<T, Ret> Constructor { get; set; }

		public IEnumerator<Ret> GetEnumerator()
		{
			return Items.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Items.Values.GetEnumerator();
		}

		public TableCache(DataAccessObject<T> dao, Func<T, Ret> constructor, Func<T, Ret, Ret> update)
		{
			if (TableData<T>.KeyProperties.Length == 0) {
				throw new InvalidOperationException("Cannot cache objects without a KeyAttribute.");
			}
			Dictionary<T, Ret> map = new Dictionary<T, Ret>(TableEqualityComparer<T>.Default);
			Map = map;
			DAO = dao;
			Items = map;
			UpdateRet = update;
			Constructor = constructor;
		}

		public DataAccessObject<T> DAO { get; private set; }

		public override IDbConnection Connection()
		{
			return DAO.Connection();
		}

		protected Ret UpsertItem(T obj)
		{
			Ret output = Map.TryGetValue(obj, out Ret value) ? UpdateRet(obj, value) : Constructor(obj);
			Map[obj] = output;
			return output;
		}

		protected List<Ret> UpsertItems(IEnumerable<T> objs)
		{
			List<Ret> list = new List<Ret>();
			foreach (T obj in objs) {
				list.Add(UpsertItem(obj));
			}
			return list;
		}


		#region IDataAccessObjectSync<T, T, Ret>
		public override List<T> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return DAO.GetKeys(whereCondition, param, buffered, commandTimeout);
		}

		public override List<T> DeleteList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			List<T> objs = DAO.DeleteList(whereCondition, param, buffered, commandTimeout);
			foreach (T obj in objs) {
				Map.Remove(obj);
			}
			return objs;
		}

		public override bool Delete(object key, int? commandTimeout = null)
		{
			return Delete(TableData<T>.CreateObject(key), commandTimeout);
		}

		public override bool Delete(T obj, int? commandTimeout = null)
		{
			bool deleted = DAO.Delete(obj, commandTimeout);
			Map.Remove(obj);
			return deleted;
		}

		public override int Delete(IEnumerable<T> objs, int? commandTimeout = null)
		{
			int deleted = DAO.Delete(objs, commandTimeout);
			foreach (T obj in objs) {
				Map.Remove(obj);
			}
			return deleted;
		}

		public override int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return DeleteList(whereCondition, param, buffered, commandTimeout).Count;
		}

		public override Ret Insert(T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Insert(obj, commandTimeout));
		}

		public override IEnumerable<Ret> Insert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return UpsertItems(DAO.Insert(objs, commandTimeout));
		}

		public override bool Update(T obj, int? commandTimeout = null)
		{
			return DAO.Update(obj, commandTimeout);
		}

		public override int Update(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return DAO.Update(objs, commandTimeout);
		}

		public override Ret Upsert(T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Upsert(obj, commandTimeout));
		}

		public override IEnumerable<Ret> Upsert(IEnumerable<T> objs, int? commandTimeout = null)
		{
			return UpsertItems(DAO.Upsert(objs, commandTimeout));
		}

		public override Ret Get(object key, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Get(key, commandTimeout));
		}

		public override Ret Get(T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Get(obj, commandTimeout));
		}

		public override List<Ret> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return UpsertItems(DAO.GetList(whereCondition, param, buffered, commandTimeout));
		}

		public override int RecordCount(string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return DAO.RecordCount(whereCondition, param, commandTimeout);
		}
		#endregion // IDataAccessObjectSync<T, T, Ret>


		#region ITransactionQueriesSync<T, T, Ret>
		public override List<T> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return DAO.GetKeys(transaction, whereCondition, param, buffered, commandTimeout);
		}

		public override List<T> DeleteList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			List<T> objs = DAO.DeleteList(transaction, whereCondition, param, buffered, commandTimeout);
			foreach (T obj in objs) {
				Map.Remove(obj);
			}
			return objs;
		}

		public override bool Delete(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return Delete(transaction, TableData<T>.CreateObject(key), commandTimeout);
		}

		public override bool Delete(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			bool deleted = DAO.Delete(transaction, obj, commandTimeout);
			Map.Remove(obj);
			return deleted;
		}

		public override int Delete(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			int deleted = DAO.Delete(transaction, objs, commandTimeout);
			foreach (T obj in objs) {
				Map.Remove(obj);
			}
			return deleted;
		}

		public override int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return DeleteList(transaction, whereCondition, param, buffered, commandTimeout).Count;
		}

		public override Ret Insert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Insert(transaction, obj, commandTimeout));
		}

		public override IEnumerable<Ret> Insert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return UpsertItems(DAO.Insert(transaction, objs, commandTimeout));
		}

		public override bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return DAO.Update(transaction, obj, commandTimeout);
		}

		public override int Update(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return DAO.Update(transaction, objs, commandTimeout);
		}

		public override Ret Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Upsert(transaction, obj, commandTimeout));
		}

		public override IEnumerable<Ret> Upsert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null)
		{
			return UpsertItems(DAO.Upsert(transaction, objs, commandTimeout));
		}

		public override Ret Get(IDbTransaction transaction, object key, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Get(transaction, key, commandTimeout));
		}

		public override Ret Get(IDbTransaction transaction, T obj, int? commandTimeout = null)
		{
			return UpsertItem(DAO.Get(transaction, obj, commandTimeout));
		}

		public override List<Ret> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null)
		{
			return UpsertItems(DAO.GetList(transaction, whereCondition, param, buffered, commandTimeout));
		}

		public override int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null)
		{
			return DAO.RecordCount(transaction, whereCondition, param, commandTimeout);
		}

		public override List<T> DeleteList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			List<T> keys = DAO.DeleteList(objs, buffered, commandTimeout);
			foreach (T key in keys) {
				Map.Remove(key);
			}
			return keys;
		}

		public override List<T> DeleteList(IDbTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null)
		{
			List<T> keys = DAO.DeleteList(transaction, objs, buffered, commandTimeout);
			foreach (T key in keys) {
				Map.Remove(key);
			}
			return keys;
		}
		#endregion // ITransactionQueriesSync<T, T, Ret>
	}
}