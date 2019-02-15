using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Interfaces
{
	public abstract class ITableData<T> : IConnectionQueries<T> where T : class
	{
		public PropertyInfo[] Properties { get; protected set; }
		public PropertyInfo[] KeyProperties { get; protected set; }
		public PropertyInfo[] AutoKeyProperties { get; protected set; }
		public PropertyInfo[] SelectProperties { get; protected set; }
		public PropertyInfo[] UpdateProperties { get; protected set; }
		public PropertyInfo[] InsertProperties { get; protected set; }
		public PropertyInfo[] EqualityProperties { get; protected set; }
		public PropertyInfo[] UpsertProperties { get; protected set; }

		public string[] Columns { get; protected set; }
		public string[] KeyColumns { get; protected set; }
	}
}
