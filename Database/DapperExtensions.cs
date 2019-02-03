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
	public static class DapperExtensions
	{
		public static bool Delete<T>(this IDbConnection connection, T obj, SqlTransaction transaction = null, int? commandTimeout = null) 
			where T : class
		{
			return TableData<T>.TableQueries.Delete(connection, obj, transaction, commandTimeout);
		}

		public static async Task<bool> DeleteAsync<T>(IDbConnection connection, T obj, SqlTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => TableData<T>.TableQueries.Delete(connection, obj, transaction, commandTimeout));
		}

		public static int Delete<T>(IDbConnection connection, string whereCondition = "", object param = null, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.TableQueries.Delete(connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public static async Task<int> DeleteAsync<T>(IDbConnection connection, string whereCondition = "", object param = null, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => TableData<T>.TableQueries.Delete(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public static void Insert<T>(IDbConnection connection, T obj, SqlTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			TableData<T>.TableQueries.Insert(connection, obj, transaction, commandTimeout);
		}

		public static async Task InsertAsync<T>(IDbConnection connection, T obj, SqlTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			await Task.Run(() => TableData<T>.TableQueries.Insert(connection, obj, transaction, commandTimeout));
		}

		public static bool Update<T>(IDbConnection connection, T obj, SqlTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.TableQueries.Update(connection, obj, transaction, commandTimeout);
		}

		public static async Task<bool> UpdateAsync<T>(IDbConnection connection, T obj, SqlTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => TableData<T>.TableQueries.Update(connection, obj, transaction, commandTimeout));
		}

		public static int Update<T>(IDbConnection connection, string whereCondition = "", object param = null, SqlTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.TableQueries.Update(connection, whereCondition, param, transaction, commandTimeout);
		}

		public static async Task<int> UpdateAsync<T>(IDbConnection connection, string whereCondition = "", object param = null, SqlTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => TableData<T>.TableQueries.Update(connection, whereCondition, param, transaction, commandTimeout));
		}

		public static T Get<T>(IDbConnection connection, T obj, SqlTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.TableQueries.Get(connection, obj, transaction, commandTimeout);
		}

		public static async Task<T> GetAsync<T>(IDbConnection connection, T obj, SqlTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => TableData<T>.TableQueries.Get(connection, obj, transaction, commandTimeout));
		}

		public static List<T> GetList<T>(IDbConnection connection, string whereCondition = "", object param = null, 
			SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.TableQueries.GetList(connection, whereCondition, param, transaction, buffered, commandTimeout);
		}

		public static async Task<List<T>> GetListAsync<T>(IDbConnection connection, string whereCondition = "", object param = null, 
			SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
			where T : class
		{
			return await Task.Run(() => TableData<T>.TableQueries.GetList(connection, whereCondition, param, transaction, buffered, commandTimeout));
		}

		public static int RecordCount<T>(IDbConnection connection, string whereCondition = "", object param = null, SqlTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return TableData<T>.TableQueries.RecordCount(connection, whereCondition, param, transaction, commandTimeout);
		}

		public static Task<int> RecordCountAsync<T>(IDbConnection connection, string whereCondition = "", object param = null, SqlTransaction transaction = null, int? commandTimeout = null)
			where T : class
		{
			return Task.Run(() => TableData<T>.TableQueries.RecordCount(connection, whereCondition, param, transaction, commandTimeout));
		}
	}
}
