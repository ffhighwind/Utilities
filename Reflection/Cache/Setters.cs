﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection.Cache
{
	public static class Setters<TTarget, TValue>
	{
		private static IDictionary<PropertyKey, Action<TTarget, TValue>> Properties;
		private static IDictionary<FieldKey, Action<TTarget, TValue>> Fields;
		private static IDictionary<string, Action<TTarget, TValue>> Names;

		static Setters()
		{
			if (Reflect.Concurrent) {
				Properties = new ConcurrentDictionary<PropertyKey, Action<TTarget, TValue>>(PropertyKey.Comparer);
				Fields = new ConcurrentDictionary<FieldKey, Action<TTarget, TValue>>(FieldKey.Comparer);
				Names = new ConcurrentDictionary<string, Action<TTarget, TValue>>(StringComparer.Ordinal);
			}
			else {
				Properties = new Dictionary<PropertyKey, Action<TTarget, TValue>>(PropertyKey.Comparer);
				Fields = new Dictionary<FieldKey, Action<TTarget, TValue>>(FieldKey.Comparer);
				Names = new Dictionary<string, Action<TTarget, TValue>>(StringComparer.Ordinal);
			}
		}

		public static void SetConcurrent(bool concurrent = true)
		{
			if (concurrent) {
				if (Properties is Dictionary<PropertyKey, Invoker<TTarget>>) {
					Properties = new ConcurrentDictionary<PropertyKey, Action<TTarget, TValue>>(Properties, PropertyKey.Comparer);
					Fields = new ConcurrentDictionary<FieldKey, Action<TTarget, TValue>>(Fields, FieldKey.Comparer);
					Names = new ConcurrentDictionary<string, Action<TTarget, TValue>>(Names, StringComparer.Ordinal);
				}
			}
			else if (Properties is ConcurrentDictionary<PropertyKey, Invoker<TTarget>>) {
				Properties = new Dictionary<PropertyKey, Action<TTarget, TValue>>(Properties, PropertyKey.Comparer);
				Fields = new Dictionary<FieldKey, Action<TTarget, TValue>>(Fields, FieldKey.Comparer);
				Names = new Dictionary<string, Action<TTarget, TValue>>(Names, StringComparer.Ordinal);
			}
		}

		public static void ClearCache(bool resize = false)
		{
			if (resize) {
				if (Properties is Dictionary<PropertyKey, Invoker<TTarget>>) {
					Properties = new Dictionary<PropertyKey, Action<TTarget, TValue>>(PropertyKey.Comparer);
					Fields = new Dictionary<FieldKey, Action<TTarget, TValue>>(FieldKey.Comparer);
					Names = new Dictionary<string, Action<TTarget, TValue>>(StringComparer.Ordinal);
				}
				else {
					Properties = new ConcurrentDictionary<PropertyKey, Action<TTarget, TValue>>(PropertyKey.Comparer);
					Fields = new ConcurrentDictionary<FieldKey, Action<TTarget, TValue>>(FieldKey.Comparer);
					Names = new ConcurrentDictionary<string, Action<TTarget, TValue>>(StringComparer.Ordinal);
				}
			}
			else {
				Properties.Clear();
				Fields.Clear();
				Names.Clear();
			}
		}

		public static Action<TTarget, TValue> Create(string name)
		{
			if (!Names.TryGetValue(name, out Action<TTarget, TValue> result)) {
				Type type = typeof(TTarget);
				FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				if (field == null) {
					field = type.GetField(name);
				}
				if (field != null) {
					result = Create(field);
				}
				else {
					PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					if (property == null) {
						property = type.GetProperty(name);
					}
					if (property == null) {
						throw new InvalidOperationException("No field or property with the name " + name);
					}
					result = Create(property);
				}
				Names[name] = result;
			}
			return result;
		}

		public static Action<TTarget, TValue> Create(PropertyInfo property)
		{
			PropertyKey key = new PropertyKey()
			{
				Property = property,
				Type = typeof(TTarget)
			};
			if (!Properties.TryGetValue(key, out Action<TTarget, TValue> result)) {
				result = ReflectGen<TTarget>.DelegateForSet<TValue>(property);
				Properties[key] = result;
			}
			return result;
		}

		public static Action<TTarget, TValue> Create(FieldInfo field)
		{
			FieldKey key = new FieldKey()
			{
				Field = field,
				Type = typeof(TTarget)
			};
			if (!Fields.TryGetValue(key, out Action<TTarget, TValue> result)) {
				result = ReflectGen<TTarget>.DelegateForSet<TValue>(field);
				Fields[key] = result;
			}
			return result;
		}
	}
}
