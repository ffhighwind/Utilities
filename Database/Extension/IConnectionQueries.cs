using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public interface IConnectionQueries<T> where T : class
	{
		List<T> GetKeys(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		bool Delete(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null);
		bool Delete(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		int Delete(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null);
		int Delete(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);
		List<T> DeleteList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);

		T Insert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		IEnumerable<T> Insert(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null);

		bool Update(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		int Update(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null);

		T Upsert(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		IEnumerable<T> Upsert(IDbConnection connection, IEnumerable<T> objs, IDbTransaction transaction = null, int? commandTimeout = null);

		T Get(IDbConnection connection, object key, IDbTransaction transaction = null, int? commandTimeout = null);
		T Get(IDbConnection connection, T obj, IDbTransaction transaction = null, int? commandTimeout = null);
		List<T> GetList(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null);

		int RecordCount(IDbConnection connection, string whereCondition = "", object param = null, IDbTransaction transaction = null, int? commandTimeout = null);
	}
}
