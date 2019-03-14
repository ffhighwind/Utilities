using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection.Cache
{
	public struct ConstructorKey : IEqualityComparer<ConstructorKey>
	{
		public Type Output;
		public Type Type;
		public Type[] ParamTypes;

		public ConstructorKey(Type output, Type type, Type[] paramTypes)
		{
			Output = output;
			Type = type;
			ParamTypes = paramTypes;
		}

		public bool Equals(ConstructorKey x, ConstructorKey y)
		{
			if (x.Type.Equals(y.Type) && x.Output.Equals(y.Output)) {
				for (int i = 0; i < ParamTypes.Length; i++) {
					if (!x.ParamTypes[i].Equals(y.ParamTypes[i])) {
						return false;
					}
				}
			}
			return true;
		}

		public int GetHashCode(ConstructorKey obj)
		{
			int hashCode = 23 * (obj.Type.GetHashCode() + obj.Output.GetHashCode());
			for (int i = 0; i < obj.ParamTypes.Length; i++) {
				hashCode ^= obj.ParamTypes[i].GetHashCode();
			}
			return hashCode;
		}

		public static IEqualityComparer<ConstructorKey> Comparer = new ConstructorKey();
	}
}
