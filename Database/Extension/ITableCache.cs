using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public interface ITableCache<T, KeyType, BaseCacheT>
		where T : class
		where BaseCacheT : ICacheBase<T>
	{
		bool Delete(object key, int? commandTimeout = null);
		bool Delete(KeyType key, int? commandTimeout = null);
		bool Delete(T obj, int? commandTimeout = null);
		int Delete(IEnumerable<T> objs, int? commandTimeout = null);
		int Delete(string whereCondition = "", object param = null, bool buffered = true, int? commandTimeout = null);
	}
}
