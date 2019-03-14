using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities.Reflection.Cache;

namespace Utilities.Reflection
{
	public delegate TTarget Ctor<TTarget>(params object[] param);
	public delegate TReturn Invoker<TTarget, TReturn>(TTarget target, params object[] param);
	public delegate void Invoker<TTarget>(TTarget target, params object[] param);

	public static class Reflect
	{
		public static bool Concurrent = false;

		public static Func<TTarget> Constructor<TTarget>()
		{
			return Constructors<TTarget>.New;
		}

		public static Ctor<TTarget> Constructor<TTarget>(params Type[] paramTypes)
		{
			return Constructors<TTarget>.Create(paramTypes);
		}

		public static Invoker<TTarget> Method<TTarget>(string name)
		{
			return Methods<TTarget>.Create(name);
		}

		public static Invoker<TTarget, TReturn> Method<TTarget, TReturn>(string name)
		{
			return Methods<TTarget, TReturn>.Create(name);
		}

		public static Invoker<TTarget> Method<TTarget>(MethodInfo method)
		{
			return Methods<TTarget>.Create(method);
		}

		public static Invoker<TTarget, TReturn> Method<TTarget, TReturn>(MethodInfo method)
		{
			return Methods<TTarget, TReturn>.Create(method);
		}

		public static Action<TTarget, TValue> Setter<TTarget, TValue>(string name)
		{
			return Setters<TTarget, TValue>.Create(name);
		}

		public static Action<TTarget, TValue> Setter<TTarget, TValue>(FieldInfo field)
		{
			return Setters<TTarget, TValue>.Create(field);
		}

		public static Action<TTarget, TValue> Setter<TTarget, TValue>(PropertyInfo property)
		{
			return Setters<TTarget, TValue>.Create(property);
		}

		public static Func<TTarget, TValue> Getter<TTarget, TValue>(string name)
		{
			return Getters<TTarget, TValue>.Create(name);
		}

		public static Func<TTarget, TValue> Getter<TTarget, TValue>(FieldInfo field)
		{
			return Getters<TTarget, TValue>.Create(field);
		}

		public static Func<TTarget, TValue> Getter<TTarget, TValue>(PropertyInfo property)
		{
			return Getters<TTarget, TValue>.Create(property);
		}
	}
}
