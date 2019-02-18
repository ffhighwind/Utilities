using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Dapper.Extension.Interfaces
{
	public interface IConnectionQueriesSync<T> where T : class
	{
		IEnumerable<KeyType> GetKeys<KeyType>(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		IEnumerable<T> GetKeys(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);

		bool Delete<KeyType>(IDbConnection connection, KeyType key, IDbTransaction transaction = null, int? commandTimeout = null);
		bool Delete(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null);
		bool Delete(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		int Delete(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		IEnumerable<KeyType> DeleteList<KeyType>(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		IEnumerable<T> DeleteList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);

		T Insert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		bool Update(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		T Upsert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);

		T Get<KeyType>(IDbConnection connection, KeyType key, IDbTransaction transaction = null, int? commandTimeout = null);
		T Get(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null);
		T Get(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		IEnumerable<T> GetList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);

		int RecordCount(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null);

		int BulkDelete<KeyType>(SqlConnection connection, IEnumerable<KeyType> keys, SqlTransaction transaction = null, int? commandTimeout = null);
		IEnumerable<KeyType> BulkDeleteList<KeyType>(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		IEnumerable<T> BulkDeleteList(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		int BulkDelete(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null);
		int BulkUpdate(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, int? commandTimeout = null);
		IEnumerable<T> BulkInsert(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		IEnumerable<T> BulkUpsert(SqlConnection connection, IEnumerable<T> objs, SqlTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
	}
}
