using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MatchUpdateAttribute : Attribute
	{
		public MatchUpdateAttribute(Func<string> function)
		{
			ValueGenerator = () =>
			{
				string value = function();
				DapperExtensions.ValidateSqlValue(value);
				return "(" + value + ")";
			};
		}

		public MatchUpdateAttribute(string value = null)
		{
			value = value?.Trim();
			if (value == null || value.Length == 0) {
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
