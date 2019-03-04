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
	public class TableQueriesData<T, KeyType> where T : class
	{
		public TableDelegates<T, KeyType>.SqlKeysKeys BulkDeleteListFunc { get; set; }
		public TableDelegates<T, KeyType>.SqlKeysInt BulkDeleteFunc { get; set; }
		public TableDelegates<T, KeyType>.SqlKeyBool DeleteKeyFunc { get; set; }
		public TableDelegates<T, KeyType>.DbWhereKeys DeleteKeysWhereFunc { get; set; }
		public TableDelegates<T, KeyType>.SqlKeyObj GetKeyFunc { get; set; }
		public TableDelegates<T, KeyType>.DbWhereKeys GetKeysWhereFunc { get; set; }
	}

	public class TableQueriesData<T> where T : class
	{
		public PropertyInfo[] Properties { get; set; }
		public PropertyInfo[] KeyProperties { get; set; }
		public PropertyInfo[] AutoKeyProperties { get; set; }
		public PropertyInfo[] EqualityProperties { get; set; }

		public string[] Columns { get; set; }
		public string[] KeyColumns { get; set; }

		public TableDelegates<T>.SqlListInt BulkDeleteFunc { get; set; }
		public TableDelegates<T>.SqlListList BulkDeleteListFunc { get; set; }
		public TableDelegates<T>.SqlListList BulkInsertFunc { get; set; }
		public TableDelegates<T>.SqlListInt BulkUpdateFunc { get; set; }
		public TableDelegates<T>.SqlListList BulkUpdateListFunc { get; set; }
		public TableDelegates<T>.SqlListInt BulkUpsertFunc { get; set; }
		public TableDelegates<T>.SqlListList BulkUpsertListFunc { get; set; }
		public TableDelegates<T>.DbDictBool DeleteDictFunc { get; set; }
		public TableDelegates<T>.DbObjBool DeleteFunc { get; set; }
		public TableDelegates<T>.DbWhereInt DeleteWhereFunc { get; set; }
		public TableDelegates<T>.DbWhereList DeleteListFunc { get; set; }
		public TableDelegates<T>.DbDictObj GetDictFunc { get; set; }
		public TableDelegates<T>.DbObjObj GetFunc { get; set; }
		public TableDelegates<T>.DbWhereList GetKeysFunc { get; set; }
		public TableDelegates<T>.DbWhereList GetListFunc { get; set; }
		public TableDelegates<T>.DbObjObj InsertFunc { get; set; }
		public TableDelegates<T>.DbWhereInt RecordCountFunc { get; set; }
		//public TableDelegates<T>.RemoveDuplicatesFunc RemoveDuplicatesFunc { get; set; }
		public TableDelegates<T>.DbObjBool UpdateFunc { get; set; }
		public TableDelegates<T>.DbObjObj UpsertFunc { get; set; }
	}
}
