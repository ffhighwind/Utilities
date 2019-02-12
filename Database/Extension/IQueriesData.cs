using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public interface IQueriesData<T> where T : class
	{
		PropertyInfo[] Properties { get; }
		PropertyInfo[] KeyProperties { get; }
		PropertyInfo[] AutoKeyProperties { get; }
		PropertyInfo[] SelectProperties { get; }
		PropertyInfo[] UpdateProperties { get; }
		PropertyInfo[] InsertProperties { get; }
		PropertyInfo[] EqualityProperties { get; }
		BindingFlags PropertyFlags { get; }

		string[] Columns { get; }
		string[] KeyColumns { get; }
	}
}
