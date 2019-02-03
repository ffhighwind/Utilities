using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Database
{
	public class TableAttribute : Attribute
	{
		public TableAttribute(string name, bool cached = false)
		{
			Name = name;
		}

		public string Name { get; private set; }
		public bool IsCached { get; private set; }
	}
}
