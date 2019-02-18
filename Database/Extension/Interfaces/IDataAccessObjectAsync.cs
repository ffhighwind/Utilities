using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Interfaces
{
	public interface IDataAccessObjectAsync<T> : IDataAccessObjectAsync<T, T> where T : class {	}

	public interface IDataAccessObjectAsync<T, Ret> where T : class
	{
		Task<IEnumerable<T>> GetKeysAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<bool> DeleteAsync(object key, int? commandTimeout = null);
		Task<bool> DeleteAsync(T obj, int? commandTimeout = null);
		Task<int> DeleteAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		Task<IEnumerable<T>> DeleteListAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<Ret> InsertAsync(T obj, int? commandTimeout = null);
		Task<bool> UpdateAsync(T obj, int? commandTimeout = null);
		Task<Ret> UpsertAsync(T obj, int? commandTimeout = null);

		Task<Ret> GetAsync(object key, int? commandTimeout = null);
		Task<Ret> GetAsync(T obj, int? commandTimeout = null);
		Task<IEnumerable<Ret>> GetListAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<int> RecordCountAsync(string whereCondition = "", object param = null, int? commandTimeout = null);

		Task<IEnumerable<T>> BulkDeleteListAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		Task<int> BulkDeleteAsync(IEnumerable<T> objs, int? commandTimeout = null);
		Task<IEnumerable<Ret>> BulkInsertAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		Task<int> BulkUpdateAsync(IEnumerable<T> objs, int? commandTimeout = null);
		Task<IEnumerable<Ret>> BulkUpsertAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
	}


	public interface IDataAccessObjectAsync<T, KeyType, Ret> where T : class
	{
		Task<IEnumerable<KeyType>> GetKeysAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<bool> DeleteAsync(KeyType key, int? commandTimeout = null);
		Task<bool> DeleteAsync(object key, int? commandTimeout = null);
		Task<bool> DeleteAsync(T obj, int? commandTimeout = null);
		Task<int> DeleteAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		Task<IEnumerable<KeyType>> DeleteListAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<Ret> InsertAsync(T obj, int? commandTimeout = null);
		Task<bool> UpdateAsync(T obj, int? commandTimeout = null);
		Task<Ret> UpsertAsync(T obj, int? commandTimeout = null);

		Task<Ret> GetAsync(KeyType key, int? commandTimeout = null);
		Task<Ret> GetAsync(object key, int? commandTimeout = null);
		Task<Ret> GetAsync(T obj, int? commandTimeout = null);
		Task<IEnumerable<Ret>> GetListAsync(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Task<int> RecordCountAsync(string whereCondition = "", object param = null, int? commandTimeout = null);

		Task<int> BulkDeleteAsync(IEnumerable<KeyType> keys, int? commandTimeout = null);
		Task<IEnumerable<KeyType>> BulkDeleteListAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		Task<int> BulkDeleteAsync(IEnumerable<T> objs, int? commandTimeout = null);
		Task<IEnumerable<Ret>> BulkInsertAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		Task<int> BulkUpdateAsync(IEnumerable<T> objs, int? commandTimeout = null);
		Task<IEnumerable<Ret>> BulkUpsertAsync(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
	}
}
