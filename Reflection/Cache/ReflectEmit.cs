using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Utilities.Reflection.Cache
{
	/// <summary>
	/// A modified implementation of Vexe's FastReflection.
	/// Original authors: Vexe and GeorgeR
	/// <see cref="https://github.com/vexe/Fast.Reflection/blob/master/FastReflection.cs"/>
	/// </summary>
	public static class ReflectEmit<TTarget>
	{
		/// <summary>
		/// Generates a assembly called 'name' that's useful for debugging purposes and inspecting the resulting C# code in ILSpy
		/// If 'field' is not null, it generates a setter and getter for that field
		/// If 'property' is not null, it generates a setter and getter for that property
		/// If 'method' is not null, it generates a call for that method
		/// if 'targetType' and 'ctorParamTypes' are not null, it generates a constructor for the target type that takes the specified arguments
		/// </summary>
		public static void EmitDebugAssembly(ILGenerator emit, string name, FieldInfo field, PropertyInfo property, MethodInfo method, Type targetType, Type[] ctorParamTypes)
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
				EmitSetter(emit, field);
				emit = buildMethod("FieldGetter", fieldType, new Type[] { typeof(TTarget) });
				EmitGetter(emit, field);
			}

			if (property != null) {
				Type propType = weakTyping ? typeof(object) : property.PropertyType;
				emit = buildMethod("PropertySetter", typeof(void), new Type[] { typeof(TTarget).MakeByRefType(), propType });
				EmitSetter(emit, property);
				emit = buildMethod("PropertyGetter", propType, new Type[] { typeof(TTarget) });
				EmitGetter(emit, property);
			}

			if (method != null) {
				Type returnType = (weakTyping || method.ReturnType == typeof(void)) ? typeof(object) : method.ReturnType;
				emit = buildMethod("MethodCaller", returnType, new Type[] { typeof(TTarget), typeof(object[]) });
				EmitMethod(emit, method);
			}

			if (targetType != null) {
				emit = buildMethod("Ctor", typeof(TTarget), new Type[] { typeof(object[]) });
				EmitConstructor(emit, targetType, ctorParamTypes);
			}

			typeBuilder.CreateType();
			asmBuilder.Save(name);
		}

		public static void EmitConstructor(ILGenerator emit, Type type, Type[] paramTypes)
		{
			// arg0: object[] arguments
			// goal: return new T(arguments)
			Type targetType = typeof(TTarget) == typeof(object) ? type : typeof(TTarget);

			if (targetType.IsValueType && paramTypes.Length == 0) {
				LocalBuilder tmp = emit.DeclareLocal(targetType);
				emit.Emit(OpCodes.Ldloca, tmp);
				emit.Emit(OpCodes.Initobj, targetType);
				emit.Emit(OpCodes.Ldloc, tmp);
				if (typeof(TTarget) == typeof(object) && targetType.IsValueType) {
					emit.Emit(OpCodes.Box, targetType);
				}
				emit.Emit(OpCodes.Ret);
			}
			else {
				ConstructorInfo ctor = targetType.GetConstructor(paramTypes);
				if (ctor == null) {
					throw new Exception("Generating constructor for type: " + targetType +
						(paramTypes.Length == 0 ? "No empty constructor found!" :
						"No constructor found that matches the following parameter types: " +
						string.Join(",", paramTypes.Select(x => x.Name).ToArray())));
				}
				_EmitConstructor(emit, type, targetType, ctor, paramTypes);
			}
		}

		public static void EmitConstructor(ILGenerator emit, Type type, ConstructorInfo constructor)
		{
			Type targetType = typeof(TTarget) == typeof(object) ? type : typeof(TTarget);
			_EmitConstructor(emit, type, targetType, constructor, constructor.GetParameters().Select(p => p.ParameterType).ToList());
		}

		private static void _EmitConstructor(ILGenerator emit, Type type, Type targetType, ConstructorInfo constructor, IList<Type> paramTypes)
		{
			// push parameters in order to then call ctor
			for (int i = 0, imax = paramTypes.Count; i < imax; i++) {
				emit.Emit(OpCodes.Ldarg_0);     // push args array
				emit.Emit(OpCodes.Ldc_I4, i);   // push index
				emit.Emit(OpCodes.Ldelem_Ref);  // push array[index]
				emit.Emit(OpCodes.Unbox_Any, paramTypes[i]); // cast
			}
			emit.Emit(OpCodes.Newobj, constructor);

			if (typeof(TTarget) == typeof(object) && targetType.IsValueType) {
				emit.Emit(OpCodes.Box, targetType);
			}
			emit.Emit(OpCodes.Ret);
		}

		public static void EmitMethod(ILGenerator emit, MethodInfo method)
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

		public static void EmitGetter(ILGenerator emit, FieldInfo field)
		{
			_EmitGetter(emit, field, field.FieldType, field.IsStatic,
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
				}
			);
		}

		public static void EmitGetter(ILGenerator emit, PropertyInfo property)
		{
			_EmitGetter(emit, property, property.PropertyType,
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

		private static void _EmitGetter(ILGenerator emit, MemberInfo member, Type memberType, bool isStatic, Action<ILGenerator, MemberInfo> get)
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

		public static void EmitSetter(ILGenerator emit, FieldInfo field)
		{
			EmitSetter(emit, field, field.FieldType, field.IsStatic,
				(e, f) =>
				{
					if (f.IsStatic) {
						e.Emit(OpCodes.Stsfld, f);
					}
					else {
						e.Emit(OpCodes.Stfld, f);
					}
				}
			);
		}

		public static void EmitSetter(ILGenerator emit, PropertyInfo property)
		{
			EmitSetter(emit, property, property.PropertyType,
				property.GetSetMethod(true).IsStatic, (e, p) =>
				{
					MethodInfo m = p.GetSetMethod(true);
					if (m.IsVirtual)
						emit.Emit(OpCodes.Callvirt, m);
					else
						emit.Emit(OpCodes.Call, m);
				}
			);
		}

		private static void EmitSetter<MInfo>(ILGenerator emit, MInfo member, Type memberType, bool isStatic, Action<ILGenerator, MInfo> set)
			where MInfo : MemberInfo
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
				//if (!targetType.IsValueType)
				//	emit.Emit(OpCodes.Ldind_Ref);
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
