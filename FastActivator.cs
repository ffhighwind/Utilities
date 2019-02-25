using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
	public static class FastActivator<T> where T : new()
	{
		/// <summary>
		/// Fast default constructor to be used with the new() constraint. Much faster than calling new T() because that calls 
		/// the slow reflection based method <see cref="System.Activator.CreateInstance(Type)"/>.
		/// </summary>
		/// <see cref="https://blogs.msdn.microsoft.com/seteplia/2017/02/01/dissecting-the-new-constraint-in-c-a-perfect-example-of-a-leaky-abstraction/"/>
		public static readonly Func<T> Create =	DynamicModuleLambdaCompiler.GenerateFactory<T>();
	}

	/// <summary>
	/// Internally used by FastActivator.
	/// </summary>
	internal static class DynamicModuleLambdaCompiler
	{
		public static Func<T> GenerateFactory<T>() where T : new()
		{
			Expression<Func<T>> expr = () => new T();
			NewExpression newExpr = (NewExpression) expr.Body;

			var method = new DynamicMethod(
				name: "lambda",
				returnType: newExpr.Type,
				parameterTypes: new Type[0],
				m: typeof(DynamicModuleLambdaCompiler).Module,
				skipVisibility: true);

			ILGenerator ilGen = method.GetILGenerator();
			// Constructor for value types could be null
			if (newExpr.Constructor != null) {
				ilGen.Emit(OpCodes.Newobj, newExpr.Constructor);
			}
			else {
				LocalBuilder temp = ilGen.DeclareLocal(newExpr.Type);
				ilGen.Emit(OpCodes.Ldloca, temp);
				ilGen.Emit(OpCodes.Initobj, newExpr.Type);
				ilGen.Emit(OpCodes.Ldloc, temp);
			}

			ilGen.Emit(OpCodes.Ret);

			return (Func<T>) method.CreateDelegate(typeof(Func<T>));
		}
	}
}
