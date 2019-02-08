using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper
{
	[AttributeUsage(AttributeTargets.Property)]
	public class KeyAttribute : Attribute
	{
		/// <summary>
		/// A Property with a given key, or composite key if multiple KeyAttributes exist.
		/// </summary>
		/// <param name="required">Determines if the key is auto-generated. 
		/// False is equivilent to [Key][IgnoreInsert][IgnoreUpdate] while true is just [Key].</param>
		public KeyAttribute(bool required = false)
		{
			Required = required;
		}

		public bool Required { get; private set; }
	}
}
