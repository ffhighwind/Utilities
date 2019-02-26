using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension;

namespace Dapper
{
	[AttributeUsage(AttributeTargets.Property)]
	public class IgnoreUpdateAttribute : Attribute
	{
		public IgnoreUpdateAttribute(string value = null)
		{
			DapperExtensions.ValidateSqlValue(value);
			Value = string.IsNullOrWhiteSpace(value) ? null : ("(" + value.Trim() + ")");
		}

		public string Value { get; private set; }
	}
}
