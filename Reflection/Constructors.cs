using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection
{
	internal static class CtorsPrivate
	{
		public static Type[] objectArrayParam = new Type[] { typeof(object[]) };
	}

	public static class Constructors<TTarget>
	{
		private static IDictionary<ConstructorKey, Ctor<TTarget>> Ctors;
		public static readonly Func<TTarget> New = (Func<TTarget>) ReflectGen<TTarget>.DelegateForCtor(typeof(Func<TTarget>), typeof(TTarget), Array.Empty<Type>(), Array.Empty<Type>());

		static Constructors()
		{
			ClearCache(true);
		}

		public static void MakeConcurrent()
		{
			if (!(Ctors is ConcurrentDictionary<ConstructorKey, Ctor<TTarget>>)) {
				Ctors = new ConcurrentDictionary<ConstructorKey, Ctor<TTarget>>(Ctors, ConstructorKey.Comparer);
			}
		}

		public static void ClearCache(bool resize = false)
		{
			if (resize) {
				if (Ctors is ConcurrentDictionary<ConstructorKey, Ctor<TTarget>>) {
					Ctors = new ConcurrentDictionary<ConstructorKey, Ctor<TTarget>>(Ctors, ConstructorKey.Comparer);
				}
				else {
					Ctors = new Dictionary<ConstructorKey, Ctor<TTarget>>(Ctors, ConstructorKey.Comparer);
				}
			}
			else {
				Ctors.Clear();
			}
		}

		public static Ctor<TTarget> Constructor(params Type[] paramTypes)
		{
			return Constructor(typeof(TTarget), paramTypes);
		}

		public static Ctor<TTarget> Constructor(Type type, params Type[] paramTypes)
		{
			ConstructorKey key = new ConstructorKey()
			{
				Output = typeof(TTarget),
				Type = type,
				ParamTypes = paramTypes
			};
			if(!Ctors.TryGetValue(key, out Ctor<TTarget> result)) {
				result = (Ctor<TTarget>) ReflectGen<TTarget>.DelegateForCtor(typeof(Ctor<TTarget>), typeof(TTarget), CtorsPrivate.objectArrayParam, paramTypes);
				Ctors[key] = result;
			}
			return result;
		}
	}
}
