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
	public partial class ReflectCache<TTarget>
	{
		private IDictionary<ConstructorKey, Delegate> Constructors;
		private IDictionary<MethodKey, Delegate> Methods;
		private IDictionary<FieldKey, Delegate> Getters;
		private IDictionary<FieldKey, Delegate> Setters;

		private const string kCtorInvokerName = "ctor.";
		private const string kMethodCallerName = "method.";
		private const string kFieldSetterName = "fset.";
		private const string kFieldGetterName = "fget.";
		private const string kPropertySetterName = "pset.";
		private const string kPropertyGetterName = "pget.";

		public static readonly Func<TTarget> New;

		static ReflectCache()
		{
			Type targetType = typeof(TTarget);
			TypeCode typeCode = Type.GetTypeCode(targetType);
			if (targetType != typeof(object) && typeCode == TypeCode.Object) {
				New = (Func<TTarget>) new ReflectCache<TTarget>().DelegateForCtor(typeof(Func<TTarget>), targetType, Array.Empty<Type>(), Array.Empty<Type>());
			}
		}

		internal ReflectCache()
		{
			Clear(true);
		}

		public void SetConcurrent()
		{
			if (!(Constructors is ConcurrentDictionary<ConstructorKey, Delegate>)) {
				Constructors = new ConcurrentDictionary<ConstructorKey, Delegate>(Constructors, ConstructorKey.Comparer);
				Methods = new ConcurrentDictionary<MethodKey, Delegate>(Methods, MethodKey.Comparer);
				Getters = new ConcurrentDictionary<FieldKey, Delegate>(Getters, FieldKey.Comparer);
				Setters = new ConcurrentDictionary<FieldKey, Delegate>(Setters, FieldKey.Comparer);
			}
		}

		public void Clear(bool resize = false)
		{
			if (!resize) {
				Constructors.Clear();
				Methods.Clear();
				Getters.Clear();
				Setters.Clear();
			}
			else {
				if (Constructors is ConcurrentDictionary<ConstructorKey, Delegate>) {
					Constructors = new ConcurrentDictionary<ConstructorKey, Delegate>(ConstructorKey.Comparer);
					Methods = new ConcurrentDictionary<MethodKey, Delegate>(MethodKey.Comparer);
					Getters = new ConcurrentDictionary<FieldKey, Delegate>(FieldKey.Comparer);
					Setters = new ConcurrentDictionary<FieldKey, Delegate>(FieldKey.Comparer);
				}
				else {
					Constructors = new Dictionary<ConstructorKey, Delegate>(ConstructorKey.Comparer);
					Methods = new Dictionary<MethodKey, Delegate>(MethodKey.Comparer);
					Getters = new Dictionary<FieldKey, Delegate>(FieldKey.Comparer);
					Setters = new Dictionary<FieldKey, Delegate>(FieldKey.Comparer);
				}
			}
		}

		private Delegate GenDelegateForMember<TMember>(Type delegateType, TMember member, string dynMethodName,
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
		public Delegate DelegateForCtor(Type delegateType, Type type, Type[] inputParamTypes, params Type[] paramTypes)
		{
			ConstructorKey key = new ConstructorKey()
			{
				Output = typeof(TTarget),
				Type = type,
				ParamTypes = paramTypes,
			};
			if (!Constructors.TryGetValue(key, out Delegate result)) {
				DynamicMethod dynMethod = new DynamicMethod(kCtorInvokerName + type.Name + "_" + paramTypes.Length, type, inputParamTypes, true);
				ILGenerator emit = dynMethod.GetILGenerator();
				Emit.GenCtor(emit, type, paramTypes);
				result = dynMethod.CreateDelegate(delegateType);
				Constructors[key] = result;
			}
			return result;
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		public Func<TTarget, TReturn> DelegateForGet<TReturn>(PropertyInfo property)
		{
			if (!property.CanRead) {
				throw new InvalidOperationException("Property is not readable: " + property.Name);
			}
			Delegate result;
			MethodKey key = new MethodKey(typeof(TReturn), property.GetGetMethod());
			if (!Methods.TryGetValue(key, out result)) {
				result = GenDelegateForMember<PropertyInfo>(
					typeof(Func<TTarget, TReturn>),
					property,
					kPropertyGetterName + property.DeclaringType.Name + "." + property.Name,
					Emit.GenPropertyGetter,
					typeof(TReturn),
					property.DeclaringType);
				MethodInfo method = property.GetGetMethod();
				Methods[key] = result;
			}
			return (Func<TTarget, TReturn>) result;
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		public Func<TTarget, TValue> DelegateForSet<TValue>(PropertyInfo property)
		{
			if (!property.CanWrite) {
				throw new InvalidOperationException("Property is not writable " + property.Name);
			}
			Delegate result;
			MethodKey key = new MethodKey(typeof(TValue), property.GetSetMethod());
			if (!Methods.TryGetValue(key, out result)) {
				result = GenDelegateForMember<PropertyInfo>(
					typeof(Func<TTarget, TValue>),
					property,
					kPropertySetterName + property.DeclaringType.Name + "." + property.Name,
					Emit.GenPropertySetter,
					typeof(void),
					property.DeclaringType.MakeByRefType(),
					typeof(TValue));
				MethodInfo setter = property.GetSetMethod();
				Methods[key] = result;
			}
			return (Func<TTarget, TValue>) result;
		}

		/// <summary>
		/// Generates an open-instance delegate to get the value of the property from a given target
		/// </summary>
		public Func<TTarget, TReturn> DelegateForGet<TReturn>(FieldInfo field)
		{
			Delegate result;
			FieldKey key = new FieldKey(typeof(TReturn), field);
			if (!Getters.TryGetValue(key, out result)) {
				result = GenDelegateForMember<FieldInfo>(
					typeof(Func<TTarget, TReturn>),
					field,
					kFieldGetterName + field.DeclaringType.Name + "." + field.Name,
					Emit.GenFieldGetter,
					typeof(TReturn),
					field.DeclaringType);
				Getters[key] = result;
			}
			return (Func<TTarget, TReturn>) result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public Func<TTarget, TValue> DelegateForSet<TValue>(FieldInfo field)
		{
			FieldKey key = new FieldKey(typeof(TValue), field);
			if (!Setters.TryGetValue(key, out Delegate result)) {
				result = GenDelegateForMember<FieldInfo>(
					typeof(Func<TTarget, TValue>),
					field,
					kFieldSetterName + field.DeclaringType.Name + "." + field.Name,
					Emit.GenFieldSetter,
					typeof(void),
					field.DeclaringType.MakeByRefType(),
					typeof(TValue));
				Setters[key] = result;
			}
			return (Func<TTarget, TValue>) result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public Invoker<TTarget> DelegateForCall(MethodInfo method)
		{
			Delegate result;
			MethodKey key = new MethodKey(typeof(TTarget), method);
			if (!Methods.TryGetValue(key, out result)) {
				result = GenDelegateForMember<MethodInfo>(
					typeof(Invoker<TTarget>),
					method,
					kMethodCallerName + method.DeclaringType.Name + "." + method.Name,
					Emit.GenMethodInvocation,
					typeof(void),
					method.DeclaringType,
					typeof(object[]));
				Methods[key] = result;
			}
			return (Invoker<TTarget>) result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public Invoker<TTarget, TReturn> DelegateForCall<TReturn>(MethodInfo method)
		{
			Delegate result;
			MethodKey key = new MethodKey(typeof(TTarget), method);
			if (!Methods.TryGetValue(key, out result)) {
				result = GenDelegateForMember<MethodInfo>(
					typeof(Invoker<TTarget, TReturn>),
					method,
					kMethodCallerName + method.DeclaringType.Name + "." + method.Name,
					Emit.GenMethodInvocation,
					typeof(TReturn),
					method.DeclaringType,
					typeof(object[]));
				Methods[key] = result;
			}
			return (Invoker<TTarget, TReturn>) result;
		}
	}
}