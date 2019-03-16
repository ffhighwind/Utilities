using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection.Cache
{
	public static class Getters<TTarget, TValue>
	{
		private static IDictionary<PropertyKey, Delegate> Properties;
		private static IDictionary<FieldKey, Delegate> Fields;
		private static IDictionary<string, Delegate> Names;

		static Getters()
		{
			if (Reflect.Concurrent) {
				Properties = new ConcurrentDictionary<PropertyKey, Delegate>(PropertyKey.Comparer);
				Fields = new ConcurrentDictionary<FieldKey, Delegate>(FieldKey.Comparer);
				Names = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);
			}
			else {
				Properties = new Dictionary<PropertyKey, Delegate>(PropertyKey.Comparer);
				Fields = new Dictionary<FieldKey, Delegate>(FieldKey.Comparer);
				Names = new Dictionary<string, Delegate>(StringComparer.Ordinal);
			}
		}

		public static void SetConcurrent(bool concurrent = true)
		{
			if (concurrent) {
				if (Properties is Dictionary<PropertyKey, Invoker<TTarget>>) {
					Properties = new ConcurrentDictionary<PropertyKey, Delegate>(Properties, PropertyKey.Comparer);
					Fields = new ConcurrentDictionary<FieldKey, Delegate>(Fields, FieldKey.Comparer);
					Names = new ConcurrentDictionary<string, Delegate>(Names, StringComparer.Ordinal);
				}
			}
			else if (Properties is ConcurrentDictionary<PropertyKey, Func<TTarget, TValue>>) {
				Properties = new Dictionary<PropertyKey, Delegate>(Properties, PropertyKey.Comparer);
				Fields = new Dictionary<FieldKey, Delegate>(Fields, FieldKey.Comparer);
				Names = new Dictionary<string, Delegate>(Names, StringComparer.Ordinal);
			}
		}

		public static void ClearCache(bool resize = false)
		{
			if (resize) {
				if (Properties is Dictionary<PropertyKey, Invoker<TTarget>>) {
					Properties = new Dictionary<PropertyKey, Delegate>(PropertyKey.Comparer);
					Fields = new Dictionary<FieldKey, Delegate>(FieldKey.Comparer);
					Names = new Dictionary<string, Delegate>(StringComparer.Ordinal);
				}
				else {
					Properties = new ConcurrentDictionary<PropertyKey, Delegate>(PropertyKey.Comparer);
					Fields = new ConcurrentDictionary<FieldKey, Delegate>(FieldKey.Comparer);
					Names = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);
				}
			}
			else {
				Properties.Clear();
				Fields.Clear();
				Names.Clear();
			}
		}

		public static Delegate Create(string name)
		{
			if (!Names.TryGetValue(name, out Delegate result)) {
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

		public static Delegate Create(PropertyInfo property)
		{
			PropertyKey key = new PropertyKey()
			{
				Property = property,
				Type = typeof(TTarget)
			};
			if (!Properties.TryGetValue(key, out Delegate result)) {
				if (property.DeclaringType.IsClass) {
					result = (Delegate) ReflectGen<TTarget>.DelegateForGet<TValue>(property);
				}
				else {
					result = (Delegate) ReflectGen<TTarget>.DelegateForGetRef<TValue>(property);
				}
				Properties[key] = result;
			}
			return result;
		}

		public static Delegate Create(FieldInfo field)
		{
			FieldKey key = new FieldKey()
			{
				Field = field,
				Type = typeof(TTarget)
			};
			if (!Fields.TryGetValue(key, out Delegate result)) {
				if (field.DeclaringType.IsClass) {
					result = ReflectGen<TTarget>.DelegateForGet<TValue>(field);
				}
				else {
					result = ReflectGen<TTarget>.DelegateForGetRef<TValue>(field);
				}
				Fields[key] = result;
			}
			return result;
		}
	}
}
