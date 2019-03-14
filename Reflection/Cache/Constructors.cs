using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection.Cache
{
	internal static class Constructors
	{
		public static Type[] objectArrayParam = new Type[] { typeof(object[]) };
	}

	public static class Constructors<TTarget>
	{
		private static IDictionary<ConstructorKey, Ctor<TTarget>> Ctors;
		public static readonly Func<TTarget> New = (Func<TTarget>) ReflectGen<TTarget>.DelegateForCtor(typeof(Func<TTarget>), typeof(TTarget), Array.Empty<Type>(), Array.Empty<Type>());

		static Constructors()
		{
			if (Reflect.Concurrent) {
				Ctors = new ConcurrentDictionary<ConstructorKey, Ctor<TTarget>>(ConstructorKey.Comparer);
			}
			else {
				Ctors = new Dictionary<ConstructorKey, Ctor<TTarget>>(ConstructorKey.Comparer);
			}
		}

		public static void SetConcurrent(bool concurrent = true)
		{
			if (concurrent) {
				if (Ctors is Dictionary<ConstructorKey, Ctor<TTarget>>) {
					Ctors = new ConcurrentDictionary<ConstructorKey, Ctor<TTarget>>(Ctors, ConstructorKey.Comparer);
				}
			}
			else if (Ctors is ConcurrentDictionary<ConstructorKey, Ctor<TTarget>>) {
				Ctors = new Dictionary<ConstructorKey, Ctor<TTarget>>(Ctors, ConstructorKey.Comparer);
			}
		}

		public static void ClearCache(bool resize = false)
		{
			if (resize) {
				if (Ctors is Dictionary<ConstructorKey, Ctor<TTarget>>) {
					Ctors = new Dictionary<ConstructorKey, Ctor<TTarget>>(ConstructorKey.Comparer);
				}
				else {
					Ctors = new ConcurrentDictionary<ConstructorKey, Ctor<TTarget>>(ConstructorKey.Comparer);
				}
			}
			else {
				Ctors.Clear();
			}
		}

		public static Ctor<TTarget> Create(params Type[] paramTypes)
		{
			return Create(typeof(TTarget), paramTypes);
		}

		public static Ctor<TTarget> Create(Type type, params Type[] paramTypes)
		{
			ConstructorKey key = new ConstructorKey()
			{
				Output = typeof(TTarget),
				Type = type,
				ParamTypes = paramTypes
			};
			if (!Ctors.TryGetValue(key, out Ctor<TTarget> result)) {
				result = (Ctor<TTarget>) ReflectGen<TTarget>.DelegateForCtor(typeof(Ctor<TTarget>), typeof(TTarget), Constructors.objectArrayParam, paramTypes);
				Ctors[key] = result;
			}
			return result;
		}
	}
}