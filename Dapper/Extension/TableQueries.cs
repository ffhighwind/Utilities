using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension.Interfaces;

namespace Dapper.Extension
{
	public class TableQueries<T, KeyType> where T : class
	{
		public TableQueries(TableQueriesData<T, KeyType> impl)
		{
			BulkDeleteFunc = impl.BulkDeleteFunc;
			DeleteFunc = impl.DeleteKeyFunc;
			DeleteListFunc = impl.DeleteKeysWhereFunc;
			GetFunc = impl.GetKeyFunc;
			GetKeysFunc = impl.GetKeysWhereFunc;
			BulkDeleteListFunc = impl.BulkDeleteListFunc;
		}

		public TableDelegates<T, KeyType>.SqlKeysKeys BulkDeleteListFunc { get; private set; }
		public TableDelegates<T, KeyType>.SqlKeysInt BulkDeleteFunc { get; private set; }
		public TableDelegates<T, KeyType>.SqlKeyBool DeleteFunc { get; private set; }
		public TableDelegates<T, KeyType>.DbWhereKeys DeleteListFunc { get; private set; }
		public TableDelegates<T, KeyType>.SqlKeyObj GetFunc { get; private set; }
		public TableDelegates<T, KeyType>.DbWhereKeys GetKeysFunc { get; private set; }
	}


	public class TableQueries<T> where T : class
	{
		public TableQueries(TableQueriesData<T> impl)
		{
			Properties = impl.Properties;
			KeyProperties = impl.KeyProperties;
			AutoKeyProperties = impl.AutoKeyProperties;
			EqualityProperties = impl.EqualityProperties;
			Columns = impl.Columns;
			KeyColumns = impl.KeyColumns;

			BulkDeleteFunc = impl.BulkDeleteFunc;
			BulkDeleteListFunc = impl.BulkDeleteListFunc;
			BulkInsertFunc = impl.BulkInsertFunc;
			BulkUpdateFunc = impl.BulkUpdateFunc;
			BulkUpdateListFunc = impl.BulkUpdateListFunc;
			BulkUpsertFunc = impl.BulkUpsertFunc;
			BulkUpsertListFunc = impl.BulkUpsertListFunc;
			DeleteDictFunc = impl.DeleteDictFunc;
			DeleteFunc = impl.DeleteFunc;
			DeleteWhereFunc = impl.DeleteWhereFunc;
			DeleteListFunc = impl.DeleteListFunc;
			GetDictFunc = impl.GetDictFunc;
			GetFunc = impl.GetFunc;
			GetKeysFunc = impl.GetKeysFunc;
			GetListFunc = impl.GetListFunc;
			InsertFunc = impl.InsertFunc;
			RecordCountFunc = impl.RecordCountFunc;
			UpdateFunc = impl.UpdateFunc;
			UpsertFunc = impl.UpsertFunc;
		}

		public PropertyInfo[] Properties { get; private set; }
		public PropertyInfo[] KeyProperties { get; private set; }
		public PropertyInfo[] AutoKeyProperties { get; private set; }
		public PropertyInfo[] EqualityProperties { get; private set; }

		public string[] Columns { get; private set; }
		public string[] KeyColumns { get; private set; }

		public TableDelegates<T>.SqlListInt BulkDeleteFunc { get; private set; }
		public TableDelegates<T>.SqlListList BulkDeleteListFunc { get; private set; }
		public TableDelegates<T>.SqlListList BulkInsertFunc { get; private set; }
		public TableDelegates<T>.SqlListInt BulkUpdateFunc { get; private set; }
		public TableDelegates<T>.SqlListList BulkUpdateListFunc { get; private set; }
		public TableDelegates<T>.SqlListInt BulkUpsertFunc { get; private set; }
		public TableDelegates<T>.SqlListList BulkUpsertListFunc { get; private set; }
		public TableDelegates<T>.DbDictBool DeleteDictFunc { get; private set; }
		public TableDelegates<T>.DbObjBool DeleteFunc { get; private set; }
		public TableDelegates<T>.DbWhereInt DeleteWhereFunc { get; private set; }
		public TableDelegates<T>.DbWhereList DeleteListFunc { get; private set; }
		public TableDelegates<T>.DbDictObj GetDictFunc { get; private set; }
		public TableDelegates<T>.DbObjObj GetFunc { get; private set; }
		public TableDelegates<T>.DbWhereList GetKeysFunc { get; private set; }
		public TableDelegates<T>.DbWhereList GetListFunc { get; private set; }
		public TableDelegates<T>.DbObjObj InsertFunc { get; private set; }
		public TableDelegates<T>.DbWhereInt RecordCountFunc { get; private set; }
		public TableDelegates<T>.DbObjBool UpdateFunc { get; private set; }
		public TableDelegates<T>.DbObjObj UpsertFunc { get; private set; }
	}
}
