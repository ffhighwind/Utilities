using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Reflection
{
	internal struct PropertyKey : IEqualityComparer<PropertyKey>
	{
		public PropertyKey(Type type, PropertyInfo property)
		{
			Type = type;
			Property = property;
		}

		public Type Type;
		public PropertyInfo Property;

		public static IEqualityComparer<PropertyKey> Comparer = new PropertyKey();

		public bool Equals(PropertyKey x, PropertyKey y)
		{
			return x.Type.Equals(y.Type) && x.Property.Equals(y.Property);
		}

		public int GetHashCode(PropertyKey obj)
		{
			return 23 * obj.GetHashCode() ^ obj.GetHashCode();
		}
	}
}
