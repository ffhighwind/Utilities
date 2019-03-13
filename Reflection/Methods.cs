using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection
{
	public static class Methods<TTarget>
	{
		private static IDictionary<MethodKey, Invoker<TTarget>> Invokers;
		private static IDictionary<string, Invoker<TTarget>> Names;

		static Methods()
		{
			ClearCache(true);
		}

		public static void MakeConcurrent()
		{
			if (Invokers is Dictionary<MethodKey, Invoker<TTarget>>) {
				Invokers = new ConcurrentDictionary<MethodKey, Invoker<TTarget>>(Invokers, MethodKey.Comparer);
				Names = new ConcurrentDictionary<string, Invoker<TTarget>>(Names, StringComparer.Ordinal);
			}
		}

		public static void ClearCache(bool resize = false)
		{
			if (resize) {
				if (Invokers is Dictionary<MethodKey, Invoker<TTarget>>) {
					Invokers = new Dictionary<MethodKey, Invoker<TTarget>>(Invokers, MethodKey.Comparer);
					Names = new Dictionary<string, Invoker<TTarget>>(Names, StringComparer.Ordinal);
				}
				else {
					Invokers = new ConcurrentDictionary<MethodKey, Invoker<TTarget>>(Invokers, MethodKey.Comparer);
					Names = new ConcurrentDictionary<string, Invoker<TTarget>>(Names, StringComparer.Ordinal);
				}
			}
			else {
				Invokers.Clear();
				Names.Clear();
			}
		}

		public static Invoker<TTarget> Method(string name)
		{
			if (!Names.TryGetValue(name, out Invoker<TTarget> result)) {
				Type type = typeof(TTarget);
				MethodInfo mi = _Method(type, name);
				Invoker<TTarget> invoker = Method(mi);
				Names[name] = invoker;
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

		public static Invoker<TTarget> Method(MethodInfo method)
		{
			MethodKey key = new MethodKey()
			{
				Method = method,
				Type = typeof(TTarget)
			};
			if (!Invokers.TryGetValue(key, out Invoker<TTarget> result)) {
				result = ReflectGen<TTarget>.DelegateForCall(method);
				Invokers[key] = result;
			}
			return result;
		}
	}

	public static class Methods<TTarget, TReturn>
	{
		private static IDictionary<MethodKey, Invoker<TTarget, TReturn>> Invokers;
		private static IDictionary<string, Invoker<TTarget, TReturn>> Names;

		static Methods()
		{
			ClearCache(true);
		}

		public static void MakeConcurrent()
		{
			if (!(Invokers is ConcurrentDictionary<MethodKey, Invoker<TTarget>>)) {
				Invokers = new ConcurrentDictionary<MethodKey, Invoker<TTarget, TReturn>>(Invokers, MethodKey.Comparer);
				Names = new ConcurrentDictionary<string, Invoker<TTarget, TReturn>>(Names, StringComparer.Ordinal);
			}
		}

		public static void ClearCache(bool resize = false)
		{
			if (resize) {
				if (Invokers is ConcurrentDictionary<MethodKey, Invoker<TTarget>>) {
					Invokers = new ConcurrentDictionary<MethodKey, Invoker<TTarget, TReturn>>(Invokers, MethodKey.Comparer);
					Names = new ConcurrentDictionary<string, Invoker<TTarget, TReturn>>(Names, StringComparer.Ordinal);
				}
				else {
					Invokers = new Dictionary<MethodKey, Invoker<TTarget, TReturn>>(Invokers, MethodKey.Comparer);
					Names = new Dictionary<string, Invoker<TTarget, TReturn>>(Names, StringComparer.Ordinal);
				}
			}
			else {
				Invokers.Clear();
				Names.Clear();
			}
		}

		public static Invoker<TTarget, TReturn> Method(string name)
		{
			if (!Names.TryGetValue(name, out Invoker<TTarget, TReturn> result)) {
				Type type = typeof(TTarget);
				MethodInfo mi = Methods<TTarget>._Method(type, name);
				Invoker<TTarget, TReturn> invoker = Method(mi);
				Names[name] = invoker;
			}
			return result;
		}

		public static Invoker<TTarget, TReturn> Method(MethodInfo method)
		{
			MethodKey key = new MethodKey()
			{
				Method = method,
				Type = typeof(TTarget)
			};
			if (!Invokers.TryGetValue(key, out Invoker<TTarget, TReturn> result)) {
				result = ReflectGen<TTarget>.DelegateForCall<TReturn>(method);
				Invokers[key] = result;
			}
			return result;
		}
	}
}
