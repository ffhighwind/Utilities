using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ForeignKeyAttribute : Attribute
	{
		public ForeignKeyAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; private set; }
	}
}
