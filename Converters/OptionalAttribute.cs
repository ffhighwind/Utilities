using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Converters
{
	/// <summary>
	/// Determines if the property is optional when converting between types.
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Property)]
	public class OptionalAttribute : Attribute
	{
	}
}
