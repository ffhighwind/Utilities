using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension.Interfaces;

namespace Dapper.Extension
{
	public static class TableData<T> where T : class
	{
		public static TableAttribute TableAttribute { get; private set; } = typeof(T).GetCustomAttribute<TableAttribute>(false);
		public static ITableData<T> Queries { get; private set; } = TableDataImpl<T>.Default;
		public static string TableName { get; private set; } = TableAttribute?.Name.Replace("'", "") ?? typeof(T).Name;

		public static PropertyInfo[] Properties => Queries.Properties;
		public static PropertyInfo[] KeyProperties => Queries.KeyProperties;
		public static PropertyInfo[] AutoKeyProperties => Queries.AutoKeyProperties;
		public static PropertyInfo[] SelectProperties => Queries.SelectProperties;
		public static PropertyInfo[] UpdateProperties => Queries.UpdateProperties;
		public static PropertyInfo[] InsertProperties => Queries.InsertProperties;
		public static PropertyInfo[] EqualityProperties => Queries.EqualityProperties;

		public static string[] Columns => Queries.Columns;
		public static string[] KeyColumns => Queries.KeyColumns;

		/// <summary>
		/// Creates an object from a single value KeyProperty.
		/// </summary>
		/// <typeparam name="KeyType">The type of the key.</typeparam>
		/// <param name="key">The value of the key.</param>
		/// <returns>A new object with the specified key.</returns>
		public static T CreateObject<KeyType>(KeyType key)
		{
			T objKey = (T) System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
			SetKey(objKey, key);
			return objKey;
		}

		/// <summary>
		/// Copies a single value to the KeyProperty of an object.
		/// </summary>
		public static void SetKey<KeyType>(T obj, KeyType key)
		{
			TableData<T>.Queries.KeyProperties[0].SetValue(obj, key);
		}

		/// <summary>
		/// Creates an ExpandoObject (key) from a single value.
		/// </summary>
		public static object CreateKey<KeyType>(KeyType value)
		{
			dynamic newKey = new ExpandoObject();
			newKey[TableData<T>.Queries.KeyProperties[0].Name] = value;
			return newKey;
		}

		/// <summary>
		/// Gets the value of the first key from an object. This assumes that there is only one KeyAttribute.
		/// </summary>
		/// <typeparam name="KeyType">The type of the key.</typeparam>
		/// <param name="obj">The input object to pull the key from.</param>
		/// <returns>The value of the key.</returns>
		public static KeyType GetKey<KeyType>(T obj)
		{
			return (KeyType) TableData<T>.Queries.KeyProperties[0].GetValue(obj);
		}


		/// <summary>
		/// Creates an object from a key where the type has identical KeyProperties (e.g. ExpandoObject or typeof(T)).
		/// </summary>
		public static T CreateObject(object key)
		{
			T objKey = (T) System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
			SetKey(objKey, key);
			return objKey;
		}

		/// <summary>
		/// Creates an object from a key where the type has identical KeyProperties (e.g. ExpandoObject or typeof(T)).
		/// </summary>
		public static void SetKey(T obj, object key)
		{
			if (key is IDictionary<string, object> expando) {
				for (int i = 0; i < KeyProperties.Length; i++) {
					KeyProperties[i].SetValue(obj, expando[KeyProperties[i].Name]);
				}
			}
			else {
				Type type = key.GetType();
				for (int i = 0; i < KeyProperties.Length; i++) {
					KeyProperties[i].SetValue(obj, type.GetProperty(KeyProperties[i].Name, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance).GetValue(key));
				}
			}
		}

		/// <summary>
		/// Gets an ExpandoObject T with the keys filled in.
		/// </summary>
		/// <param name="obj">The input object to pull keys from.</param>
		/// <returns>An ExpandoObject with keys from the input.</returns>
		public static object GetKey(T obj)
		{
			dynamic key = new ExpandoObject();
			for (int i = 0; i < KeyProperties.Length; i++) {
				key[KeyProperties[i].Name] = KeyProperties[i].GetValue(obj);
			}
			return key;
		}


		/// <summary>
		/// Gets the value of the first key from an object. This assumes that there is only one KeyAttribute.
		/// </summary>
		/// <typeparam name="KeyType">The type of the key.</typeparam>
		/// <param name="obj">The input object to pull the key from.</param>
		/// <returns>The value of the key.</returns>
		public static KeyType GetKey<KeyType>(object obj)
		{
			dynamic key = new ExpandoObject();
			for (int i = 0; i < KeyProperties.Length; i++) {
				key[KeyProperties[i].Name] = KeyProperties[i].GetValue(obj);
			}
			return key;
		}

		/// <summary>
		/// Creates a shallow clone of the object.
		/// </summary>
		public static T Clone(T source)
		{
			T dest = (T) System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
			for (int i = 0; i < Properties.Length; i++) {
				Properties[i].SetValue(dest, Properties[i].GetValue(source));
			}
			return dest;
		}

		/// <summary>
		/// Returns true if the destination was modified, or false if they were identical.
		/// </summary>
		public static bool Copy(T source, T dest)
		{
			for (int i = 0; i < Properties.Length; i++) {
				object sourceValue = Properties[i].GetValue(source);
				object destValue = Properties[i].GetValue(dest);
				if (sourceValue != destValue) {
					for (int j = i; j < Properties.Length; j++) {
						Properties[j].SetValue(dest, Properties[j].GetValue(source));
					}
					return true;
				}
			}
			return false;
		}
	}
}
