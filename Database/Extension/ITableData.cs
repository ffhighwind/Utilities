using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public interface ITableData<T> : ITableQueries<T>, ITableQueriesAsync<T> where T : class
	{
		TableAttribute TableAttribute { get; }
		string TableName { get; }

		PropertyInfo[] Properties { get; }
		PropertyInfo[] KeyProperties { get; }
		PropertyInfo[] AutoKeyProperties { get; }
		PropertyInfo[] SelectProperties { get; }
		PropertyInfo[] UpdateProperties { get; }
		PropertyInfo[] InsertProperties { get; }
		PropertyInfo[] CompareProperties { get; }

		string[] Columns { get; }
		string[] KeyColumns { get; }

		void CopyKey(object key, T obj);
		T CreateObject(object key);

		/// <summary>
		/// Returns true if the destination was modified, or false if they were identical.
		/// </summary>
		bool Copy(T source, T dest);
	}
}
