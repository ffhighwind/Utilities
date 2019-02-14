using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Interfaces
{
	public interface ITransactionQueriesSync<T> : ITransactionQueriesSync<T, T> where T : class { }


	public interface ITransactionQueriesSync<T, Ret> where T : class
	{
		bool Delete(IDbTransaction transaction, object key, int? commandTimeout = null);
		bool Delete(IDbTransaction transaction, T obj, int? commandTimeout = null);
		int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		List<T> DeleteList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Ret Insert(IDbTransaction transaction, T obj, int? commandTimeout = null);
		bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Ret Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null);

		List<T> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		Ret Get(IDbTransaction transaction, object key, int? commandTimeout = null);
		Ret Get(IDbTransaction transaction, T obj, int? commandTimeout = null);
		List<Ret> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null);

		List<T> BulkDeleteList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		int BulkDelete(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		IEnumerable<Ret> BulkInsert(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		IEnumerable<Ret> BulkUpsert(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		int BulkUpdate(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
	}


	public interface ITransactionQueriesSync<T, KeyType, Ret> where T : class
	{
		bool Delete(IDbTransaction transaction, KeyType key, int? commandTimeout = null);
		bool Delete(IDbTransaction transaction, object key, int? commandTimeout = null);
		bool Delete(IDbTransaction transaction, T obj, int? commandTimeout = null);
		int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		List<KeyType> DeleteList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Ret Insert(IDbTransaction transaction, T obj, int? commandTimeout = null);
		bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null);
		Ret Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null);

		List<KeyType> GetKeys(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		Ret Get(IDbTransaction transaction, KeyType key, int? commandTimeout = null);
		Ret Get(IDbTransaction transaction, object key, int? commandTimeout = null);
		Ret Get(IDbTransaction transaction, T obj, int? commandTimeout = null);
		List<Ret> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null);

		int BulkUpdate(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		int BulkDelete(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		int BulkDelete(SqlTransaction transaction, IEnumerable<KeyType> keys, int? commandTimeout = null);
		List<KeyType> BulkDeleteList(SqlTransaction transaction, IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		IEnumerable<Ret> BulkInsert(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		IEnumerable<Ret> BulkUpsert(SqlTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
	}
}
