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
	public delegate void MemberSetter<TTarget, TValue>(ref TTarget target, TValue value);
	public delegate TReturn MemberGetter<TTarget, TReturn>(TTarget target);
	public delegate TReturn MethodCaller<TTarget, TReturn>(TTarget target, object[] args);
	public delegate T CtorInvoker<T>(object[] parameters);

	/// <summary>
	/// A dynamic reflection extensions library that emits IL to set/get fields/properties, call methods and invoke constructors
	/// Once the delegate is created, it can be stored and reused resulting in much faster access times than using regular reflection
	/// The results are cached. Once a delegate is generated, any subsequent call to generate the same delegate on the same field/property/method will return the previously generated delegate
	/// Note: Since this generates IL, it won't work on AOT platforms such as iOS an Android. But is useful and works very well in editor codes and standalone targets
	/// <see cref="https://github.com/vexe/Fast.Reflection/blob/master/FastReflection.cs"/>
	/// </summary>
	public class ReflectCache
	{
		private ILGenerator emit;
		private IDictionary<ConstructorKey, Delegate> Constructors;
		private IDictionary<MethodInfo, Delegate> Methods;
		private IDictionary<FieldInfo, Delegate> Getters;
		private IDictionary<FieldInfo, Delegate> Setters;

		private const string kCtorInvokerName = "C.";
		private const string kMethodCallerName = "M.";
		private const string kFieldSetterName = "FS.";
		private const string kFieldGetterName = "FG.";
		private const string kPropertySetterName = "PS.";
		private const string kPropertyGetterName = "PG.";

		public static bool Concurrent = false;

		private class ConstructorKey
		{
			public Type type;
			public Type[] paramTypes;
		}

		internal ReflectCache()
		{
			if (Concurrent) {
				Constructors = new Dictionary<ConstructorKey, Delegate>();
				Methods = new Dictionary<MethodInfo, Delegate>();
				Getters = new Dictionary<FieldInfo, Delegate>();
				Setters = new Dictionary<FieldInfo, Delegate>();
			}
			else {
				Constructors = new ConcurrentDictionary<ConstructorKey, Delegate>();
				Methods = new ConcurrentDictionary<MethodInfo, Delegate>();
				Getters = new ConcurrentDictionary<FieldInfo, Delegate>();
				Setters = new ConcurrentDictionary<FieldInfo, Delegate>();
			}
		}

		public void MakeConcurrent(bool concurrent = true)
		{
			if (concurrent) {
				Constructors = new Dictionary<ConstructorKey, Delegate>();
				Methods = new Dictionary<MethodInfo, Delegate>();
				Getters = new Dictionary<FieldInfo, Delegate>();
				Setters = new Dictionary<FieldInfo, Delegate>();
			}
			else {
				Constructors = new ConcurrentDictionary<ConstructorKey, Delegate>();
				Methods = new ConcurrentDictionary<MethodInfo, Delegate>();
				Getters = new ConcurrentDictionary<FieldInfo, Delegate>();
				Setters = new ConcurrentDictionary<FieldInfo, Delegate>();
			}
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public CtorInvoker<T> DelegateForCtor<T>(Type type, params Type[] paramTypes)
		{
			ConstructorKey key = new ConstructorKey()
			{
				type = type,
				paramTypes = paramTypes,
			};
			Delegate result;
			if (Constructors.TryGetValue(key, out result)) {
				return (CtorInvoker<T>) result;
			}

			DynamicMethod dynMethod = new DynamicMethod(kCtorInvokerName, typeof(T), new Type[] { typeof(object[]) });

			emit = dynMethod.GetILGenerator();
			GenCtor<T>(type, paramTypes);

			result = dynMethod.CreateDelegate(typeof(CtorInvoker<T>));
			Constructors[key] = result;
			return (CtorInvoker<T>) result;
		}

		/// <summary>
		/// Generates or gets a weakly-typed open-instance delegate to the specified type constructor that takes the specified type params
		/// </summary>
		public CtorInvoker<object> DelegateForCtor(Type type, params Type[] ctorParamTypes)
		{
			return DelegateForCtor<object>(type, ctorParamTypes);
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		public MemberGetter<TTarget, TReturn> DelegateForGet<TTarget, TReturn>(PropertyInfo property)
		{
			if (!property.CanRead) {
				throw new InvalidOperationException("Property is not readable " + property.Name);
			}

			int key = GetKey<TTarget, TReturn>(property, kPropertySetterName);
			Delegate result;
			if (Methods.TryGetValue(property.GetGetMethod(), out result)) {
				return (MemberGetter<TTarget, TReturn>) result;
			}

			return GenDelegateForMember<MemberGetter<TTarget, TReturn>, PropertyInfo>(
				property, key, kPropertyGetterName, GenPropertyGetter<TTarget>,
				typeof(TReturn), typeof(TTarget));
		}

		/// <summary>
		/// Generates or gets a weakly-typed open-instance delegate to get the value of the specified property from a given target
		/// </summary>
		public MemberGetter<object, object> DelegateForGet(PropertyInfo property)
		{
			return DelegateForGet<object, object>(property);
		}

		/// <summary>
		/// Generates or gets a strongly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		public MemberSetter<TTarget, TValue> DelegateForSet<TTarget, TValue>(PropertyInfo property)
		{
			if (!property.CanWrite) {
				throw new InvalidOperationException("Property is not writable " + property.Name);
			}

			int key = GetKey<TTarget, TValue>(property, kPropertySetterName);
			Delegate result;
			if (cache.TryGetValue(key, out result)) {
				return (MemberSetter<TTarget, TValue>) result;
			}

			return GenDelegateForMember<MemberSetter<TTarget, TValue>, PropertyInfo>(
				property, key, kPropertySetterName, GenPropertySetter<TTarget>,
				typeof(void), typeof(TTarget).MakeByRefType(), typeof(TValue));
		}

		/// <summary>
		/// Generates or gets a weakly-typed open-instance delegate to set the value of the specified property on a given target
		/// </summary>
		public MemberSetter<object, object> DelegateForSet(PropertyInfo property)
		{
			return DelegateForSet<object, object>(property);
		}

		/// <summary>
		/// Generates an open-instance delegate to get the value of the property from a given target
		/// </summary>
		public MemberGetter<TTarget, TReturn> DelegateForGet<TTarget, TReturn>(FieldInfo field)
		{
			int key = GetKey<TTarget, TReturn>(field, kFieldGetterName);
			Delegate result;
			if (cache.TryGetValue(key, out result)) {
				return (MemberGetter<TTarget, TReturn>) result;
			}

			return GenDelegateForMember<MemberGetter<TTarget, TReturn>, FieldInfo>(
				field, key, kFieldGetterName, GenFieldGetter<TTarget>,
				typeof(TReturn), typeof(TTarget));
		}

		/// <summary>
		/// Generates a weakly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public MemberGetter<object, object> DelegateForGet(FieldInfo field)
		{
			return DelegateForGet<object, object>(field);
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public MemberSetter<TTarget, TValue> DelegateForSet<TTarget, TValue>(FieldInfo field)
		{
			int key = GetKey<TTarget, TValue>(field, kFieldSetterName);
			Delegate result;
			if (cache.TryGetValue(key, out result)) {
				return (MemberSetter<TTarget, TValue>) result;
			}

			return GenDelegateForMember<MemberSetter<TTarget, TValue>, FieldInfo>(
				field, key, kFieldSetterName, GenFieldSetter<TTarget>,
				typeof(void), typeof(TTarget).MakeByRefType(), typeof(TValue));
		}

		/// <summary>
		/// Generates a weakly-typed open-instance delegate to set the value of the field in a given target
		/// </summary>
		public MemberSetter<object, object> DelegateForSet(FieldInfo field)
		{
			return DelegateForSet<object, object>(field);
		}

		/// <summary>
		/// Generates a strongly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public MethodCaller<TTarget, TReturn> DelegateForCall<TTarget, TReturn>(MethodInfo method)
		{
			int key = GetKey<TTarget, TReturn>(method, kMethodCallerName);
			Delegate result;
			if (cache.TryGetValue(key, out result)) {
				return (MethodCaller<TTarget, TReturn>) result;
			}

			return GenDelegateForMember<MethodCaller<TTarget, TReturn>, MethodInfo>(
				method, key, kMethodCallerName, GenMethodInvocation<TTarget>,
				typeof(TReturn), typeof(TTarget), typeof(object[]));
		}

		/// <summary>
		/// Generates a weakly-typed open-instance delegate to invoke the specified method
		/// </summary>
		public MethodCaller<object, object> DelegateForCall(MethodInfo method)
		{
			return DelegateForCall<object, object>(method);
		}

		/*
		/// <summary>
		/// Executes the delegate on the specified target and arguments but only if it's not null
		/// </summary>
		public static void SafeInvoke<TTarget, TValue>(MethodCaller<TTarget, TValue> caller, TTarget target, params object[] args)
		{
			if (caller != null) {
				caller(target, args);
			}
		}

		/// <summary>
		/// Executes the delegate on the specified target and value but only if it's not null
		/// </summary>
		public static void SafeInvoke<TTarget, TValue>(MemberSetter<TTarget, TValue> setter, ref TTarget target, TValue value)
		{
			if (setter != null) {
				setter(ref target, value);
			}
		}

		/// <summary>
		/// Executes the delegate on the specified target only if it's not null, returns default(TReturn) otherwise
		/// </summary>
		public static TReturn SafeInvoke<TTarget, TReturn>(MemberGetter<TTarget, TReturn> getter, TTarget target)
		{
			if (getter != null) {
				return getter(target);
			}
			return default(TReturn);
		}
		*/

		/// <summary>
		/// Generates a assembly called 'name' that's useful for debugging purposes and inspecting the resulting C# code in ILSpy
		/// If 'field' is not null, it generates a setter and getter for that field
		/// If 'property' is not null, it generates a setter and getter for that property
		/// If 'method' is not null, it generates a call for that method
		/// if 'targetType' and 'ctorParamTypes' are not null, it generates a constructor for the target type that takes the specified arguments
		/// </summary>
		public void GenDebugAssembly(string name, FieldInfo field, PropertyInfo property, MethodInfo method, Type targetType, Type[] ctorParamTypes)
		{
			GenDebugAssembly<object>(name, field, property, method, targetType, ctorParamTypes);
		}

		/// <summary>
		/// Generates a assembly called 'name' that's useful for debugging purposes and inspecting the resulting C# code in ILSpy
		/// If 'field' is not null, it generates a setter and getter for that field
		/// If 'property' is not null, it generates a setter and getter for that property
		/// If 'method' is not null, it generates a call for that method
		/// if 'targetType' and 'ctorParamTypes' are not null, it generates a constructor for the target type that takes the specified arguments
		/// </summary>
		public void GenDebugAssembly<TTarget>(string name, FieldInfo field, PropertyInfo property, MethodInfo method, Type targetType, Type[] ctorParamTypes)
		{
			AssemblyName asmName = new AssemblyName("Asm");
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule("Mod", name);
			TypeBuilder typeBuilder = modBuilder.DefineType("Test", TypeAttributes.Public);

			bool weakTyping = typeof(TTarget) == typeof(object);

			Func<string, Type, Type[], ILGenerator> buildMethod = (methodName, returnType, parameterTypes) =>
			{
				MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
					MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
					CallingConventions.Standard,
					returnType, parameterTypes);
				return methodBuilder.GetILGenerator();
			};

			if (field != null) {
				Type fieldType = weakTyping ? typeof(object) : field.FieldType;
				emit = buildMethod("FieldSetter", typeof(void), new Type[] { typeof(TTarget).MakeByRefType(), fieldType });
				GenFieldSetter<TTarget>(field);
				emit = buildMethod("FieldGetter", fieldType, new Type[] { typeof(TTarget) });
				GenFieldGetter<TTarget>(field);
			}

			if (property != null) {
				Type propType = weakTyping ? typeof(object) : property.PropertyType;
				emit = buildMethod("PropertySetter", typeof(void), new Type[] { typeof(TTarget).MakeByRefType(), propType });
				GenPropertySetter<TTarget>(property);
				emit = buildMethod("PropertyGetter", propType, new Type[] { typeof(TTarget) });
				GenPropertyGetter<TTarget>(property);
			}

			if (method != null) {
				Type returnType = (weakTyping || method.ReturnType == typeof(void)) ? typeof(object) : method.ReturnType;
				emit = buildMethod("MethodCaller", returnType, new Type[] { typeof(TTarget), typeof(object[]) });
				GenMethodInvocation<TTarget>(method);
			}

			if (targetType != null) {
				emit = buildMethod("Ctor", typeof(TTarget), new Type[] { typeof(object[]) });
				GenCtor<TTarget>(targetType, ctorParamTypes);
			}

			typeBuilder.CreateType();
			asmBuilder.Save(name);
		}

		private static int GetKey<T, R>(MemberInfo member, string dynMethodName)
		{
			return member.GetHashCode() ^ dynMethodName.GetHashCode() ^ typeof(T).GetHashCode() ^ typeof(R).GetHashCode();
		}

		private TDelegate GenDelegateForMember<TDelegate, TMember>(TMember member, int key, string dynMethodName,
			Action<TMember> generator, Type returnType, params Type[] paramTypes)
			where TMember : MemberInfo
			where TDelegate : class
		{
			DynamicMethod dynMethod = new DynamicMethod(dynMethodName, returnType, paramTypes, true);

			emit = dynMethod.GetILGenerator();
			generator(member);

			Delegate result = dynMethod.CreateDelegate(typeof(TDelegate));
			cache[key] = result;
			return (TDelegate) (object) result;
		}

		private void GenCtor<T>(Type type, Type[] paramTypes)
		{
			// arg0: object[] arguments
			// goal: return new T(arguments)
			Type targetType = typeof(T) == typeof(object) ? type : typeof(T);

			if (targetType.IsValueType && paramTypes.Length == 0) {
				LocalBuilder tmp = emit.DeclareLocal(targetType);
				emit.Emit(OpCodes.Ldloca, tmp);
				emit.Emit(OpCodes.Initobj, targetType);
				emit.Emit(OpCodes.Ldloc, tmp);
			}
			else {
				ConstructorInfo ctor = targetType.GetConstructor(paramTypes);
				if (ctor == null) {
					throw new Exception("Generating constructor for type: " + targetType +
						(paramTypes.Length == 0 ? "No empty constructor found!" :
						"No constructor found that matches the following parameter types: " +
						string.Join(",", paramTypes.Select(x => x.Name).ToArray())));
				}

				// push parameters in order to then call ctor
				for (int i = 0, imax = paramTypes.Length; i < imax; i++) {
					emit.Emit(OpCodes.Ldarg_0);     // push args array
					emit.Emit(OpCodes.Ldc_I4, i);   // push index
					emit.Emit(OpCodes.Ldelem_Ref);  // push array[index]
					emit.Emit(OpCodes.Unbox_Any, paramTypes[i]); // cast
				}
				emit.Emit(OpCodes.Newobj, ctor);
			}

			if (typeof(T) == typeof(object) && targetType.IsValueType) {
				emit.Emit(OpCodes.Box, targetType);
			}
			emit.Emit(OpCodes.Ret);
		}

		private void GenMethodInvocation<TTarget>(MethodInfo method)
		{
			bool weaklyTyped = typeof(TTarget) == typeof(object);

			// push target if not static (instance-method. in that case first arg is always 'this')
			if (!method.IsStatic) {
				Type targetType = weaklyTyped ? method.DeclaringType : typeof(TTarget);

				emit.DeclareLocal(targetType);
				emit.Emit(OpCodes.Ldarg_0);
				if (weaklyTyped) {
					emit.Emit(OpCodes.Unbox_Any, targetType);
				}
				emit.Emit(OpCodes.Stloc, 0);
				if (targetType.IsValueType) {
					emit.Emit(OpCodes.Ldloca, 0);
				}
				else {
					emit.Emit(OpCodes.Ldloc, 0);
				}
			}

			// push arguments in order to call method
			ParameterInfo[] prams = method.GetParameters();
			for (int i = 0, imax = prams.Length; i < imax; i++) {
				emit.Emit(OpCodes.Ldarg_1);     // push array
				emit.Emit(OpCodes.Ldc_I4, i);   // push index
				emit.Emit(OpCodes.Ldelem_Ref);  // pop array, index and push array[index]

				ParameterInfo param = prams[i];
				Type dataType = param.ParameterType;

				if (dataType.IsByRef)
					dataType = dataType.GetElementType();

				var tmp = emit.DeclareLocal(dataType);
				emit.Emit(OpCodes.Unbox_Any, dataType);
				emit.Emit(OpCodes.Stloc, tmp);

				if (param.ParameterType.IsByRef)
					emit.Emit(OpCodes.Ldloca, tmp);
				else
					emit.Emit(OpCodes.Ldloc, tmp);
			}

			// perform the correct call (pushes the result)
			if (method.IsVirtual)
				emit.Emit(OpCodes.Callvirt, method);
			else
				emit.Emit(OpCodes.Call, method);

			// if method wasn't static that means we declared a temp local to load the target
			// that means our local variables index for the arguments start from 1
			int localVarStart = method.IsStatic ? 0 : 1;
			for (int i = 0; i < prams.Length; i++) {
				Type paramType = prams[i].ParameterType;
				if (paramType.IsByRef) {
					Type byRefType = paramType.GetElementType();
					emit.Emit(OpCodes.Ldarg_1);
					emit.Emit(OpCodes.Ldc_I4, i);
					emit.Emit(OpCodes.Ldloc, i + localVarStart);
					if (byRefType.IsValueType) {
						emit.Emit(OpCodes.Box, byRefType);
					}
					emit.Emit(OpCodes.Stelem_Ref);
				}
			}
			if (method.ReturnType == typeof(void)) {
				emit.Emit(OpCodes.Ldnull);
			}
			else if (weaklyTyped) {
				if (method.ReturnType.IsValueType)
					emit.Emit(OpCodes.Box, method.ReturnType);
			}
			emit.Emit(OpCodes.Ret);
		}

		private void GenFieldGetter<TTarget>(FieldInfo field)
		{
			GenMemberGetter<TTarget>(field, field.FieldType, field.IsStatic,
				(e, f) =>
				{
					if (field.IsLiteral) {
						if (field.FieldType == typeof(bool)) {
							e.Emit(OpCodes.Ldc_I4_1);
						}
						else if (field.FieldType == typeof(int)) {
							e.Emit(OpCodes.Ldc_I4, (int) field.GetRawConstantValue());
						}
						else if (field.FieldType == typeof(float)) {
							e.Emit(OpCodes.Ldc_R4, (float) field.GetRawConstantValue());
						}
						else if (field.FieldType == typeof(double)) {
							e.Emit(OpCodes.Ldc_R8, (double) field.GetRawConstantValue());
						}
						else if (field.FieldType == typeof(string)) {
							e.Emit(OpCodes.Ldstr, (string) field.GetRawConstantValue());
						}
						else {
							throw new NotSupportedException($"Creating a FieldGetter for type: {field.FieldType.Name} is unsupported.");
						}
					}
					else {
						if (field.IsStatic) {
							e.Emit(OpCodes.Ldsfld, (FieldInfo) f);
						}
						else {
							e.Emit(OpCodes.Ldfld, (FieldInfo) f);
						}
					}
				});
		}

		private void GenPropertyGetter<TTarget>(PropertyInfo property)
		{
			GenMemberGetter<TTarget>(property, property.PropertyType,
				property.GetGetMethod(true).IsStatic,
				(e, p) =>
				{
					MethodInfo m = ((PropertyInfo) p).GetGetMethod(true);
					if (m.IsVirtual)
						emit.Emit(OpCodes.Callvirt, m);
					else
						emit.Emit(OpCodes.Call, m);
				}
			);
		}

		private void GenMemberGetter<TTarget>(MemberInfo member, Type memberType, bool isStatic, Action<ILGenerator, MemberInfo> get)
		{
			if (typeof(TTarget) == typeof(object)) // weakly-typed?
			{
				// if we're static immediately load member and return value
				// otherwise load and cast target, get the member value and box it if neccessary:
				// return ((DeclaringType)target).member;
				if (!isStatic) {
					emit.Emit(OpCodes.Ldarg_0);
					if (member.DeclaringType.IsValueType) {
						emit.Emit(OpCodes.Unbox, member.DeclaringType);
					}
					else {
						emit.Emit(OpCodes.Castclass, member.DeclaringType);
					}
				}
				get(emit, member);
				if (memberType.IsValueType) {
					emit.Emit(OpCodes.Box, memberType);
				}
			}
			else // we're strongly-typed, don't need any casting or boxing
			{
				// if we're static return member value immediately
				// otherwise load target and get member value immeidately
				// return target.member;
				if (!isStatic) {
					if (typeof(TTarget).IsValueType) {
						emit.Emit(OpCodes.Ldarga, 0);
					}
					else {
						emit.Emit(OpCodes.Ldarg, 0);
					}
				}
				get(emit, member);
			}
			emit.Emit(OpCodes.Ret);
		}

		private void GenFieldSetter<TTarget>(FieldInfo field)
		{
			GenMemberSetter<TTarget>(field, field.FieldType, field.IsStatic,
				(e, f) =>
				{
					if (field.IsStatic) {
						e.Emit(OpCodes.Stsfld, field);
					}
					else {
						e.Emit(OpCodes.Stfld, field);
					}
				}
			);
		}

		private void GenPropertySetter<TTarget>(PropertyInfo property)
		{
			GenMemberSetter<TTarget>(property, property.PropertyType,
				property.GetSetMethod(true).IsStatic, (e, p) =>
				{
					MethodInfo m = ((PropertyInfo) p).GetSetMethod(true);
					if (m.IsVirtual)
						emit.Emit(OpCodes.Callvirt, m);
					else
						emit.Emit(OpCodes.Call, m);
				}
			);
		}

		private void GenMemberSetter<TTarget>(MemberInfo member, Type memberType, bool isStatic, Action<ILGenerator, MemberInfo> set)
		{
			Type targetType = typeof(TTarget);
			bool stronglyTyped = targetType != typeof(object);

			// if we're static set member immediately
			if (isStatic) {
				emit.Emit(OpCodes.Ldarg_1);
				if (!stronglyTyped)
					emit.Emit(OpCodes.Unbox_Any, memberType);
				set(emit, member);
				emit.Emit(OpCodes.Ret);
				return;
			}

			if (stronglyTyped) {
				// push target and value argument, set member immediately
				// target.member = value;
				emit.Emit(OpCodes.Ldarg_0);
				if (!targetType.IsValueType)
					emit.Emit(OpCodes.Ldind_Ref);
				emit.Emit(OpCodes.Ldarg_1);
				set(emit, member);
				emit.Emit(OpCodes.Ret);
				return;
			}

			// we're weakly-typed
			targetType = member.DeclaringType;
			if (!targetType.IsValueType) // are we a reference-type?
			{
				// load and cast target, load and cast value and set
				// ((TargetType)target).member = (MemberType)value;
				emit.Emit(OpCodes.Ldarg_0);
				emit.Emit(OpCodes.Ldind_Ref);
				emit.Emit(OpCodes.Castclass, targetType);
				emit.Emit(OpCodes.Ldarg_1);
				emit.Emit(OpCodes.Unbox_Any, memberType);
				set(emit, member);
				emit.Emit(OpCodes.Ret);
				return;
			}

			// we're a value-type
			// handle boxing/unboxing for the user so he doesn't have to do it himself
			// here's what we're basically generating (remember, we're weakly typed, so
			// the target argument is of type object here):
			// TargetType tmp = (TargetType)target; // unbox
			// tmp.member = (MemberField)value;		// set member value
			// target = tmp;						// box back
			emit.DeclareLocal(targetType);
			emit.Emit(OpCodes.Ldarg_0);
			emit.Emit(OpCodes.Ldind_Ref);
			emit.Emit(OpCodes.Unbox_Any, targetType);
			emit.Emit(OpCodes.Stloc_0);
			emit.Emit(OpCodes.Ldloca, 0);
			emit.Emit(OpCodes.Ldarg_1);
			emit.Emit(OpCodes.Unbox_Any, memberType);
			set(emit, member);
			emit.Emit(OpCodes.Ldarg_0);
			emit.Emit(OpCodes.Ldloc_0);
			emit.Emit(OpCodes.Box, targetType);
			emit.Emit(OpCodes.Stind_Ref);
			emit.Emit(OpCodes.Ret);
		}
	}
}