using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection.Cache
{
	public struct MethodKey : IEqualityComparer<MethodKey>
	{
		public MethodKey(Type type, MethodInfo method)
		{
			Type = type;
			Method = method;
		}

		public Type Type;
		public MethodInfo Method;

		public bool Equals(MethodKey x, MethodKey y)
		{
			return x.Type.Equals(y.Type) && MethodInfoEqualityComparer.Default.Equals(x.Method, y.Method);
		}

		public int GetHashCode(MethodKey obj)
		{
			return 23 * obj.Type.GetHashCode() ^ obj.GetHashCode();
		}

		public static IEqualityComparer<MethodKey> Comparer = new MethodKey();
	}
}
