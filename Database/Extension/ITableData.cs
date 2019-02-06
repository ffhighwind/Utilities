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
		PropertyInfo[] EqualityProperties { get; }

		string[] Columns { get; }
		string[] KeyColumns { get; }

		void SetKey(T source, T dest);
		void SetKey(T obj, object key);
		void SetKey<KeyType>(T obj, KeyType key);
		/// <summary>
		/// Gets an ExpandoObject T with the keys filled in.
		/// </summary>
		/// <param name="obj">The input object to pull keys from.</param>
		/// <returns>An ExpandoObject with keys from the input.</returns>
		object GetKey(T obj);

		/// <summary>
		/// Gets the value of the first key from an object. This assumes that there is only one KeyAttribute.
		/// </summary>
		/// <typeparam name="Tout">The type of the key.</typeparam>
		/// <param name="obj">The input object to pull the key from.</param>
		/// <returns>The value of the key.</returns>
		Tout GetKey<Tout>(T obj);


		object MakeKey<KeyType>(KeyType key);

		/// <summary>
		/// Creates an object from a key where the type has identical KeyProperties (e.g. ExpandoObject or typeof(T)).
		/// </summary>
		/// <param name="key">The key or value</param>
		/// <returns></returns>
		T CreateObject(object key);

		T CreateObject<KeyType>(KeyType key);

		/// <summary>
		/// Returns true if the destination was modified, or false if they were identical.
		/// </summary>
		bool Copy(T source, T dest);
		T Clone(T source);
	}
}
