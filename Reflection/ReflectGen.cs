using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Utilities.Reflection.Cache;

/// <summary>
/// A modified implementation of Vexe's FastReflection.
/// Original authors: Vexe and GeorgeR
/// <see cref="https://github.com/vexe/Fast.Reflection/blob/master/FastReflection.cs"/>
/// </summary>
namespace Utilities.Reflection
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
		private static Delegate GenDelegate<TMember>(Type delegateType, TMember member, string dynMethodName,
			Action<ILGenerator, Type, TMember> generator, Type returnType, params Type[] paramTypes)
			where TMember : MemberInfo
		{
			DynamicMethod dynMethod = new DynamicMethod(dynMethodName, returnType, paramTypes, true);
			ILGenerator emit = dynMethod.GetILGenerator();
			generator(emit, paramTypes.Length == 0 ? typeof(TTarget) : paramTypes[0], member);
			Delegate result = dynMethod.CreateDelegate(delegateType);
			return result;
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public static Func<TTarget> Constructor(Type type)
		{
			return (Func<TTarget>) Constructor(typeof(Func<TTarget>), type, Array.Empty<Type>(), Array.Empty<Type>());
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public static Ctor<TTarget> Constructor(Type type, params Type[] paramTypes)
		{
			return (Ctor<TTarget>) Constructor(typeof(Ctor<TTarget>), type, ReflectGen.objectArrayParam, paramTypes);
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public static Ctor<TTarget> Constructor(params Type[] paramTypes)
		{
			return (Ctor<TTarget>) Constructor(typeof(Ctor<TTarget>), typeof(TTarget), ReflectGen.objectArrayParam, paramTypes);
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public static Delegate Constructor(Type delegateType, Type inputType, Type[] inputParamTypes, params Type[] paramTypes)
		{
			string methodName = inputType.Name + "(" + string.Join(",", paramTypes.Select(p => p.Name)) + ")";
			DynamicMethod dynMethod = new DynamicMethod(methodName, inputType, inputParamTypes, true);
			ILGenerator emit = dynMethod.GetILGenerator();
			ReflectEmit<TTarget>.EmitConstructor(emit, inputType, paramTypes);
			Delegate result = dynMethod.CreateDelegate(delegateType);
			return result;
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		public static Func<TTarget, TReturn> Getter<TReturn>(PropertyInfo property)
		{
			return (Func<TTarget, TReturn>) Getter(property, typeof(Func<TTarget, TReturn>), typeof(TReturn), typeof(TTarget));
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		public static GetterRef<TTarget, TReturn> GetterRef<TReturn>(PropertyInfo property)
		{
			return (GetterRef<TTarget, TReturn>) Getter(property, typeof(GetterRef<TTarget, TReturn>), typeof(TReturn), typeof(TTarget).MakeByRefType());
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		public static Delegate Getter(PropertyInfo property, Type delegateType, Type returnType, Type inputType)
		{
			if (!property.CanRead) {
				throw new InvalidOperationException("Property is not readable: " + property.Name);
			}
			Delegate result = GenDelegate<PropertyInfo>(
				delegateType,
				property,
				"get_" + property.Name + "(" + property.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.EmitGetter,
				returnType,
				inputType);
			return result;
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		public static Action<TTarget, TValue> Setter<TValue>(PropertyInfo property)
		{
			return (Action<TTarget, TValue>) Setter(property, typeof(Action<TTarget, TValue>), typeof(TTarget), typeof(TValue));
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		public static SetterRef<TTarget, TValue> SetterRef<TValue>(PropertyInfo property)
		{
			return (SetterRef<TTarget, TValue>) Setter(property, typeof(SetterRef<TTarget, TValue>), typeof(TTarget), typeof(TValue));
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		public static Delegate Setter(PropertyInfo property, Type delegateType, Type inputType, Type valueType)
		{
			if (!property.CanWrite) {
				throw new InvalidOperationException("Property is not writable " + property.Name);
			}
			Delegate result = GenDelegate<PropertyInfo>(
				delegateType,
				property,
				"set_" + property.Name + "(" + property.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.EmitSetter,
				null,
				inputType,
				valueType);
			return result;
		}

		/// <summary>
		/// Generates an open-instance delegate to get the value of the property from a given target
		/// </summary>
		public static Func<TTarget, TReturn> Getter<TReturn>(FieldInfo field)
		{
			return (Func<TTarget, TReturn>) Getter(field, typeof(Func<TTarget, TReturn>), typeof(TReturn), typeof(TTarget));
		}

		/// <summary>
		/// Generates an open-instance delegate to get the value of the property from a given target
		/// </summary>
		public static GetterRef<TTarget, TReturn> GetterRef<TReturn>(FieldInfo field)
		{
			return (GetterRef<TTarget, TReturn>) Getter(field, typeof(GetterRef<TTarget, TReturn>), typeof(TReturn), typeof(TTarget).MakeByRefType());
		}

		/// <summary>
		/// Generates an open-instance delegate to get the value of the property from a given target
		/// </summary>
		public static Delegate Getter(FieldInfo field, Type delegateType, Type returnType, Type inputType)
		{
			Delegate result = GenDelegate<FieldInfo>(
				delegateType,
				field,
				"get_" + field.Name + "(" + field.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.EmitGetter,
				returnType,
				inputType);
			return result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public static Action<TTarget, TValue> Setter<TValue>(FieldInfo field)
		{
			return (Action<TTarget, TValue>) Setter(field, typeof(Action<TTarget, TValue>), typeof(TTarget), typeof(TValue));
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public static SetterRef<TTarget, TValue> SetterRef<TValue>(FieldInfo field)
		{
			return (SetterRef<TTarget, TValue>) Setter(field, typeof(SetterRef<TTarget, TValue>), typeof(TTarget).MakeByRefType(), typeof(TValue));
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public static Delegate Setter(FieldInfo field, Type delegateType, Type inputType, Type valueType)
		{
			Delegate result = GenDelegate<FieldInfo>(
				delegateType,
				field,
				"set_" + field.Name + "(" + field.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.EmitSetter,
				null,
				inputType,
				valueType);
			return result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static Invoker<TTarget> Method(MethodInfo method)
		{
			return (Invoker<TTarget>) Method(method, typeof(Invoker<TTarget>), null, typeof(TTarget));
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static InvokerRef<TTarget> MethodRef(MethodInfo method)
		{
			return (InvokerRef<TTarget>) Method(method, typeof(InvokerRef<TTarget>), null, typeof(TTarget).MakeByRefType(), typeof(object[]));
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static Invoker<TTarget, TReturn> Method<TReturn>(MethodInfo method)
		{
			return (Invoker<TTarget, TReturn>) Method(method, typeof(Invoker<TTarget, TReturn>), typeof(TReturn), typeof(TTarget), typeof(object[]));
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static InvokerRef<TTarget, TReturn> MethodRef<TReturn>(MethodInfo method)
		{
			return (InvokerRef<TTarget, TReturn>) Method(method, typeof(InvokerRef<TTarget, TReturn>), typeof(TReturn), typeof(TTarget).MakeByRefType(), typeof(object[]));
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public static Delegate Method(MethodInfo method, Type delegateType, Type returnType, Type inputType, params Type[] paramTypes)
		{
			Delegate result = GenDelegate<MethodInfo>(
				delegateType,
				method,
				method.Name + "(" + method.DeclaringType.Name + ")",
				ReflectEmit<TTarget>.EmitMethod,
				returnType,
				CombineTypes(inputType, paramTypes));
			return result;
		}

		private static Type[] CombineTypes(Type inputType, Type[] paramTypes)
		{
			Type[] inputParams = new Type[1 + paramTypes.Length];
			inputParams[0] = inputType;
			for (int i = 0; i < paramTypes.Length; i++) {
				inputParams[i + 1] = paramTypes[i];
			}
			return inputParams;
		}
	}
}