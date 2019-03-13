﻿using System;
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
namespace Utilities.Reflection
{
	public delegate TTarget Ctor<TTarget>(params object[] param);
	public delegate TReturn Invoker<TTarget, TReturn>(TTarget target, params object[] param);
	public delegate void Invoker<TTarget>(TTarget target, params object[] param);

	/// <summary>
	/// A dynamic reflection extensions library that emits IL to set/get fields/properties, call methods and invoke constructors
	/// Once the delegate is created, it can be stored and reused resulting in much faster access times than using regular reflection
	/// The results are cached. Once a delegate is generated, any subsequent call to generate the same delegate on the same field/property/method will return the previously generated delegate
	/// Note: Since this generates IL, it won't work on AOT platforms such as iOS an Android. But is useful and works very well in editor codes and standalone targets
	/// <see cref="https://github.com/vexe/Fast.Reflection/blob/master/FastReflection.cs"/>
	/// </summary>
	public static class ReflectGen<TTarget>
	{
		private const string ctorName = "ctor.";
		private const string methodName = "method.";
		private const string fieldSetterName = "fset.";
		private const string fieldGetterName = "fget.";
		private const string propertySetterName = "pset.";
		private const string propertyGetterName = "pget.";

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
			ConstructorKey key = new ConstructorKey()
			{
				Output = typeof(TTarget),
				Type = type,
				ParamTypes = paramTypes,
			};
			DynamicMethod dynMethod = new DynamicMethod(ctorName + type.Name + "_" + paramTypes.Length, type, inputParamTypes, true);
			ILGenerator emit = dynMethod.GetILGenerator();
			ReflectEmit<TTarget>.GenCtor(emit, type, paramTypes);
			Delegate result = dynMethod.CreateDelegate(delegateType);
			return result;
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		public static Func<TTarget, TReturn> DelegateForGet<TReturn>(PropertyInfo property)
		{
			if (!property.CanRead) {
				throw new InvalidOperationException("Property is not readable: " + property.Name);
			}
			Delegate result = GenDelegateForMember<PropertyInfo>(
				typeof(Func<TTarget, TReturn>),
				property,
				propertyGetterName + property.DeclaringType.Name + "." + property.Name,
				ReflectEmit<TTarget>.GenPropertyGetter,
				typeof(TReturn),
				property.DeclaringType);
			return (Func<TTarget, TReturn>) result;
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		public static Func<TTarget, TValue> DelegateForSet<TValue>(PropertyInfo property)
		{
			if (!property.CanWrite) {
				throw new InvalidOperationException("Property is not writable " + property.Name);
			}
			Delegate result = GenDelegateForMember<PropertyInfo>(
				typeof(Func<TTarget, TValue>),
				property,
				propertySetterName + property.DeclaringType.Name + "." + property.Name,
				ReflectEmit<TTarget>.GenPropertySetter,
				typeof(void),
				property.DeclaringType.MakeByRefType(),
				typeof(TValue));
			return (Func<TTarget, TValue>) result;
		}

		/// <summary>
		/// Generates an open-instance delegate to get the value of the property from a given target
		/// </summary>
		public static Func<TTarget, TReturn> DelegateForGet<TReturn>(FieldInfo field)
		{
			Delegate result = GenDelegateForMember<FieldInfo>(
				typeof(Func<TTarget, TReturn>),
				field,
				fieldGetterName + field.DeclaringType.Name + "." + field.Name,
				ReflectEmit<TTarget>.GenFieldGetter,
				typeof(TReturn),
				field.DeclaringType);
			return (Func<TTarget, TReturn>) result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public static Func<TTarget, TValue> DelegateForSet<TValue>(FieldInfo field)
		{
			Delegate result = GenDelegateForMember<FieldInfo>(
				typeof(Func<TTarget, TValue>),
				field,
				fieldSetterName + field.DeclaringType.Name + "." + field.Name,
				ReflectEmit<TTarget>.GenFieldSetter,
				typeof(void),
				field.DeclaringType.MakeByRefType(),
				typeof(TValue));
			return (Func<TTarget, TValue>) result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static Invoker<TTarget> DelegateForCall(MethodInfo method)
		{
			Delegate result = GenDelegateForMember<MethodInfo>(
				typeof(Invoker<TTarget>),
				method,
				methodName + method.DeclaringType.Name + "." + method.Name,
				ReflectEmit<TTarget>.GenMethodInvocation,
				typeof(void),
				method.DeclaringType,
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
				methodName + method.DeclaringType.Name + "." + method.Name,
				ReflectEmit<TTarget>.GenMethodInvocation,
				typeof(TReturn),
				method.DeclaringType,
				typeof(object[]));
			return (Invoker<TTarget, TReturn>) result;
		}
	}
}