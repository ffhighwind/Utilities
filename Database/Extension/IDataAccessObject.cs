using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public interface IDataAccessObject<T> where T : class
	{
		List<T> GetKeys(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		bool Delete(object key, int? commandTimeout = null);

		bool Delete(T obj, int? commandTimeout = null);

		int Delete(IEnumerable<T> objs, int? commandTimeout = null);

		int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		List<T> DeleteList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		void Insert(T obj, int? commandTimeout = null);

		void Insert(IEnumerable<T> objs, int? commandTimeout = null);

		bool Update(T obj, int? commandTimeout = null);

		int Update(IEnumerable<T> objs, int? commandTimeout = null);

		void Upsert(T obj, int? commandTimeout = null);

		void Upsert(IEnumerable<T> objs, int? commandTimeout = null);

		T Get(object key, int? commandTimeout = null);

		T Get(T obj, int? commandTimeout = null);

		List<T> GetList(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);

		int RecordCount(string whereCondition = "", object param = null, int? commandTimeout = null);
	}
}