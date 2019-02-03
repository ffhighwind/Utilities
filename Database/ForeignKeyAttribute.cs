using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Database
{
	public class ForeignKeyAttribute<T> where T : class
	{
		public ForeignKeyAttribute(string propertyName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
		{
			Property = typeof(T).GetProperty(propertyName, flags);
		}

		public PropertyInfo Property { get; private set; }
	}
}
