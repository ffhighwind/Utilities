using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Interfaces
{
	public interface ITransactionQueriesAsync<T> : ITransactionQueriesAsync<T, T> where T : class {	}

	public interface ITransactionQueriesAsync<T, Ret> where T : class
	{
		Task<bool> DeleteAsync(IDbTransaction transaction, object key, int? commandTimeout = null);
		Task<bool> DeleteAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task<int> DeleteAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		Task<List<T>> DeleteListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<Ret> InsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task<bool> UpdateAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task<Ret> UpsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);

		Task<List<T>> GetKeysAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		Task<Ret> GetAsync(IDbTransaction transaction, object key, int? commandTimeout = null);
		Task<Ret> GetAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task<List<Ret>> GetListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<int> RecordCountAsync(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null);

		Task<int> BulkUpdateAsync(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		Task<List<T>> BulkDeleteListAsync(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		Task<int> BulkDeleteAsync(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		Task<IEnumerable<Ret>> BulkInsertAsync(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		Task<IEnumerable<Ret>> BulkUpsertAsync(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
	}


	public interface ITransactionQueriesAsync<T, KeyType, Ret> where T : class
	{
		Task<bool> DeleteAsync(IDbTransaction transaction, KeyType key, int? commandTimeout = null);
		Task<bool> DeleteAsync(IDbTransaction transaction, object key, int? commandTimeout = null);
		Task<bool> DeleteAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task<int> DeleteAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		Task<List<KeyType>> DeleteListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<Ret> InsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task<bool> UpdateAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task<Ret> UpsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);

		Task<List<KeyType>> GetKeysAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		Task<Ret> GetAsync(IDbTransaction transaction, KeyType key, int? commandTimeout = null);
		Task<Ret> GetAsync(IDbTransaction transaction, object key, int? commandTimeout = null);
		Task<Ret> GetAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task<List<Ret>> GetListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<int> RecordCountAsync(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null);

		Task<IEnumerable<Ret>> BulkInsertAsync(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		Task<int> BulkDeleteAsync(SqlTransaction transaction, IEnumerable<KeyType> keys, int? commandTimeout = null);
		Task<List<KeyType>> BulkDeleteListAsync(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		Task<int> BulkDeleteAsync(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		Task<int> BulkUpdateAsync(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		Task<IEnumerable<Ret>> BulkUpsertAsync(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
	}
}
