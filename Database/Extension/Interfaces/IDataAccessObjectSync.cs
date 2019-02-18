using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Interfaces
{
	public interface IDataAccessObjectSync<T> : IDataAccessObjectSync<T, T> where T : class { }

	public interface IDataAccessObjectSync<T, Ret> where T : class
	{
		IEnumerable<T> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		bool Delete(object key, int? commandTimeout = null);
		bool Delete(T obj, int? commandTimeout = null);
		int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		IEnumerable<T> DeleteList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Ret Insert(T obj, int? commandTimeout = null);
		bool Update(T obj, int? commandTimeout = null);
		Ret Upsert(T obj, int? commandTimeout = null);

		Ret Get(object key, int? commandTimeout = null);
		Ret Get(T obj, int? commandTimeout = null);
		IEnumerable<Ret> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		int RecordCount(string whereCondition = "", object param = null, int? commandTimeout = null);

		IEnumerable<T> BulkDeleteList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		IEnumerable<Ret> BulkUpsert(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		int BulkUpdate(IEnumerable<T> objs, int? commandTimeout = null);
		int BulkDelete(IEnumerable<T> objs, int? commandTimeout = null);
		IEnumerable<Ret> BulkInsert(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
	}



	public interface IDataAccessObjectSync<T, KeyType, Ret> where T : class
	{
		IEnumerable<KeyType> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		bool Delete(KeyType key, int? commandTimeout = null);
		bool Delete(object key, int? commandTimeout = null);
		bool Delete(T obj, int? commandTimeout = null);
		int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
		IEnumerable<KeyType> DeleteList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		Ret Insert(T obj, int? commandTimeout = null);
		bool Update(T obj, int? commandTimeout = null);
		Ret Upsert(T obj, int? commandTimeout = null);

		Ret Get(KeyType key, int? commandTimeout = null);
		Ret Get(object key, int? commandTimeout = null);
		Ret Get(T obj, int? commandTimeout = null);
		IEnumerable<Ret> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		int RecordCount(string whereCondition = "", object param = null, int? commandTimeout = null);

		int BulkDelete(IEnumerable<KeyType> keys, int? commandTimeout = null);
		IEnumerable<KeyType> BulkDeleteList(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		int BulkDelete(IEnumerable<T> objs, int? commandTimeout = null);
		int BulkUpdate(IEnumerable<T> objs, int? commandTimeout = null);
		IEnumerable<Ret> BulkInsert(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
		IEnumerable<Ret> BulkUpsert(IEnumerable<T> objs, bool buffered = true, int? commandTimeout = null);
	}
}