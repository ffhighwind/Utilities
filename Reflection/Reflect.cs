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
		internal static readonly ReflectCache Default = new ReflectCache();

		public static CtorInvoker<object> Constructor(Type type, params Type[] paramTypes)
		{
			return Default.DelegateForCtor(type, paramTypes);
		}

		public static MemberGetter<object, object> Getter(PropertyInfo property)
		{
			return Default.DelegateForGet(property);
		}

		public static MemberSetter<object, object> Setter(PropertyInfo property)
		{
			return Default.DelegateForSet(property);
		}

		public static MemberGetter<object, object> Getter(FieldInfo field)
		{
			return Default.DelegateForGet(field);
		}

		public static MemberSetter<object, object> Setter(FieldInfo field)
		{
			return Default.DelegateForSet(field);
		}

		public static MethodCaller<object, object> Method(MethodInfo method)
		{
			return Reflect.Default.DelegateForCall(method);
		}
	}

	public static class Reflect<T>
	{
		static Reflect() {
			try {
				New = Constructor();
			}
			catch { }
		}
		public static readonly CtorInvoker<T> New;

		public static CtorInvoker<T> Constructor(params Type[] paramTypes)
		{
			return Reflect.Default.DelegateForCtor<T>(typeof(T), paramTypes);
		}

		public static MemberGetter<TTarget, TReturn> Getter<TTarget, TReturn>(PropertyInfo property)
		{
			return Reflect.Default.DelegateForGet<TTarget, TReturn>(property);
		}

		public static MemberSetter<TTarget, TReturn> Setter<TTarget, TReturn>(PropertyInfo property)
		{
			return Reflect.Default.DelegateForSet<TTarget, TReturn>(property);
		}

		public static MemberGetter<TTarget, TReturn> Getter<TTarget, TReturn>(FieldInfo field)
		{
			return Reflect.Default.DelegateForGet<TTarget, TReturn>(field);
		}

		public static MemberSetter<TTarget, TReturn> Setter<TTarget, TReturn>(FieldInfo field)
		{
			return Reflect.Default.DelegateForSet<TTarget, TReturn>(field);
		}

		public static MethodCaller<TTarget, TReturn> Method<TTarget, TReturn>(MethodInfo method)
		{
			return Reflect.Default.DelegateForCall<TTarget, TReturn>(method);
		}
	}
}
