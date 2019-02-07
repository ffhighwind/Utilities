using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public interface ITableQueriesAsync<T> where T : class
	{
		Task<List<T>> GetKeysAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);

		Task<bool> DeleteAsync(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<bool> DeleteAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<int> DeleteAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<int> DeleteAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		Task<List<T>> DeleteListAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);

		Task<T> InsertAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<IEnumerable<T>> InsertAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<bool> UpdateAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<int> UpdateAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<T> UpsertAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<IEnumerable<T>> UpsertAsync(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<T> GetAsync(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<T> GetAsync(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);

		Task<List<T>> GetListAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);

		Task<int> RecordCountAsync(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null);
	}
}
