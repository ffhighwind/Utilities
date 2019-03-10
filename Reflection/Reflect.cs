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
		public static void ClearCache(bool resize = false)
		{
			Reflect<object>.ClearCache(resize);
		}

		public static void SetConcurrent()
		{
			Reflect<object>.SetConcurrent();
		}

		public static Func<object> Constructor(Type type, params Type[] paramTypes)
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
		private static readonly ReflectCache<TTarget> Cache = new ReflectCache<TTarget>();

		public static readonly Func<TTarget> New = ReflectCache<TTarget>.New;

		public static void ClearCache(bool resize = false)
		{
			Cache.Clear(resize);
		}

		public static void SetConcurrent()
		{
			Cache.SetConcurrent();
		}

		public static Func<TTarget> Constructor(Type type, params Type[] paramTypes)
		{
			return Cache.DelegateForCtor(type, paramTypes);
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

		public static Func<TTarget, TReturn> Getter<TReturn>(FieldInfo field)
		{
			return Cache.DelegateForGet<TReturn>(field);
		}

		public static Func<TTarget, object> Getter(FieldInfo field)
		{
			return Cache.DelegateForGet<object>(field);
		}

		public static Func<TTarget, TReturn> Setter<TReturn>(FieldInfo field)
		{
			return Cache.DelegateForSet<TReturn>(field);
		}

		public static Func<TTarget, object> Setter(FieldInfo field)
		{
			return Cache.DelegateForSet<object>(field);
		}

		public static Func<TTarget, TReturn> Func<TReturn>(MethodInfo method)
		{
			return (Func<TTarget, TReturn>) Cache.DelegateForCall<TReturn>(method);
		}

		public static Func<TTarget, object> Func(MethodInfo method)
		{
			return (Func<TTarget, object>) Cache.DelegateForCall<object>(method);
		}

		public static Action<TTarget> Action(MethodInfo method)
		{
			return (Action<TTarget>) Cache.DelegateForCall<object>(method);
		}
	}
}
