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
		private static readonly ReflectCache Default = new ReflectCache();

		public static Func<object> Constructor(Type type, params Type[] paramTypes)
		{
			return Default.DelegateForCtor(type, paramTypes);
		}

		public static Func<object, TReturn> Getter<TReturn>(PropertyInfo property)
		{
			return Default.DelegateForGet<object, TReturn>(property);
		}

		public static Func<object, object> Getter(PropertyInfo property)
		{
			return Default.DelegateForGet(property);
		}

		public static Func<object, TReturn> Setter<TReturn>(PropertyInfo property)
		{
			return Default.DelegateForSet<object, TReturn>(property);
		}

		public static Func<object, object> Setter(PropertyInfo property)
		{
			return Default.DelegateForSet(property);
		}

		public static Func<object, TReturn> Getter<TReturn>(FieldInfo field)
		{
			return Default.DelegateForGet<object, TReturn>(field);
		}

		public static Func<object, object> Getter(FieldInfo field)
		{
			return Default.DelegateForGet(field);
		}

		public static Func<object, TReturn> Setter<TReturn>(FieldInfo field)
		{
			return Default.DelegateForSet<object, TReturn>(field);
		}

		public static Func<object, object> Setter(FieldInfo field)
		{
			return Default.DelegateForSet(field);
		}

		public static Func<object, TReturn> Func<TReturn>(MethodInfo method)
		{
			return (Func<object, TReturn>) Default.DelegateForCall<object, TReturn>(method);
		}

		public static Func<object, object> Func(MethodInfo method)
		{
			return (Func<object, object>) Default.DelegateForCall(method);
		}

		public static Action<object, object[]> Action(MethodInfo method)
		{
			return (Action<object, object[]>) Default.DelegateForCall(method);
		}
	}

	public static class Reflect<TTarget>
	{
		private static readonly ReflectCache Default = new ReflectCache();

		static Reflect()
		{
			try {
				New = Constructor();
			}
			catch { }
		}
		public static readonly Func<TTarget> New;

		public static Func<TTarget> Constructor(params Type[] paramTypes)
		{
			return Default.DelegateForCtor<TTarget>(typeof(TTarget), paramTypes);
		}

		public static Func<TTarget, TReturn> Getter<TReturn>(PropertyInfo property)
		{
			return Default.DelegateForGet<TTarget, TReturn>(property);
		}

		public static Func<TTarget, object> Getter(PropertyInfo property)
		{
			return Default.DelegateForGet<TTarget, object>(property);
		}

		public static Func<TTarget, TReturn> Setter<TReturn>(PropertyInfo property)
		{
			return Default.DelegateForSet<TTarget, TReturn>(property);
		}

		public static Func<TTarget, object> Setter(PropertyInfo property)
		{
			return Default.DelegateForSet<TTarget, object>(property);
		}

		public static Func<TTarget, TReturn> Getter<TReturn>(FieldInfo field)
		{
			return Default.DelegateForGet<TTarget, TReturn>(field);
		}

		public static Func<TTarget, object> Getter(FieldInfo field)
		{
			return Default.DelegateForGet<TTarget, object>(field);
		}

		public static Func<TTarget, TReturn> Setter<TReturn>(FieldInfo field)
		{
			return Default.DelegateForSet<TTarget, TReturn>(field);
		}

		public static Func<TTarget, object> Setter(FieldInfo field)
		{
			return Default.DelegateForSet<TTarget, object>(field);
		}

		public static Func<TTarget, TReturn> Func<TReturn>(MethodInfo method)
		{
			return (Func<TTarget, TReturn>) Default.DelegateForCall<TTarget, TReturn>(method);
		}

		public static Func<TTarget, object> Func(MethodInfo method)
		{
			return (Func<TTarget, object>) Default.DelegateForCall<TTarget, object>(method);
		}

		public static Action<TTarget> Action(MethodInfo method)
		{
			return (Action<TTarget>) Default.DelegateForCall<TTarget, object>(method);
		}
	}
}
