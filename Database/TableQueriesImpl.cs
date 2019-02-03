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
	internal class TableQueriesImpl<T> : ITableQueries<T> where T : class
	{
		public bool Delete(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return 0 < connection.Execute(TableData<T>.DeleteSingleQuery, key, transaction, commandTimeout);
		}

		public bool Delete(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return 0 < connection.Execute(TableData<T>.DeleteSingleQuery, obj, transaction, commandTimeout);
		}

		public int Delete(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Execute(TableData<T>.DeleteQuery + whereCondition, param, transaction, commandTimeout);
		}

		public int Delete(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			using (IDbTransaction tempTransaction = transaction != null ? null : connection.BeginTransaction()) {
				transaction = transaction ?? tempTransaction;
				int count = 0;
				foreach (T obj in objs) {
					if (Delete(connection, obj, transaction, commandTimeout)) {
						count++;
					}
				}
				tempTransaction?.Commit();
				return count;
			}
		}

		public T Get(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.Query<T>(TableData<T>.SelectSingleQuery, obj, transaction, true, commandTimeout).SingleOrDefault();
		}

		public T Get(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return connection.Query<T>(TableData<T>.SelectSingleQuery, key, transaction, true, commandTimeout).SingleOrDefault();
		}

		public List<T> GetKeys(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(TableData<T>.SelectListKeysQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public List<T> GetList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
		{
			return connection.Query<T>(TableData<T>.SelectListQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public void Insert(IDbConnection connection, T obj, IDbTransaction transaction, int? commandTimeout)
		{
			connection.Execute(TableData<T>.InsertQuery, obj, transaction, commandTimeout);
		}

		public void Insert(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			using (IDbTransaction tempTransaction = transaction != null ? null : connection.BeginTransaction()) {
				transaction = transaction ?? tempTransaction;
				foreach (T obj in objs) {
					Insert(connection, obj, transaction, commandTimeout);
				}
				tempTransaction?.Commit();
			}
		}

		public int RecordCount(IDbConnection connection, string whereCondition, object param, IDbTransaction transaction, int? commandTimeout)
		{
			return connection.ExecuteScalar<int>(TableData<T>.CountQuery + whereCondition, param, transaction, commandTimeout);
		}

		public bool Update(IDbConnection connection, T obj, IDbTransaction transaction, int? commandTimeout)
		{
			return 0 < connection.ExecuteScalar<int>(TableData<T>.UpdateQuery, obj, transaction, commandTimeout);
		}

		public int Update(IDbConnection connection, string whereCondition, object param, IDbTransaction transaction, int? commandTimeout)
		{
			return connection.ExecuteScalar<int>(TableData<T>.UpdateQuery + whereCondition, param, transaction, commandTimeout);
		}

		public int Update(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			using (IDbTransaction tempTransaction = transaction != null ? null : connection.BeginTransaction()) {
				transaction = transaction ?? tempTransaction;
				int count = 0;
				foreach (T obj in objs) {
					if (Update(connection, obj, transaction, commandTimeout)) {
						count++;
					}
				}
				tempTransaction?.Commit();
				return count;
			}
		}

		public void Upsert(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			using (IDbTransaction tempTransaction = transaction != null ? null : connection.BeginTransaction()) {
				transaction = transaction ?? tempTransaction;
				foreach (T obj in objs) {
					Upsert(connection, obj, transaction, commandTimeout);
				}
				tempTransaction?.Commit();
			}
		}

		public void Upsert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			connection.Execute(TableData<T>.UpsertQuery, obj, transaction, commandTimeout);
		}
	}
}