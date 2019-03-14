using System;
using System.Collections.Generic;

namespace Utilities.UnitTests
{
	public class Test16 : IEquatable<Test16>
	{
		public Test16() { }
		public Test16(int year)
		{
			Year = year;
		}

		public int Field;
		public int Year { get; set; }
		public string Segment { get; set; }
		public string Country { get; set; }
		public string Product { get; set; }
		public string DiscountBand { get; set; }
		public decimal UnitsSold { get; set; }
		public string ManufacturingPrice { get; set; }
		public string SalePrice { get; set; }
		public string GrossSales { get; set; }
		public string Discounts { get; set; }
		public string Sales { get; set; }
		public string COGS { get; set; }
		public string Profit { get; set; }
		public DateTime Date { get; set; }
		public int MonthNumber { get; set; }
		public string MonthName { get; set; }

		public override bool Equals(object obj)
		{
			return Equals(obj as Test16);
		}

		public void Test() { }

		public bool Equals(Test16 other)
		{
			return other != null &&
				   Year == other.Year &&
				   Segment == other.Segment &&
				   Country == other.Country &&
				   Product == other.Product &&
				   DiscountBand == other.DiscountBand &&
				   UnitsSold == other.UnitsSold &&
				   ManufacturingPrice == other.ManufacturingPrice &&
				   SalePrice == other.SalePrice &&
				   GrossSales == other.GrossSales &&
				   Discounts == other.Discounts &&
				   Sales == other.Sales &&
				   COGS == other.COGS &&
				   Profit == other.Profit &&
				   Date == other.Date &&
				   MonthNumber == other.MonthNumber &&
				   MonthName == other.MonthName;
		}

		public override int GetHashCode()
		{
			int hashCode = 378801229;
			hashCode = hashCode * -1521134295 + Year.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Segment);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Country);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Product);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DiscountBand);
			hashCode = hashCode * -1521134295 + UnitsSold.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ManufacturingPrice);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SalePrice);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GrossSales);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Discounts);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Sales);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(COGS);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Profit);
			hashCode = hashCode * -1521134295 + Date.GetHashCode();
			hashCode = hashCode * -1521134295 + MonthNumber.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MonthName);
			return hashCode;
		}
	}
}
