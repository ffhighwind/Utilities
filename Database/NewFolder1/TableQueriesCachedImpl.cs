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
	internal class TableQueriesCachedImpl<T> : ITableQueries<T> where T : class
	{
		internal TableQueriesImpl<T> impl = new TableQueriesImpl<T>();

		internal TableQueriesCachedImpl()
		{
			if (TableData<T>.KeyProperties.Length == 0) {
				throw new InvalidOperationException("Cannot cache objects without a KeyAttribute.");
			}
		}

		public bool Delete(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			T objKey = (T)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
			TableData<T>.CopyKeyToObject(key, objKey);
			return Delete(connection, objKey, transaction, commandTimeout);
		}

		public bool Delete(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			have a cache factory instead, when getting stuff
			check if stale then pull everything
			store the ConnString and Query
			should include all of the queries but as virtual
			should have a force update of an item

			TableData<T>.Cache.Remove(obj);
			return impl.Delete(connection, obj, transaction, commandTimeout);
		}

		public int Delete(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			TableData<T>.Cache.Remove(objs);
			return impl.Delete(connection, objs, transaction, commandTimeout);
		}

		public int Delete(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			if (string.IsNullOrWhiteSpace(whereCondition)) {
				TableData<T>.Cache.Clear();
			}
			else if (TableData<T>.Cache.Cache.Count != 0) {
				List<T> objs = GetKeys(connection, whereCondition, param, transaction, buffered, commandTimeout);
				TableData<T>.Cache.Remove(objs);
			}
			return impl.Delete(connection, whereCondition, param);
		}

		public T Get(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			T keyObj = (T)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
			TableData<T>.CopyKeyToObject(key, keyObj);
			return Get(connection, keyObj, transaction, commandTimeout);
		}

		public T Get(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			DateTime now = DateTime.Now;
			if (!TableData<T>.Cache.Cache.TryGetValue(obj, out var val) || val.StaleTime > now) {
				T result = impl.Get(connection, obj, transaction, commandTimeout);
				return TableData<T>.Cache.Upsert(result, now);
			}
			return val.Value;
		}

		internal List<T> GetKeys(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return impl.GetKeys(connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public List<T> GetList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			if (string.IsNullOrWhiteSpace(whereCondition)) {
				whereCondition = "";
			}
			if()
			List<T> list = impl.GetList(connection, whereCondition, param, transaction, buffered, commandTimeout);
			DateTime now = DateTime.Now;
			TableData<T>.Cache.GetListQueries[whereCondition] = now;
			return TableData<T>.Cache.Upsert(list, now);
		}

		public void Insert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			impl.Insert(connection, obj, transaction, commandTimeout);
			TableData<T>.Cache.Upsert(obj, DateTime.Now);
		}

		public void Insert(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			impl.Insert(connection, objs, transaction, commandTimeout);
			TableData<T>.Cache.Upsert(objs, DateTime.Now);
		}

		public int RecordCount(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return impl.RecordCount(connection, whereCondition, param, transaction, commandTimeout);
		}

		public bool Update(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return impl.Update(connection, obj, transaction, commandTimeout);
		}

		public int Update(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return impl.Update(connection, objs, transaction, commandTimeout);
		}

		public void Upsert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			throw new NotImplementedException();
		}

		public void Upsert(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			throw new NotImplementedException();
		}
	}
}
