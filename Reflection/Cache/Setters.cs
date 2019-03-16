using System;
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
		public static void SetConcurrent(bool concurrent = true)
		{
			_Setters<TTarget, TValue, Action<TTarget, TValue>>.SetConcurrent(concurrent);
		}

		public static void ClearCache(bool resize = false)
		{
			_Setters<TTarget, TValue, Action<TTarget, TValue>>.ClearCache(resize);
		}
	}

	public static class SettersRef<TTarget, TValue>
	{
		public static void SetConcurrent(bool concurrent = true)
		{
			_Setters<TTarget, TValue, SetterRef<TTarget, TValue>>.SetConcurrent(concurrent);
		}

		public static void ClearCache(bool resize = false)
		{
			_Setters<TTarget, TValue, SetterRef<TTarget, TValue>>.ClearCache(resize);
		}
	}

	internal static class _Setters<TTarget, TValue, TDelegate>
	{
		private static IDictionary<PropertyKey, TDelegate> Properties;
		private static IDictionary<FieldKey, TDelegate> Fields;
		private static IDictionary<string, TDelegate> Names;
		private static readonly Type TargetType;

		static _Setters()
		{
			if (Reflect.Concurrent) {
				Properties = new ConcurrentDictionary<PropertyKey, TDelegate>(PropertyKey.Comparer);
				Fields = new ConcurrentDictionary<FieldKey, TDelegate>(FieldKey.Comparer);
				Names = new ConcurrentDictionary<string, TDelegate>(StringComparer.Ordinal);
			}
			else {
				Properties = new Dictionary<PropertyKey, TDelegate>(PropertyKey.Comparer);
				Fields = new Dictionary<FieldKey, TDelegate>(FieldKey.Comparer);
				Names = new Dictionary<string, TDelegate>(StringComparer.Ordinal);
			}
			TargetType = typeof(TDelegate) == typeof(SetterRef<TTarget, TValue>) ? typeof(TTarget).MakeByRefType() : typeof(TTarget);
		}

		public static void SetConcurrent(bool concurrent = true)
		{
			if (concurrent) {
				if (Properties is Dictionary<PropertyKey, Invoker<TTarget>>) {
					Properties = new ConcurrentDictionary<PropertyKey, TDelegate>(Properties, PropertyKey.Comparer);
					Fields = new ConcurrentDictionary<FieldKey, TDelegate>(Fields, FieldKey.Comparer);
					Names = new ConcurrentDictionary<string, TDelegate>(Names, StringComparer.Ordinal);
				}
			}
			else if (Properties is ConcurrentDictionary<PropertyKey, Invoker<TTarget>>) {
				Properties = new Dictionary<PropertyKey, TDelegate>(Properties, PropertyKey.Comparer);
				Fields = new Dictionary<FieldKey, TDelegate>(Fields, FieldKey.Comparer);
				Names = new Dictionary<string, TDelegate>(Names, StringComparer.Ordinal);
			}
		}

		public static void ClearCache(bool resize = false)
		{
			if (resize) {
				if (Properties is Dictionary<PropertyKey, Invoker<TTarget>>) {
					Properties = new Dictionary<PropertyKey, TDelegate>(PropertyKey.Comparer);
					Fields = new Dictionary<FieldKey, TDelegate>(FieldKey.Comparer);
					Names = new Dictionary<string, TDelegate>(StringComparer.Ordinal);
				}
				else {
					Properties = new ConcurrentDictionary<PropertyKey, TDelegate>(PropertyKey.Comparer);
					Fields = new ConcurrentDictionary<FieldKey, TDelegate>(FieldKey.Comparer);
					Names = new ConcurrentDictionary<string, TDelegate>(StringComparer.Ordinal);
				}
			}
			else {
				Properties.Clear();
				Fields.Clear();
				Names.Clear();
			}
		}

		public static TDelegate Create(string name)
		{
			if (!Names.TryGetValue(name, out TDelegate result)) {
				Type type = typeof(TTarget);
				FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				if (field == null) {
					PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					if (property == null) {
						field = type.GetField(name);
						if (field == null) {
							property = type.GetProperty(name);
							if (property == null) {
								throw new InvalidOperationException("No field or property with the name " + name);
							}
						}
					}
					if (property != null) {
						result = Create(property);
					}
				}
				if (field != null) {
					result = Create(field);
				}
				Names[name] = result;
			}
			return result;
		}

		public static TDelegate Create(PropertyInfo property)
		{
			PropertyKey key = new PropertyKey()
			{
				Property = property,
				Type = typeof(TTarget)
			};
			if (!Properties.TryGetValue(key, out TDelegate result)) {
				Delegate setter = ReflectGen<TTarget>.Setter(property, typeof(TDelegate), TargetType, typeof(TValue));
				result = (TDelegate) (object) setter;
				Properties[key] = result;
			}
			return result;
		}

		public static TDelegate Create(FieldInfo field)
		{
			FieldKey key = new FieldKey()
			{
				Field = field,
				Type = typeof(TTarget)
			};
			if (!Fields.TryGetValue(key, out TDelegate result)) {
				Delegate setter = ReflectGen<TTarget>.Setter(field, typeof(TDelegate), TargetType, typeof(TValue));
				result = (TDelegate) (object) setter;
				Fields[key] = result;
			}
			return result;
		}
	}
}
