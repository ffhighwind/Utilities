using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection.Cache
{
	public static class Methods<TTarget>
	{
		private static IDictionary<MethodKey, Delegate> Invokers;
		private static IDictionary<string, Delegate> Names;

		static Methods()
		{
			if (Reflect.Concurrent) {
				Invokers = new ConcurrentDictionary<MethodKey, Delegate>(MethodKey.Comparer);
				Names = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);
			}
			else {
				Invokers = new Dictionary<MethodKey, Delegate>(MethodKey.Comparer);
				Names = new Dictionary<string, Delegate>(StringComparer.Ordinal);
			}
		}

		public static void SetConcurrent(bool concurrent = true)
		{
			if (concurrent) {
				if (Invokers is Dictionary<MethodKey, Delegate>) {
					Invokers = new ConcurrentDictionary<MethodKey, Delegate>(Invokers, MethodKey.Comparer);
					Names = new ConcurrentDictionary<string, Delegate>(Names, StringComparer.Ordinal);
				}
			}
			else if (Invokers is ConcurrentDictionary<MethodKey, Delegate>) {
				Invokers = new Dictionary<MethodKey, Delegate>(Invokers, MethodKey.Comparer);
				Names = new Dictionary<string, Delegate>(Names, StringComparer.Ordinal);
			}
		}

		public static void ClearCache(bool resize = false)
		{
			if (resize) {
				if (Invokers is Dictionary<MethodKey, Invoker<TTarget>>) {
					Invokers = new Dictionary<MethodKey, Delegate>(MethodKey.Comparer);
					Names = new Dictionary<string, Delegate>(StringComparer.Ordinal);
				}
				else {
					Invokers = new ConcurrentDictionary<MethodKey, Delegate>(MethodKey.Comparer);
					Names = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);
				}
			}
			else {
				Invokers.Clear();
				Names.Clear();
			}
		}

		public static Delegate Create(string name)
		{
			if (!Names.TryGetValue(name, out Delegate result)) {
				Type type = typeof(TTarget);
				MethodInfo mi = _Method(type, name);
				result = Create(mi);
				Names[name] = result;
			}
			return result;
		}

		internal static MethodInfo _Method(Type type, string name)
		{
			MethodInfo[] mis = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			for (int i = 0; i < mis.Length; i++) {
				if (mis[i].Name == name) {
					MethodInfo mi = mis[i];
					for (i++; i < mis.Length; i++) {
						if (mis[i].Name == name) {
							throw new InvalidOperationException("Ambiguous method name: " + name);
						}
					}
					return mi;
				}
			}
			return type.GetMethod(name);
		}

		public static Delegate Create(MethodInfo method)
		{
			MethodKey key = new MethodKey()
			{
				Method = method,
				Type = typeof(TTarget)
			};
			if (!Invokers.TryGetValue(key, out Delegate result)) {
				if (method.DeclaringType.IsClass) {
					result = ReflectGen<TTarget>.DelegateForCall(method);
				}
				else {
					result = ReflectGen<TTarget>.DelegateForCallRef(method);
				}
				Invokers[key] = result;
			}
			return result;
		}
	}

	public static class Methods<TTarget, TReturn>
	{
		private static IDictionary<MethodKey, Delegate> Invokers;
		private static IDictionary<string, Delegate> Names;

		static Methods()
		{
			if(Reflect.Concurrent) {
				Invokers = new ConcurrentDictionary<MethodKey, Delegate>(MethodKey.Comparer);
				Names = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);
			}
			else {
				Invokers = new Dictionary<MethodKey, Delegate>(MethodKey.Comparer);
				Names = new Dictionary<string, Delegate>(StringComparer.Ordinal);
			}
		}

		public static void SetConcurrent(bool concurrent = true)
		{
			if(concurrent) {
				Invokers = new ConcurrentDictionary<MethodKey, Delegate>(Invokers, MethodKey.Comparer);
				Names = new ConcurrentDictionary<string, Delegate>(Names, StringComparer.Ordinal);
			}
			else {
				Invokers = new Dictionary<MethodKey, Delegate>(Invokers, MethodKey.Comparer);
				Names = new Dictionary<string, Delegate>(Names, StringComparer.Ordinal);
			}
		}

		public static void ClearCache(bool resize = false)
		{
			if (resize) {
				if (Invokers is Dictionary<MethodKey, Delegate>) {
					Invokers = new Dictionary<MethodKey, Delegate>(MethodKey.Comparer);
					Names = new Dictionary<string, Delegate>(StringComparer.Ordinal);
				}
				else {
					Invokers = new ConcurrentDictionary<MethodKey, Delegate>(MethodKey.Comparer);
					Names = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);
				}
			}
			else {
				Invokers.Clear();
				Names.Clear();
			}
		}

		public static Delegate Create(string name)
		{
			if (!Names.TryGetValue(name, out Delegate result)) {
				Type type = typeof(TTarget);
				MethodInfo mi = Methods<TTarget>._Method(type, name);
				result = Create(mi);
				Names[name] = result;
			}
			return result;
		}

		public static Delegate Create(MethodInfo method)
		{
			MethodKey key = new MethodKey()
			{
				Method = method,
				Type = typeof(TTarget)
			};
			if (!Invokers.TryGetValue(key, out Delegate result)) {
				if (method.DeclaringType.IsClass) {
					result = ReflectGen<TTarget>.DelegateForCall<TReturn>(method);
				}
				else {
					result = ReflectGen<TTarget>.DelegateForCallRef<TReturn>(method);
				}
				Invokers[key] = result;
			}
			return result;
		}
	}
}
