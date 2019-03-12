using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection
{
	public static class Reflect
	{
		internal static Type[] objectArrayParam = new Type[1] { typeof(object) };
		public static bool Concurrent = false;

		public static void ClearCache(bool resize = false)
		{
			Reflect<object>.ClearCache(resize);
		}

		public static void SetConcurrent()
		{
			Reflect<object>.SetConcurrent();
		}

		public static Ctor<object> Constructor(Type type, params Type[] paramTypes)
		{
			return Reflect<object>.Constructor(type, paramTypes);
		}

		public static Func<object, TReturn> Getter<TReturn>(PropertyInfo property)
		{
			return Reflect<object>.Getter<TReturn>(property);
		}

		public static Func<object, object> Getter(PropertyInfo property)
		{
			return Reflect<object>.Getter<object>(property);
		}

		public static Func<object, TReturn> Setter<TReturn>(PropertyInfo property)
		{
			return Reflect<object>.Setter<TReturn>(property);
		}

		public static Func<object, object> Setter(PropertyInfo property)
		{
			return Reflect<object>.Setter<object>(property);
		}

		public static Func<object, TReturn> Getter<TReturn>(FieldInfo field)
		{
			return Reflect<object>.Getter<TReturn>(field);
		}

		public static Func<object, object> Getter(FieldInfo field)
		{
			return Reflect<object>.Getter<object>(field);
		}

		public static Func<object, TReturn> Setter<TReturn>(FieldInfo field)
		{
			return Reflect<object>.Setter<TReturn>(field);
		}

		public static Func<object, object> Setter(FieldInfo field)
		{
			return Reflect<object>.Setter<object>(field);
		}

		public static Func<object, TReturn> Func<TReturn>(MethodInfo method)
		{
			return Reflect.Func<TReturn>(method);
		}

		public static Func<object, object> Func(MethodInfo method)
		{
			return Reflect.Func<object>(method);
		}

		public static Action<object, object[]> Action(MethodInfo method)
		{
			return Reflect.Action(method);
		}
	}

	public static class Reflect<TTarget>
	{
		private static readonly ReflectCache<TTarget> Cache;
		private static IDictionary<string, Delegate> Getters;
		private static IDictionary<string, Delegate> Setters;
		private static IDictionary<string, Delegate> Methods;

		public static readonly Func<TTarget> New;

		static Reflect()
		{
			Cache = new ReflectCache<TTarget>();
			New = ReflectCache<TTarget>.New;
			ClearCache(true);
		}

		public static void ClearCache(bool resize = false)
		{
			Cache.Clear(resize);
			if (!resize) {
				Getters.Clear();
				Setters.Clear();
			}
			else {
				if (Getters is ConcurrentDictionary<string, Delegate>) {
					Getters = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);
					Setters = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);
					Methods = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);
				}
				else {
					Getters = new Dictionary<string, Delegate>(StringComparer.Ordinal);
					Setters = new Dictionary<string, Delegate>(StringComparer.Ordinal);
					Methods = new Dictionary<string, Delegate>(StringComparer.Ordinal);
				}
			}
		}

		public static void SetConcurrent()
		{
			if (!(Getters is ConcurrentDictionary<string, Delegate>)) {
				Getters = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);
				Setters = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);
				Methods = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);
				Cache.SetConcurrent();
			}
		}

		public static Ctor<TTarget> Constructor(params Type[] paramTypes)
		{
			return (Ctor<TTarget>) Cache.DelegateForCtor(typeof(Ctor<TTarget>), typeof(TTarget), Reflect.objectArrayParam, paramTypes);
		}

		public static Ctor<TTarget> Constructor(Type type, params Type[] paramTypes)
		{
			return (Ctor<TTarget>) Cache.DelegateForCtor(typeof(Ctor<TTarget>), type, Reflect.objectArrayParam, paramTypes);
		}

		public static Func<TTarget, TReturn> Getter<TReturn>(PropertyInfo property)
		{
			return Cache.DelegateForGet<TReturn>(property);
		}

		public static Func<TTarget, object> Getter(PropertyInfo property)
		{
			return Cache.DelegateForGet<object>(property);
		}

		public static Func<TTarget, TReturn> Setter<TReturn>(PropertyInfo property)
		{
			return Cache.DelegateForSet<TReturn>(property);
		}

		public static Func<TTarget, object> Setter(PropertyInfo property)
		{
			return Cache.DelegateForSet<object>(property);
		}

		public static Func<TTarget, TReturn> Getter<TReturn>(string name)
		{
			if (Getters.TryGetValue(name, out Delegate result) && result is Func<TTarget, TReturn> getter) {
				return getter;
			}
			Func<TTarget, TReturn> method = _Getter<TReturn>(name, BindingFlags.Instance | BindingFlags.DeclaredOnly);
			if (method == null) {
				method = _Getter<TReturn>(name, BindingFlags.FlattenHierarchy);
				if (method == null) {
					throw new InvalidOperationException("No field or property with the name " + name);
				}
			}
			Getters[name] = method;
			return method;
		}

		private static Func<TTarget, TReturn> _Getter<TReturn>(string name, BindingFlags flags)
		{
			FieldInfo field = typeof(TTarget).GetFields(flags).FirstOrDefault(f => f.Name == name);
			if (field != null) {
				return Cache.DelegateForGet<TReturn>(field);
			}
			PropertyInfo property = typeof(TTarget).GetProperties(flags).FirstOrDefault(p => p.Name == name);
			if (property != null) {
				return Cache.DelegateForGet<TReturn>(property);
			}
			return null;
		}

		public static Func<TTarget, TReturn> Getter<TReturn>(FieldInfo field)
		{
			return Cache.DelegateForGet<TReturn>(field);
		}

		public static Func<TTarget, object> Getter(string name)
		{
			return Getter<object>(name);
		}

		public static Func<TTarget, object> Getter(FieldInfo field)
		{
			return Cache.DelegateForGet<object>(field);
		}

		public static Func<TTarget, TReturn> Setter<TReturn>(string name)
		{
			if (Getters.TryGetValue(name, out Delegate result) && result is Func<TTarget, TReturn> setter) {
				return setter;
			}
			Func<TTarget, TReturn> method = _Getter<TReturn>(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			if (method == null) {
				method = _Getter<TReturn>(name, BindingFlags.FlattenHierarchy);
				if (method == null) {
					throw new InvalidOperationException("No field or property with the name " + name);
				}
			}
			Setters[name] = method;
			return method;
		}

		private static Func<TTarget, TReturn> _Setter<TReturn>(string name, BindingFlags flags)
		{
			FieldInfo field = typeof(TTarget).GetFields(flags).FirstOrDefault(f => f.Name == name);
			if (field != null) {
				return Cache.DelegateForSet<TReturn>(field);
			}
			PropertyInfo property = typeof(TTarget).GetProperties(flags).FirstOrDefault(p => p.Name == name);
			if (property != null) {
				return Cache.DelegateForSet<TReturn>(property);
			}
			return null;
		}

		public static Func<TTarget, TReturn> Setter<TReturn>(FieldInfo field)
		{
			return Cache.DelegateForSet<TReturn>(field);
		}

		public static Func<TTarget, object> Setter(string name)
		{
			return Setter<object>(name);
		}

		public static Func<TTarget, object> Setter(FieldInfo field)
		{
			return Cache.DelegateForSet<object>(field);
		}

		public static Invoker<TTarget, TReturn> Method<TReturn>(string name)
		{
			if (Methods.TryGetValue(name, out Delegate result) && result is Invoker<TTarget, TReturn> method) {
				return method;
			}
			Type type = typeof(TTarget);
			MethodInfo mi = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).FirstOrDefault(m => m.Name == name);
			if (mi == null) {
				mi = type.GetMethod(name);
			}
			Delegate invoker = Cache.DelegateForCall<TReturn>(mi);
			Methods[name] = invoker;
			return (Invoker<TTarget, TReturn>) invoker;
		}

		public static Invoker<TTarget, TReturn> Method<TReturn>(MethodInfo method)
		{
			return Cache.DelegateForCall<TReturn>(method);
		}

		public static Invoker<TTarget> Method(string name)
		{
			if (Methods.TryGetValue(name, out Delegate result) && result is Invoker<TTarget> method) {
				return method;
			}
			Type type = typeof(TTarget);
			MethodInfo mi = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).FirstOrDefault(m => m.Name == name);
			if(mi == null) {
				mi = type.GetMethod(name);
			}
			Delegate invoker = Cache.DelegateForCall(mi);
			Methods[name] = invoker;
			return (Invoker<TTarget>) invoker;
		}

		private static Delegate _Method(string name, BindingFlags flags)
		{
			MethodInfo method = typeof(TTarget).GetMethods(flags).FirstOrDefault(m => m.Name == name);
			return method == null ? null : Cache.DelegateForCall(method);
		}

		public static Invoker<TTarget> Method(MethodInfo method)
		{
			return Cache.DelegateForCall(method);
		}
	}
}
