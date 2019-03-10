using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection
{
	public static class Reflect
	{
		private static readonly ReflectCache Cache = new ReflectCache();

		public static void ClearCache(bool resize = false)
		{
			Cache.Clear(resize);
		}

		public static void SetConcurrent()
		{
			Cache.SetConcurrent();
		}

		public static Func<object> Constructor(Type type, params Type[] paramTypes)
		{
			return Cache.DelegateForCtor(type, paramTypes);
		}

		public static Func<object, TReturn> Getter<TReturn>(PropertyInfo property)
		{
			return Cache.DelegateForGet<object, TReturn>(property);
		}

		public static Func<object, object> Getter(PropertyInfo property)
		{
			return Cache.DelegateForGet(property);
		}

		public static Func<object, TReturn> Setter<TReturn>(PropertyInfo property)
		{
			return Cache.DelegateForSet<object, TReturn>(property);
		}

		public static Func<object, object> Setter(PropertyInfo property)
		{
			return Cache.DelegateForSet(property);
		}

		public static Func<object, TReturn> Getter<TReturn>(FieldInfo field)
		{
			return Cache.DelegateForGet<object, TReturn>(field);
		}

		public static Func<object, object> Getter(FieldInfo field)
		{
			return Cache.DelegateForGet(field);
		}

		public static Func<object, TReturn> Setter<TReturn>(FieldInfo field)
		{
			return Cache.DelegateForSet<object, TReturn>(field);
		}

		public static Func<object, object> Setter(FieldInfo field)
		{
			return Cache.DelegateForSet(field);
		}

		public static Func<object, TReturn> Func<TReturn>(MethodInfo method)
		{
			return (Func<object, TReturn>) Cache.DelegateForCall<object, TReturn>(method);
		}

		public static Func<object, object> Func(MethodInfo method)
		{
			return (Func<object, object>) Cache.DelegateForCall(method);
		}

		public static Action<object, object[]> Action(MethodInfo method)
		{
			return (Action<object, object[]>) Cache.DelegateForCall(method);
		}
	}

	public static class Reflect<TTarget>
	{
		private static readonly ReflectCache Cache = new ReflectCache();

		static Reflect()
		{
			try {
				New = Constructor();
			}
			catch { }
		}
		public static readonly Func<TTarget> New;

		public static void ClearCache(bool resize = false)
		{
			Cache.Clear(resize);
		}

		public static void SetConcurrent()
		{
			Cache.SetConcurrent();
		}

		public static Func<TTarget> Constructor(params Type[] paramTypes)
		{
			return Cache.DelegateForCtor<TTarget>(typeof(TTarget), paramTypes);
		}

		public static Func<TTarget, TReturn> Getter<TReturn>(PropertyInfo property)
		{
			return Cache.DelegateForGet<TTarget, TReturn>(property);
		}

		public static Func<TTarget, object> Getter(PropertyInfo property)
		{
			return Cache.DelegateForGet<TTarget, object>(property);
		}

		public static Func<TTarget, TReturn> Setter<TReturn>(PropertyInfo property)
		{
			return Cache.DelegateForSet<TTarget, TReturn>(property);
		}

		public static Func<TTarget, object> Setter(PropertyInfo property)
		{
			return Cache.DelegateForSet<TTarget, object>(property);
		}

		public static Func<TTarget, TReturn> Getter<TReturn>(FieldInfo field)
		{
			return Cache.DelegateForGet<TTarget, TReturn>(field);
		}

		public static Func<TTarget, object> Getter(FieldInfo field)
		{
			return Cache.DelegateForGet<TTarget, object>(field);
		}

		public static Func<TTarget, TReturn> Setter<TReturn>(FieldInfo field)
		{
			return Cache.DelegateForSet<TTarget, TReturn>(field);
		}

		public static Func<TTarget, object> Setter(FieldInfo field)
		{
			return Cache.DelegateForSet<TTarget, object>(field);
		}

		public static Func<TTarget, TReturn> Func<TReturn>(MethodInfo method)
		{
			return (Func<TTarget, TReturn>) Cache.DelegateForCall<TTarget, TReturn>(method);
		}

		public static Func<TTarget, object> Func(MethodInfo method)
		{
			return (Func<TTarget, object>) Cache.DelegateForCall<TTarget, object>(method);
		}

		public static Action<TTarget> Action(MethodInfo method)
		{
			return (Action<TTarget>) Cache.DelegateForCall<TTarget, object>(method);
		}
	}
}
