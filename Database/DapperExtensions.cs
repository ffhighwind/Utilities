using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension;

namespace Dapper
{
	public static class DapperExtensions
	{
		#region ITableQueries<T>
		public static List<T> GetKeys<T>(this IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return connection.Query<T>(TableData<T>.SelectListKeysQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public static bool Delete<T>(this IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return 0 < connection.Execute(TableData<T>.DeleteSingleQuery, key, transaction, commandTimeout);
		}

		public static bool Delete<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return 0 < connection.Execute(TableData<T>.DeleteSingleQuery, obj, transaction, commandTimeout);
		}

		public static int Delete<T>(this IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			int count = 0;
			foreach (T obj in objs) {
				if (Delete(connection, obj, transaction, commandTimeout)) {
					count++;
				}
			}
			return count;
		}

		public static int Delete<T>(this IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return connection.Execute(TableData<T>.DeleteQuery + whereCondition, param, transaction, commandTimeout);
		}

		public static void Insert<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			connection.Execute(TableData<T>.InsertQuery, obj, transaction, commandTimeout);
		}

		public static void Insert<T>(this IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			foreach (T obj in objs) {
				Insert(connection, obj, transaction, commandTimeout);
			}
		}

		public static bool Update<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return 0 < connection.ExecuteScalar<int>(TableData<T>.UpdateQuery, obj, transaction, commandTimeout);
		}

		public static int Update<T>(this IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			int count = 0;
			foreach (T obj in objs) {
				if (Update(connection, obj, transaction, commandTimeout)) {
					count++;
				}
			}
			return count;
		}

		public static void Upsert<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			connection.Execute(TableData<T>.UpsertQuery, obj, transaction, commandTimeout);
		}

		public static void Upsert<T>(this IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			foreach (T obj in objs) {
				Upsert(connection, obj, transaction, commandTimeout);
			}
		}

		public static T Get<T>(this IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return connection.Query<T>(TableData<T>.SelectSingleQuery, key, transaction, true, commandTimeout).SingleOrDefault();
		}

		public static T Get<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return connection.Query<T>(TableData<T>.SelectSingleQuery, obj, transaction, true, commandTimeout).SingleOrDefault();
		}

		public static List<T> GetList<T>(this IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return connection.Query<T>(TableData<T>.SelectListQuery + whereCondition, param, transaction, buffered, commandTimeout).AsList();
		}

		public static int RecordCount<T>(this IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return connection.ExecuteScalar<int>(TableData<T>.CountQuery + whereCondition, param, transaction, commandTimeout);
		}
		#endregion // ITableQueries<T>

		#region ITableQueriesAsync<T>
		public static async Task<List<T>> GetKeysAsync<T>(this IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => GetKeys<T>(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => Delete<T>(connection, key, transaction, commandTimeout));
		}

		public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => Delete<T>(connection, obj, transaction, commandTimeout));
		}

		public static async Task<int> DeleteAsync<T>(this IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => Delete<T>(connection, objs, transaction, commandTimeout));
		}

		public static async Task<int> DeleteAsync<T>(this IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => Delete<T>(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public static async Task InsertAsync<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			await Task.Run(() => Insert(connection, obj, transaction, commandTimeout));
		}

		public static async Task InsertAsync<T>(this IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			await Task.Run(() => Insert(connection, objs, transaction, commandTimeout));
		}

		public static async Task<bool> UpdateAsync<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => Update(connection, obj, transaction, commandTimeout));
		}

		public static async Task<int> UpdateAsync<T>(this IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => Update(connection, objs, transaction, commandTimeout));
		}

		public static async Task UpsertAsync<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			await Task.Run(() => Upsert(connection, obj, transaction, commandTimeout));
		}

		public static async Task UpsertAsync<T>(this IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			await Task.Run(() => Upsert(connection, objs, transaction, commandTimeout));
		}

		public static async Task<T> GetAsync<T>(this IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => Get<T>(connection, key, transaction, commandTimeout));
		}

		public static async Task<T> GetAsync<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => Get(connection, obj, transaction, commandTimeout));
		}

		public static async Task<List<T>> GetListAsync<T>(this IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => GetList<T>(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public static async Task<int> RecordCountAsync<T>(this IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => RecordCount<T>(connection, whereCondition, param, transaction, commandTimeout));
		}
		#endregion // ITableQueriesAsync<T>
	}
}
