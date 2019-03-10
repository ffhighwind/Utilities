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
	//public delegate void MemberSetter<TTarget, TValue>(ref TTarget target, TValue value);
	//public delegate TReturn MemberGetter<TTarget, TReturn>(TTarget target);
	//public delegate TReturn MethodCaller<TTarget, TReturn>(TTarget target, object[] args);
	//public delegate T CtorInvoker<T>(object[] parameters);

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
		private IDictionary<MethodInfo, Delegate> Methods;
		private IDictionary<FieldInfo, Delegate> Getters;
		private IDictionary<FieldInfo, Delegate> Setters;

		private const string kCtorInvokerName = "ctor.";
		private const string kMethodCallerName = "method.";
		private const string kFieldSetterName = "fset.";
		private const string kFieldGetterName = "fget.";
		private const string kPropertySetterName = "pset.";
		private const string kPropertyGetterName = "pget.";

		public static bool Concurrent = false;
		public static readonly Func<TTarget> New;

		static ReflectCache()
		{
			Type targetType = typeof(TTarget);
			TypeCode typeCode = Type.GetTypeCode(targetType);
			if (targetType != typeof(object) && typeCode == TypeCode.Object) {
				New = new ReflectCache<TTarget>().DelegateForCtor(targetType);
			}
		}

		private struct ConstructorKey : IEqualityComparer<ConstructorKey>
		{
			public Type type;
			public Type[] paramTypes;

			public bool Equals(ConstructorKey x, ConstructorKey y)
			{
				if (x.type.Equals(y.type)) {
					for (int i = 0; i < paramTypes.Length; i++) {
						if (!x.paramTypes[i].Equals(y.paramTypes[i])) {
							return false;
						}
					}
				}
				return true;
			}

			public int GetHashCode(ConstructorKey obj)
			{
				int hashCode = 23 * obj.type.GetHashCode();
				for (int i = 0; i < obj.paramTypes.Length; i++) {
					hashCode ^= obj.paramTypes[i].GetHashCode();
				}
				return hashCode;
			}
		}

		internal ReflectCache()
		{
			Clear(true);
		}

		public void SetConcurrent()
		{
			if (!(Constructors is ConcurrentDictionary<ConstructorKey, Delegate>)) {
				Constructors = new ConcurrentDictionary<ConstructorKey, Delegate>(Constructors);
				Methods = new ConcurrentDictionary<MethodInfo, Delegate>(Methods, MethodInfoEqualityComparer.Default);
				Getters = new ConcurrentDictionary<FieldInfo, Delegate>(Getters);
				Setters = new ConcurrentDictionary<FieldInfo, Delegate>(Setters);
			}
		}

		public void Clear(bool resize = false)
		{
			if (!resize) {
				Constructors.Clear();
				Methods.Clear();
				Getters.Clear();
				Setters.Clear();
				if (New != null) {
					ConstructorKey key = new ConstructorKey()
					{
						type = typeof(TTarget),
						paramTypes = Array.Empty<Type>()
					};
					Constructors.Add(key, New);
				}
			}
			else {
				if (Concurrent) {
					Constructors = new ConcurrentDictionary<ConstructorKey, Delegate>();
					Methods = new ConcurrentDictionary<MethodInfo, Delegate>(MethodInfoEqualityComparer.Default);
					Getters = new ConcurrentDictionary<FieldInfo, Delegate>();
					Setters = new ConcurrentDictionary<FieldInfo, Delegate>();
				}
				else {
					Constructors = new Dictionary<ConstructorKey, Delegate>();
					Methods = new Dictionary<MethodInfo, Delegate>(MethodInfoEqualityComparer.Default);
					Getters = new Dictionary<FieldInfo, Delegate>();
					Setters = new Dictionary<FieldInfo, Delegate>();
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
		public Func<TTarget> DelegateForCtor(Type type, params Type[] paramTypes)
		{
			ConstructorKey key = new ConstructorKey()
			{
				type = type,
				paramTypes = paramTypes,
			};
			if (Constructors.TryGetValue(key, out Delegate result)) {
				return (Func<TTarget>) result;
			}
			DynamicMethod dynMethod = new DynamicMethod(kCtorInvokerName + type.Name + "_" + paramTypes.Length, type, new Type[] { typeof(object[]) }, true);
			ILGenerator emit = dynMethod.GetILGenerator();
			Emit.GenCtor(emit, type, paramTypes);
			result = dynMethod.CreateDelegate(typeof(Func<TTarget>));
			Constructors[key] = result;
			return (Func<TTarget>) result;
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
			if (!Methods.TryGetValue(property.GetGetMethod(), out result)) {
				result = GenDelegateForMember<PropertyInfo>(
					typeof(Func<TTarget, TReturn>),
					property,
					kPropertyGetterName + property.DeclaringType.Name + "." + property.Name,
					Emit.GenPropertyGetter,
					typeof(TReturn),
					property.DeclaringType);
				MethodInfo method = property.GetGetMethod();
				Methods[method] = result;
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
			if (!Methods.TryGetValue(property.GetSetMethod(), out result)) {
				result = GenDelegateForMember<PropertyInfo>(
					typeof(Func<TTarget, TValue>),
					property,
					kPropertySetterName + property.DeclaringType.Name + "." + property.Name,
					Emit.GenPropertySetter,
					typeof(void),
					property.DeclaringType.MakeByRefType(),
					typeof(TValue));
				MethodInfo setter = property.GetSetMethod();
				Methods[setter] = result;
			}
			return (Func<TTarget, TValue>) result;
		}

		/// <summary>
		/// Generates an open-instance delegate to get the value of the property from a given target
		/// </summary>
		public Func<TTarget, TReturn> DelegateForGet<TReturn>(FieldInfo field)
		{
			Delegate result;
			if (!Getters.TryGetValue(field, out result)) {
				result = GenDelegateForMember<FieldInfo>(
					typeof(Func<TTarget, TReturn>),
					field,
					kFieldGetterName + field.DeclaringType.Name + "." + field.Name,
					Emit.GenFieldGetter,
					typeof(TReturn),
					field.DeclaringType);
				Getters[field] = result;
			}
			return (Func<TTarget, TReturn>) result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public Func<TTarget, TValue> DelegateForSet<TValue>(FieldInfo field)
		{
			if (!Setters.TryGetValue(field, out Delegate result)) {
				result = GenDelegateForMember<FieldInfo>(
					typeof(Func<TTarget, TValue>),
					field,
					kFieldSetterName + field.DeclaringType.Name + "." + field.Name,
					Emit.GenFieldSetter,
					typeof(void),
					field.DeclaringType.MakeByRefType(),
					typeof(TValue));
				Setters[field] = result;
			}
			return (Func<TTarget, TValue>) result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public Delegate DelegateForCall(MethodInfo method)
		{
			Delegate result;
			if (!Methods.TryGetValue(method, out result)) {
				result = GenDelegateForMember<MethodInfo>(
					typeof(Action<TTarget, object[]>),
					method,
					kMethodCallerName + method.DeclaringType.Name + "." + method.Name,
					Emit.GenMethodInvocation,
					typeof(void),
					method.DeclaringType,
					typeof(object[]));
				Methods[method] = result;
			}
			return result;
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public Delegate DelegateForCall<TReturn>(MethodInfo method)
		{
			Delegate result;
			if (!Methods.TryGetValue(method, out result)) {
				result = GenDelegateForMember<MethodInfo>(
					typeof(Func<TTarget, object[], TReturn>),
					method,
					kMethodCallerName + method.DeclaringType.Name + "." + method.Name,
					Emit.GenMethodInvocation,
					typeof(TReturn),
					method.DeclaringType,
					typeof(object[]));
				Methods[method] = result;
			}
			return result;
		}
	}
}