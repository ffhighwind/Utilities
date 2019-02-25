using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Interfaces
{
	public interface IConnectionQueriesAsync<T> where T : class
	{
		Task<IEnumerable<KeyType>> GetKeysAsync<KeyType>(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		Task<IEnumerable<T>> GetKeysAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);

		Task<bool> DeleteAsync<KeyType>(IDbConnection connection, KeyType key, IDbTransaction transaction = null, int? commandTimeout = null);
		Task<bool> DeleteAsync(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null);
		Task<bool> DeleteAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		Task<int> DeleteAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		Task<IEnumerable<T>> DeleteListAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		Task<IEnumerable<KeyType>> DeleteListAsync<KeyType>(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);

		Task<T> InsertAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		Task<bool> UpdateAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		Task<T> UpsertAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<T> GetAsync<KeyType>(IDbConnection connection, KeyType key, IDbTransaction transaction = null, int? commandTimeout = null);
		Task<T> GetAsync(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null);
		Task<T> GetAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		Task<IEnumerable<T>> GetListAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);

		Task<int> RecordCountAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<IEnumerable<T>> BulkInsertAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		Task<int> BulkUpdateAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null);
		Task<int> BulkDeleteAsync<KeyType>(SqlConnection connection, IEnumerable<KeyType> keys, SqlTransaction transaction = null, int? commandTimeout = null);
		Task<int> BulkDeleteAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null);
		Task<int> BulkUpsertAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null);

		Task<IEnumerable<T>> BulkUpdateListAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		Task<IEnumerable<KeyType>> BulkDeleteListAsync<KeyType>(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		Task<IEnumerable<T>> BulkDeleteListAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		Task<IEnumerable<T>> BulkUpsertListAsync(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
	}
}
