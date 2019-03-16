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

	public delegate TReturn InvokerRef<TTarget, TReturn>(ref TTarget target, params object[] param);
	public delegate TReturn Invoker<TTarget, TReturn>(TTarget target, params object[] param);

	public delegate void InvokerRef<TTarget>(ref TTarget target, params object[] param);
	public delegate void Invoker<TTarget>(TTarget target, params object[] param);

	public delegate TValue GetterRef<TTarget, TValue>(ref TTarget target);
	public delegate void SetterRef<TTarget, TValue>(ref TTarget target, TValue value);

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

		public static Invoker<TTarget> Method<TTarget>(string name) where TTarget : class
		{
			return (Invoker<TTarget>) Methods<TTarget>.Create(name);
		}

		public static InvokerRef<TTarget> MethodRef<TTarget>(string name) where TTarget : struct
		{
			return (InvokerRef<TTarget>) Methods<TTarget>.Create(name);
		}

		public static Invoker<TTarget, TReturn> Method<TTarget, TReturn>(string name) where TTarget : class
		{
			return (Invoker<TTarget, TReturn>) Methods<TTarget, TReturn>.Create(name);
		}

		public static InvokerRef<TTarget, TReturn> MethodRef<TTarget, TReturn>(string name) where TTarget : struct
		{
			return (InvokerRef<TTarget, TReturn>) Methods<TTarget, TReturn>.Create(name);
		}

		public static Invoker<TTarget> Method<TTarget>(MethodInfo method) where TTarget : class
		{
			return (Invoker<TTarget>) Methods<TTarget>.Create(method);
		}

		public static InvokerRef<TTarget> MethodRef<TTarget>(MethodInfo method) where TTarget : struct
		{
			return (InvokerRef<TTarget>) Methods<TTarget>.Create(method);
		}

		public static Invoker<TTarget, TReturn> Method<TTarget, TReturn>(MethodInfo method) where TTarget : class
		{
			return (Invoker<TTarget, TReturn>) Methods<TTarget, TReturn>.Create(method);
		}

		public static InvokerRef<TTarget, TReturn> MethodRef<TTarget, TReturn>(MethodInfo method) where TTarget : struct
		{
			return (InvokerRef<TTarget, TReturn>) Methods<TTarget, TReturn>.Create(method);
		}

		public static Action<TTarget, TValue> Setter<TTarget, TValue>(string name) where TTarget : class
		{
			return (Action<TTarget, TValue>) Setters<TTarget, TValue>.Create(name);
		}

		public static SetterRef<TTarget, TValue> SetterRef<TTarget, TValue>(string name) where TTarget : struct
		{
			return (SetterRef<TTarget, TValue>) Setters<TTarget, TValue>.Create(name);
		}

		public static Action<TTarget, TValue> Setter<TTarget, TValue>(FieldInfo field) where TTarget : class
		{
			return (Action<TTarget, TValue>) Setters<TTarget, TValue>.Create(field);
		}

		public static SetterRef<TTarget, TValue> SetterRef<TTarget, TValue>(FieldInfo field) where TTarget : struct
		{
			return (SetterRef<TTarget, TValue>) Setters<TTarget, TValue>.Create(field);
		}

		public static Action<TTarget, TValue> Setter<TTarget, TValue>(PropertyInfo property) where TTarget : class
		{
			return (Action<TTarget, TValue>) Setters<TTarget, TValue>.Create(property);
		}

		public static SetterRef<TTarget, TValue> SetterRef<TTarget, TValue>(PropertyInfo property) where TTarget : struct
		{
			return (SetterRef<TTarget, TValue>) Setters<TTarget, TValue>.Create(property);
		}

		public static Func<TTarget, TValue> Getter<TTarget, TValue>(string name) where TTarget : class
		{
			return (Func<TTarget, TValue>) Getters<TTarget, TValue>.Create(name);
		}

		public static GetterRef<TTarget, TValue> GetterRef<TTarget, TValue>(string name) where TTarget : struct
		{
			return (GetterRef<TTarget, TValue>) Getters<TTarget, TValue>.Create(name);
		}

		public static Func<TTarget, TValue> Getter<TTarget, TValue>(FieldInfo field) where TTarget : class
		{
			return (Func<TTarget, TValue>) Getters<TTarget, TValue>.Create(field);
		}

		public static GetterRef<TTarget, TValue> GetterRef<TTarget, TValue>(FieldInfo field) where TTarget : struct
		{
			return (GetterRef<TTarget, TValue>) Getters<TTarget, TValue>.Create(field);
		}

		public static Func<TTarget, TValue> Getter<TTarget, TValue>(PropertyInfo property) where TTarget : class
		{
			return (Func<TTarget, TValue>) Getters<TTarget, TValue>.Create(property);
		}

		public static GetterRef<TTarget, TValue> GetterRef<TTarget, TValue>(PropertyInfo property) where TTarget : struct
		{
			return (GetterRef<TTarget, TValue>) Getters<TTarget, TValue>.Create(property);
		}
	}
}
