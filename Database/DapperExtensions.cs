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

		public static bool Delete<T>(this IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.DAO.Delete(connection, key, transaction, commandTimeout);
		}

		public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await TableData<T>.DAO.DeleteAsync(connection, key, transaction, commandTimeout);
		}

		public static bool Delete<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.DAO.Delete(connection, obj, transaction, commandTimeout);
		}

		public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await TableData<T>.DAO.DeleteAsync(connection, obj, transaction, commandTimeout);
		}

		public static int Delete<T>(this IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.DAO.Delete(connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public static async Task<int> DeleteAsync<T>(this IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return await TableData<T>.DAO.DeleteAsync(connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public static void Insert<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			TableData<T>.DAO.Insert(connection, obj, transaction, commandTimeout);
		}

		public static async Task InsertAsync<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			await TableData<T>.DAO.InsertAsync(connection, obj, transaction, commandTimeout);
		}

		public static bool Update<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.DAO.Update(connection, obj, transaction, commandTimeout);
		}

		public static async Task<bool> UpdateAsync<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await TableData<T>.DAO.UpdateAsync(connection, obj, transaction, commandTimeout);
		}

		public static int Update<T>(this IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.DAO.Update(connection, objs, transaction, commandTimeout);
		}

		public static async Task<int> UpdateAsync<T>(this IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await TableData<T>.DAO.UpdateAsync(connection, objs, transaction, commandTimeout);
		}

		public static T Get<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.DAO.Get(connection, obj, transaction, commandTimeout);
		}

		public static async Task<T> GetAsync<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await TableData<T>.DAO.GetAsync(connection, obj, transaction, commandTimeout);
		}

		public static List<T> GetList<T>(this IDbConnection connection, string whereCondition = "", object param = null,
			SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.DAO.GetList(connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public static async Task<List<T>> GetListAsync<T>(this IDbConnection connection, string whereCondition = "", object param = null,
			IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return await TableData<T>.DAO.GetListAsync(connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public static int RecordCount<T>(this IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.DAO.RecordCount(connection, whereCondition, param, transaction, commandTimeout);
		}

		public static async Task<int> RecordCountAsync<T>(this IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await TableData<T>.DAO.RecordCountAsync(connection, whereCondition, param, transaction, commandTimeout);
		}
	}
}
