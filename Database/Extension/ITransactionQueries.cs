using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public interface ITransactionQueries<T> where T : class
	{
		bool Delete(IDbTransaction transaction, object key, int? commandTimeout = null);
		bool Delete(IDbTransaction transaction, T obj, int? commandTimeout = null);
		int Delete(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);
		int Delete(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		void Insert(IDbTransaction transaction, T obj, int? commandTimeout = null);
		void Insert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);

		bool Update(IDbTransaction transaction, T obj, int? commandTimeout = null);
		int Update(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);

		void Upsert(IDbTransaction transaction, T obj, int? commandTimeout = null);
		void Upsert(IDbTransaction transaction, IEnumerable<T> objs, int? commandTimeout = null);

		T Get(IDbTransaction transaction, object key, int? commandTimeout = null);
		T Get(IDbTransaction transaction, T obj, int? commandTimeout = null);
		List<T> GetList(IDbTransaction transaction, string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		int RecordCount(IDbTransaction transaction, string whereCondition = "", object param = null, int? commandTimeout = null);
	}
}
