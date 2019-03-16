using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection.Cache
{
	public static class Constructors<TTarget>
	{
		private static IDictionary<ConstructorKey, Ctor<TTarget>> Ctors;
		public static readonly Func<TTarget> New;

		static Constructors()
		{
			if (Reflect.Concurrent) {
				Ctors = new ConcurrentDictionary<ConstructorKey, Ctor<TTarget>>(ConstructorKey.Comparer);
			}
			else {
				Ctors = new Dictionary<ConstructorKey, Ctor<TTarget>>(ConstructorKey.Comparer);
			}
			try {
				New = (Func<TTarget>) ReflectGen<TTarget>.Constructor(typeof(TTarget));
			}
			catch { }
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
				ParamTypes = paramTypes,
			};
			if (!Ctors.TryGetValue(key, out Ctor<TTarget> result)) {
				result = (Ctor<TTarget>) ReflectGen<TTarget>.Constructor(key.Type, key.ParamTypes);
				Ctors[key] = result;
			}
			return result;
		}
	}
}