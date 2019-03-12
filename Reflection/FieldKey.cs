using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection
{
	internal struct FieldKey : IEqualityComparer<FieldKey>
	{
		public FieldKey(Type type, FieldInfo field)
		{
			Type = type;
			Field = field;
		}

		public Type Type;
		public FieldInfo Field;

		public static IEqualityComparer<FieldKey> Comparer = new FieldKey();

		public bool Equals(FieldKey x, FieldKey y)
		{
			return x.Type.Equals(y.Type) && x.Field.Equals(y.Field);
		}

		public int GetHashCode(FieldKey obj)
		{
			return 23 * Type.GetHashCode() ^ Field.GetHashCode();
		}
	}
}
