using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

/// <summary>
/// A modified implementation of Vexe's FastReflection.
/// Original authors: Vexe and GeorgeR
/// <see cref="https://github.com/vexe/Fast.Reflection/blob/master/FastReflection.cs"/>
/// </summary>
namespace Utilities.Reflection.Cache
{

	internal static class ReflectGen
	{
		public static Type[] objectArrayParam = new Type[] { typeof(object[]) };
	}

	/// <summary>
	/// A dynamic reflection extensions library that emits IL to set/get fields/properties, call methods and invoke constructors
	/// Once the delegate is created, it can be stored and reused resulting in much faster access times than using regular reflection
	/// The results are cached. Once a delegate is generated, any subsequent call to generate the same delegate on the same field/property/method will return the previously generated delegate
	/// Note: Since this generates IL, it won't work on AOT platforms such as iOS an Android. But is useful and works very well in editor codes and standalone targets
	/// <see cref="https://github.com/vexe/Fast.Reflection/blob/master/FastReflection.cs"/>
	/// </summary>
	public static class ReflectGen<TTarget>
	{
		private static Delegate GenDelegateForMember<TMember>(Type delegateType, TMember member, string dynMethodName,
			Action<ILGenerator, TMember> generator, Type returnType, params Type[] paramTypes)
			where TMember : MemberInfo
		{
			DynamicMethod dynMethod = new DynamicMethod(dynMethodName, returnType, paramTypes, true);
			ILGenerator emit = dynMethod.GetILGenerator();
			generator(emit, member);
			Delegate result = dynMethod.CreateDelegate(delegateType);
			return result;
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public static Delegate DelegateForCtor(Type delegateType, Type type, Type[] inputParamTypes, params Type[] paramTypes)
		{
			string methodName = type.Name + "(" + string.Join(",", paramTypes.Select(p => p.Name)) + ")";
			DynamicMethod dynMethod = new DynamicMethod(methodName, type, inputParamTypes, true);
			ILGenerator emit = dynMethod.GetILGenerator();
			ReflectEmit<TTarget>.GenCtor(emit, type, paramTypes);
			Delegate result = dynMethod.CreateDelegate(delegateType);
			return result;
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public static Ctor<TTarget> DelegateForCtor(Type type, params Type[] paramTypes)
		{
			return (Ctor<TTarget>) DelegateForCtor(typeof(Ctor<TTarget>), type, ReflectGen.objectArrayParam, paramTypes);
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public static Ctor<TTarget> DelegateForCtor(params Type[] paramTypes)
		{
			return (Ctor<TTarget>) DelegateForCtor(typeof(Ctor<TTarget>), typeof(TTarget), ReflectGen.objectArrayParam, paramTypes);
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		public static Func<TTarget, TReturn> DelegateForGet<TReturn>(PropertyInfo property)
		{
			if (!property.CanRead) {
				throw new InvalidOperationException("Property is not readable: " + property.Name);
			}
			//return (Func<TTarget, TReturn>) property.GetGetMethod().CreateDelegate(typeof(Func<TTarget, TReturn>));
			Delegate result = GenDelegateForMember<PropertyInfo>(
				typeof(Func<TTarget, TReturn>),
				property,
				"get_" + property.Name + "(" + property.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.GenPropertyGetter,
				typeof(TReturn),
				typeof(TTarget));
			return (Func<TTarget, TReturn>) result;
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		public static Action<TTarget, TValue> DelegateForSet<TValue>(PropertyInfo property)
		{
			if (!property.CanWrite) {
				throw new InvalidOperationException("Property is not writable " + property.Name);
			}
			//return (Action<TTarget, TValue>) property.GetSetMethod().CreateDelegate(typeof(Action<TTarget, TValue>));

			Delegate result = GenDelegateForMember<PropertyInfo>(
				typeof(Action<TTarget, TValue>),
				property,
				"set_" + property.Name + "(" + property.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.GenPropertySetter,
				null,
				typeof(TTarget), 
				//typeof(TTarget).MakeByRefType(),
				typeof(TValue));
			return (Action<TTarget, TValue>) result;
		}

		/// <summary>
		/// Generates an open-instance delegate to get the value of the property from a given target
		/// </summary>
		public static Func<TTarget, TReturn> DelegateForGet<TReturn>(FieldInfo field)
		{
			Delegate result = GenDelegateForMember<FieldInfo>(
				typeof(Func<TTarget, TReturn>),
				field,
				"get_" + field.Name + "(" + field.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.GenFieldGetter,
				typeof(TReturn),
				typeof(TTarget));
			return (Func<TTarget, TReturn>) result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public static Action<TTarget, TValue> DelegateForSet<TValue>(FieldInfo field)
		{
			Delegate result = GenDelegateForMember<FieldInfo>(
				typeof(Action<TTarget, TValue>),
				field,
				"set_" + field.Name + "(" + field.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.GenFieldSetter,
				null,
				typeof(TTarget), 
				//typeof(TTarget).MakeByRefType(),
				typeof(TValue));
			return (Action<TTarget, TValue>) result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static Invoker<TTarget> DelegateForCall(MethodInfo method)
		{
			Delegate result = GenDelegateForMember<MethodInfo>(
				typeof(Invoker<TTarget>),
				method,
				method.Name + "(" + method.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.GenMethodInvocation,
				null,
				typeof(TTarget),
				typeof(object[]));
			return (Invoker<TTarget>) result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static Invoker<TTarget, TReturn> DelegateForCall<TReturn>(MethodInfo method)
		{
			Delegate result = GenDelegateForMember<MethodInfo>(
				typeof(Invoker<TTarget, TReturn>),
				method,
				method.Name + "(" + method.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.GenMethodInvocation,
				typeof(TReturn),
				typeof(TTarget),
				typeof(object[]));
			return (Invoker<TTarget, TReturn>) result;
		}
	}
}