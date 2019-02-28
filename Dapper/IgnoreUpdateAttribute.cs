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
		public IgnoreUpdateAttribute(Func<string> function)
		{
			ValueGenerator = () =>
			{
				string value = function();
				DapperExtensions.ValidateSqlValue(value);
				return "(" + value + ")";
			};
		}

		public IgnoreUpdateAttribute(string value = null)
		{
			value = value?.Trim();
			if(value == null || value.Length == 0) {
				ValueGenerator = () => null;
				return;
			}
			DapperExtensions.ValidateSqlValue(value);
			value = "(" + value + ")";
			ValueGenerator = () => value;
		}

		private readonly Func<string> ValueGenerator;
		public string Value => ValueGenerator();
	}
}
