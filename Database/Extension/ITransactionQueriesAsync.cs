using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public interface ITransactionQueriesAsync<T> where T : class
	{
		Task<List<T>> GetKeysAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<bool> DeleteAsync(IDbTransaction transaction, object key, int? commandTimeout = null);
		Task<bool> DeleteAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task<int> DeleteAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		Task<int> DeleteAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task InsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task InsertAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);

		Task<bool> UpdateAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task<int> UpdateAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);

		Task UpsertAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task UpsertAsync(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);

		Task<T> GetAsync(IDbTransaction transaction, object key, int? commandTimeout = null);
		Task<T> GetAsync(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Task<List<T>> GetListAsync(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<int> RecordCountAsync(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null);
	}
}
