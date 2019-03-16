using System;
using System.Collections.Generic;

namespace Utilities.UnitTests
{
	public struct Test16Struct : IEquatable<Test16Struct>
	{
		public Test16Struct(int year)
		{
			Field = 0;
			Year = year;
			Segment = null;
		}

		public int Field;
		public int Year { get; set; }
		public string Segment { get; set; }

		public override bool Equals(object obj)
		{
			return obj is Test16Struct other && Equals(other);
		}

		public void Test() { }

		public bool Equals(Test16Struct other)
		{
			return Field == other.Field &&
				   Year == other.Year &&
				   Segment == other.Segment;
		}

		public override int GetHashCode()
		{
			int hashCode = 378801229;
			hashCode = hashCode * -1521134295 + Field.GetHashCode();
			hashCode = hashCode * -1521134295 + Year.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Segment);
			return hashCode;
		}
	}
}
