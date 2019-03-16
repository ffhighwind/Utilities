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
		public static Func<TTarget> DelegateForCtor(Type type)
		{
			return (Func<TTarget>) _DelegateForCtor(typeof(Func<TTarget>), type, Array.Empty<Type>(), Array.Empty<Type>());
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public static Ctor<TTarget> DelegateForCtor(Type type, params Type[] paramTypes)
		{
			return (Ctor<TTarget>) _DelegateForCtor(typeof(Ctor<TTarget>), type, ReflectGen.objectArrayParam, paramTypes);
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public static Ctor<TTarget> DelegateForCtor(params Type[] paramTypes)
		{
			return (Ctor<TTarget>) _DelegateForCtor(typeof(Ctor<TTarget>), typeof(TTarget), ReflectGen.objectArrayParam, paramTypes);
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		private static Delegate _DelegateForCtor(Type delegateType, Type inputType, Type[] inputParamTypes, params Type[] paramTypes)
		{
			string methodName = inputType.Name + "(" + string.Join(",", paramTypes.Select(p => p.Name)) + ")";
			DynamicMethod dynMethod = new DynamicMethod(methodName, inputType, inputParamTypes, true);
			ILGenerator emit = dynMethod.GetILGenerator();
			ReflectEmit<TTarget>.GenCtor(emit, inputType, paramTypes);
			Delegate result = dynMethod.CreateDelegate(delegateType);
			return result;
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		public static Func<TTarget, TReturn> DelegateForGet<TReturn>(PropertyInfo property)
		{
			return (Func<TTarget, TReturn>) _DelegateForGet<TReturn>(typeof(Func<TTarget, TReturn>), typeof(TTarget), property);
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		public static GetterRef<TTarget, TReturn> DelegateForGetRef<TReturn>(PropertyInfo property)
		{
			return (GetterRef<TTarget, TReturn>) _DelegateForGet<TReturn>(typeof(GetterRef<TTarget, TReturn>), typeof(TTarget).MakeByRefType(), property);
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		private static Delegate _DelegateForGet<TReturn>(Type delegateType, Type inputType, PropertyInfo property)
		{
			if (!property.CanRead) {
				throw new InvalidOperationException("Property is not readable: " + property.Name);
			}
			Delegate result = GenDelegateForMember<PropertyInfo>(
				delegateType,
				property,
				"get_" + property.Name + "(" + property.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.GenPropertyGetter,
				typeof(TReturn),
				inputType);
			return result;
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		public static Action<TTarget, TValue> DelegateForSet<TValue>(PropertyInfo property)
		{
			return (Action<TTarget, TValue>) _DelegateForSet<TValue>(typeof(Action<TTarget, TValue>), typeof(TTarget), property);
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		public static SetterRef<TTarget, TValue> DelegateForSetRef<TValue>(PropertyInfo property)
		{
			return (SetterRef<TTarget, TValue>) _DelegateForSet<TValue>(typeof(SetterRef<TTarget, TValue>), typeof(TTarget), property);
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		private static Delegate _DelegateForSet<TValue>(Type delegateType, Type inputType, PropertyInfo property)
		{
			if (!property.CanWrite) {
				throw new InvalidOperationException("Property is not writable " + property.Name);
			}
			Delegate result = GenDelegateForMember<PropertyInfo>(
				delegateType,
				property,
				"set_" + property.Name + "(" + property.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.GenPropertySetter,
				null,
				inputType,
				typeof(TValue));
			return result;
		}

		/// <summary>
		/// Generates an open-instance delegate to get the value of the property from a given target
		/// </summary>
		public static Func<TTarget, TReturn> DelegateForGet<TReturn>(FieldInfo field)
		{
			return (Func<TTarget, TReturn>) _DelegateForGet<TReturn>(typeof(Func<TTarget, TReturn>), typeof(TTarget), field);
		}

		/// <summary>
		/// Generates an open-instance delegate to get the value of the property from a given target
		/// </summary>
		public static GetterRef<TTarget, TReturn> DelegateForGetRef<TReturn>(FieldInfo field)
		{
			return (GetterRef<TTarget, TReturn>) _DelegateForGet<TReturn>(typeof(GetterRef<TTarget, TReturn>), typeof(TTarget).MakeByRefType(), field);
		}

		/// <summary>
		/// Generates an open-instance delegate to get the value of the property from a given target
		/// </summary>
		private static Delegate _DelegateForGet<TReturn>(Type delegateType, Type inputType, FieldInfo field)
		{
			Delegate result = GenDelegateForMember<FieldInfo>(
				delegateType,
				field,
				"get_" + field.Name + "(" + field.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.GenFieldGetter,
				typeof(TReturn),
				inputType);
			return result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public static Action<TTarget, TValue> DelegateForSet<TValue>(FieldInfo field)
		{
			return (Action<TTarget, TValue>) _DelegateForSet<TValue>(typeof(Action<TTarget, TValue>), typeof(TTarget), field);
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public static SetterRef<TTarget, TValue> DelegateForSetRef<TValue>(FieldInfo field)
		{
			return (SetterRef<TTarget, TValue>) _DelegateForSet<TValue>(typeof(SetterRef<TTarget, TValue>), typeof(TTarget).MakeByRefType(), field);
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public static Delegate _DelegateForSet<TValue>(Type delegateType, Type inputType, FieldInfo field)
		{
			Delegate result = GenDelegateForMember<FieldInfo>(
				delegateType,
				field,
				"set_" + field.Name + "(" + field.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.GenFieldSetter,
				null,
				inputType,
				typeof(TValue));
			return result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static Invoker<TTarget> DelegateForCall(MethodInfo method)
		{
			return (Invoker<TTarget>) _DelegateForCall(typeof(Invoker<TTarget>), typeof(TTarget), method);
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static InvokerRef<TTarget> DelegateForCallRef(MethodInfo method)
		{
			return (InvokerRef<TTarget>) _DelegateForCall(typeof(InvokerRef<TTarget>), typeof(TTarget).MakeByRefType(), method);
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static Delegate _DelegateForCall(Type delegateType, Type inputType, MethodInfo method)
		{
			Delegate result = GenDelegateForMember<MethodInfo>(
				delegateType,
				method,
				method.Name + "(" + method.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.GenMethodInvocation,
				null,
				inputType,
				typeof(object[]));
			return result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static Invoker<TTarget, TReturn> DelegateForCall<TReturn>(MethodInfo method)
		{
			return (Invoker<TTarget, TReturn>) _DelegateForCall<TReturn>(typeof(Invoker<TTarget, TReturn>), typeof(TTarget), method);
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static InvokerRef<TTarget, TReturn> DelegateForCallRef<TReturn>(MethodInfo method)
		{
			return (InvokerRef<TTarget, TReturn>) _DelegateForCall<TReturn>(typeof(InvokerRef<TTarget, TReturn>), typeof(TTarget).MakeByRefType(), method);
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		private static Delegate _DelegateForCall<TReturn>(Type delegateType, Type inputType, MethodInfo method)
		{
			Delegate result = GenDelegateForMember<MethodInfo>(
				delegateType,
				method,
				method.Name + "(" + method.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.GenMethodInvocation,
				typeof(TReturn),
				inputType,
				typeof(object[]));
			return result;
		}
	}
}