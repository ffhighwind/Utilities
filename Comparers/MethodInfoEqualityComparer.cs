using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Comparers
{
	/// <summary>
	/// https://ayende.com/blog/2658/method-equality
	/// </summary>
	public class MethodInfoEqualityComparer : IEqualityComparer<MethodInfo>
	{
		public static readonly MethodInfoEqualityComparer Default = new MethodInfoEqualityComparer();
		private MethodInfoEqualityComparer() { }

		public bool Equals(MethodInfo x, MethodInfo y)
		{
			if (x.Equals(y))
				return true;
			// GetHashCode calls to RuntimeMethodHandle.StripMethodInstantiation()
			// which is needed to fix issues with method equality from generic types.
			if (x.GetHashCode() != y.GetHashCode() || x.Name != y.Name || x.DeclaringType != y.DeclaringType || x.ReturnType != y.ReturnType)
				return false;
			ParameterInfo[] leftParams = x.GetParameters();
			ParameterInfo[] rightParams = y.GetParameters();
			if (leftParams.Length != rightParams.Length)
				return false;
			for (int i = 0; i < leftParams.Length; i++) {
				if (leftParams[i].ParameterType != rightParams[i].ParameterType)
					return false;
			}
			return true;
		}

		public int GetHashCode(MethodInfo obj)
		{
			return obj.GetHashCode();
		}
	}
}
