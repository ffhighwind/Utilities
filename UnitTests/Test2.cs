using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Utilities.UnitTests
{
	[Table("Test2")]
	public class Test2
	{
		public int Col1 { get; set; }
		public string Col2 { get; set; }
		public float Col3 { get; set; }
	}
}
