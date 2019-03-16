using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection.Cache
{
	public static class Methods<TTarget, TReturn>
	{
		public static void SetConcurrent(bool concurrent = true)
		{
			_Methods<TTarget, TReturn, Invoker<TTarget, TReturn>>.SetConcurrent(concurrent);
		}

		public static void ClearCache(bool resize = false)
		{
			_Methods<TTarget, TReturn, Invoker<TTarget, TReturn>>.ClearCache(resize);
		}
	}

	public static class MethodsRef<TTarget, TReturn>
	{
		public static void SetConcurrent(bool concurrent = true)
		{
			_Methods<TTarget, TReturn, InvokerRef<TTarget, TReturn>>.SetConcurrent(concurrent);
		}

		public static void ClearCache(bool resize = false)
		{
			_Methods<TTarget, TReturn, InvokerRef<TTarget, TReturn>>.ClearCache(resize);
		}
	}

	public static class Methods<TTarget>
	{
		public static void SetConcurrent(bool concurrent = true)
		{
			_Methods<TTarget, Invoker<TTarget>>.SetConcurrent(concurrent);
		}

		public static void ClearCache(bool resize = false)
		{
			_Methods<TTarget, Invoker<TTarget>>.ClearCache(resize);
		}
	}

	public static class MethodsRef<TTarget>
	{
		public static void SetConcurrent(bool concurrent = true)
		{
			_Methods<TTarget, InvokerRef<TTarget>>.SetConcurrent(concurrent);
		}

		public static void ClearCache(bool resize = false)
		{
			_Methods<TTarget, InvokerRef<TTarget>>.ClearCache(resize);
		}
	}

	internal static class _Methods
	{
		internal static MethodInfo GetMethod(Type type, string name)
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
	}

	internal static class _Methods<TTarget, TDelegate>
	{
		private static IDictionary<MethodKey, TDelegate> Invokers;
		private static IDictionary<string, TDelegate> Names;
		private static readonly Type TargetType;

		static _Methods()
		{
			if (Reflect.Concurrent) {
				Invokers = new ConcurrentDictionary<MethodKey, TDelegate>(MethodKey.Comparer);
				Names = new ConcurrentDictionary<string, TDelegate>(StringComparer.Ordinal);
			}
			else {
				Invokers = new Dictionary<MethodKey, TDelegate>(MethodKey.Comparer);
				Names = new Dictionary<string, TDelegate>(StringComparer.Ordinal);
			}
			TargetType = typeof(TDelegate) == typeof(InvokerRef<TTarget>) ? typeof(TTarget).MakeByRefType() : typeof(TTarget);
		}

		public static void SetConcurrent(bool concurrent = true)
		{
			if (concurrent) {
				if (Invokers is Dictionary<MethodKey, TDelegate>) {
					Invokers = new ConcurrentDictionary<MethodKey, TDelegate>(Invokers, MethodKey.Comparer);
					Names = new ConcurrentDictionary<string, TDelegate>(Names, StringComparer.Ordinal);
				}
			}
			else if (Invokers is ConcurrentDictionary<MethodKey, TDelegate>) {
				Invokers = new Dictionary<MethodKey, TDelegate>(Invokers, MethodKey.Comparer);
				Names = new Dictionary<string, TDelegate>(Names, StringComparer.Ordinal);
			}
		}

		public static void ClearCache(bool resize = false)
		{
			if (resize) {
				if (Invokers is Dictionary<MethodKey, TDelegate>) {
					Invokers = new Dictionary<MethodKey, TDelegate>(MethodKey.Comparer);
					Names = new Dictionary<string, TDelegate>(StringComparer.Ordinal);
				}
				else {
					Invokers = new ConcurrentDictionary<MethodKey, TDelegate>(MethodKey.Comparer);
					Names = new ConcurrentDictionary<string, TDelegate>(StringComparer.Ordinal);
				}
			}
			else {
				Invokers.Clear();
				Names.Clear();
			}
		}

		public static TDelegate Create(string name)
		{
			if (!Names.TryGetValue(name, out TDelegate result)) {
				Type type = typeof(TTarget);
				MethodInfo mi = _Methods.GetMethod(type, name);
				result = Create(mi);
				Names[name] = result;
			}
			return result;
		}

		public static TDelegate Create(MethodInfo method)
		{
			MethodKey key = new MethodKey()
			{
				Method = method,
				Type = typeof(TTarget)
			};
			if (!Invokers.TryGetValue(key, out TDelegate result)) {
				Delegate func = ReflectGen<TTarget>.Method(method, typeof(TDelegate), null, TargetType, typeof(object[]));
				result = (TDelegate) (object) func;
				Invokers[key] = result;
			}
			return result;
		}
	}

	internal static class _Methods<TTarget, TReturn, TDelegate>
	{
		private static IDictionary<MethodKey, TDelegate> Invokers;
		private static IDictionary<string, TDelegate> Names;
		private static readonly Type TargetType;

		static _Methods()
		{
			if(Reflect.Concurrent) {
				Invokers = new ConcurrentDictionary<MethodKey, TDelegate>(MethodKey.Comparer);
				Names = new ConcurrentDictionary<string, TDelegate>(StringComparer.Ordinal);
			}
			else {
				Invokers = new Dictionary<MethodKey, TDelegate>(MethodKey.Comparer);
				Names = new Dictionary<string, TDelegate>(StringComparer.Ordinal);
			}
			TargetType = typeof(TDelegate) == typeof(InvokerRef<TTarget, TReturn>) ? typeof(TTarget).MakeByRefType() : typeof(TTarget);
		}

		public static void SetConcurrent(bool concurrent = true)
		{
			if(concurrent) {
				Invokers = new ConcurrentDictionary<MethodKey, TDelegate>(Invokers, MethodKey.Comparer);
				Names = new ConcurrentDictionary<string, TDelegate>(Names, StringComparer.Ordinal);
			}
			else {
				Invokers = new Dictionary<MethodKey, TDelegate>(Invokers, MethodKey.Comparer);
				Names = new Dictionary<string, TDelegate>(Names, StringComparer.Ordinal);
			}
		}

		public static void ClearCache(bool resize = false)
		{
			if (resize) {
				if (Invokers is Dictionary<MethodKey, TDelegate>) {
					Invokers = new Dictionary<MethodKey, TDelegate>(MethodKey.Comparer);
					Names = new Dictionary<string, TDelegate>(StringComparer.Ordinal);
				}
				else {
					Invokers = new ConcurrentDictionary<MethodKey, TDelegate>(MethodKey.Comparer);
					Names = new ConcurrentDictionary<string, TDelegate>(StringComparer.Ordinal);
				}
			}
			else {
				Invokers.Clear();
				Names.Clear();
			}
		}

		public static TDelegate Create(string name)
		{
			if (!Names.TryGetValue(name, out TDelegate result)) {
				Type type = typeof(TTarget);
				MethodInfo mi = _Methods.GetMethod(type, name);
				result = Create(mi);
				Names[name] = result;
			}
			return result;
		}

		public static TDelegate Create(MethodInfo method)
		{
			MethodKey key = new MethodKey()
			{
				Method = method,
				Type = typeof(TTarget)
			};
			if (!Invokers.TryGetValue(key, out TDelegate result)) {
				Delegate func = ReflectGen<TTarget>.Method(method, typeof(TDelegate), typeof(TReturn), TargetType, typeof(object[]));
				result = (TDelegate) (object) func;
				Invokers[key] = result;
			}
			return result;
		}
	}
}
