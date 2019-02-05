using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public interface IDataAccessObjectAsync<T> where T : class
	{
		Task<List<T>> GetKeysAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<bool> DeleteAsync(object key, int? commandTimeout = null);

		Task<bool> DeleteAsync(T obj, int? commandTimeout = null);

		Task<int> DeleteAsync(IEnumerable<T> objs, int? commandTimeout = null);

		Task<int> DeleteAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task InsertAsync(T obj, int? commandTimeout = null);

		Task InsertAsync(IEnumerable<T> objs, int? commandTimeout = null);

		Task<bool> UpdateAsync(T obj, int? commandTimeout = null);

		Task<int> UpdateAsync(IEnumerable<T> objs, int? commandTimeout = null);

		Task UpsertAsync(T obj, int? commandTimeout = null);

		Task UpsertAsync(IEnumerable<T> objs, int? commandTimeout = null);

		Task<T> GetAsync(object key, int? commandTimeout = null);

		Task<T> GetAsync(T obj, int? commandTimeout = null);

		Task<List<T>> GetListAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<int> RecordCountAsync(string whereCondition = "", object param = null, int? commandTimeout = null);
	}
}
