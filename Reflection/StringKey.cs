using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
namespace Utilities.Reflection
{
	internal struct StringKey : IEqualityComparer<StringKey>
	{
		public StringKey(Type type, string name)
		{
			Type = type;
			Name = name;
		}

		public Type Type;
		public string Name;

		public static IEqualityComparer<StringKey> Comparer = new StringKey();

		public bool Equals(StringKey x, StringKey y)
		{
			return x.Type.Equals(y.Type) && x.Name.Equals(y.Name);
		}

		public int GetHashCode(StringKey obj)
		{
			return 23 * Type.GetHashCode() ^ Name.GetHashCode();
		}
	}
}
*/