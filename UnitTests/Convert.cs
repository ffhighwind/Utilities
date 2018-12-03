using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Utilities.UnitTests
{
	/// <summary>
	/// Test files from Microsoft's website.
	/// </summary>
	[TestClass]
	public class Convert
	{
		[TestMethod]
		public void ObjectToObject()
		{
			Test16 test16 = new Test16
			{
				COGS = "COGS",
				Country = "Country",
				Date = new DateTime(2018, 5, 2),
				DiscountBand = "DiscountBand",
				Discounts = "Discounts",
				GrossSales = "GrossSales",
				ManufacturingPrice = "ManufacturingPrice",
				MonthName = "MonthName",
				MonthNumber = 5,
				Product = "Product",
				Profit = "Profit",
				SalePrice = "SalePrice",
				Sales = "Sales",
				Segment = "Segment",
				UnitsSold = 2.6m,
				Year = 2017
			};
			Test6 test6 = Converters.Converters.ObjectToObject<Test16, Test6>()(test16);
			Test16S test16s = Converters.Converters.ObjectToObject<Test16, Test16S>()(test16);
			Test16 test16from6 = Converters.Converters.ObjectToObject<Test6, Test16>()(test6);
			Test16 test16from16s = Converters.Converters.ObjectToObject<Test16S, Test16>()(test16s);
			TestEquality(test6, test16);
			TestEquality(test16, test6);
			TestEquality(test16, test16s);
			TestEquality(test6, test16from6);
			TestEquality(test16, test16from16s);
		}

		public void TestEquality(object o1, object o2)
		{
			Type ty1 = o1.GetType();
			Type ty2 = o2.GetType();
			HashSet<string> propNames = new HashSet<string>(ty1.GetProperties().Select(p => p.Name));
			PropertyInfo[] props2 = ty2.GetProperties().Where(p => propNames.Contains(p.Name)).ToArray();
			List<PropertyInfo> props1 = new List<PropertyInfo>();
			foreach (PropertyInfo pinfo2 in props2) {
				PropertyInfo pinfo1 = ty1.GetProperty(pinfo2.Name);
				object val1 = pinfo1.GetValue(o1);
				object val2 = pinfo2.GetValue(o2);
				if (val1 != val2 && val1.ToString() != val2.ToString()) {
					object val1to2 = Converters.Converters.ChangeType(val1, pinfo2.PropertyType);
					object val2to1 = Converters.Converters.ChangeType(val2, pinfo1.PropertyType);
					if (!val1to2.Equals(val2) && !val2to1.Equals(val1)) {
						throw new InvalidOperationException();
					}
				}
			}
		}

		[TestMethod]
		public void ListToObject()
		{
			List<string> names = new List<string>() {
				"Segment",
				"Country",
				"Product",
				"DiscountBand",
				"UnitsSold",
				"ManufacturingPrice",
				"SalePrice",
                //"GrossSales",
                "Discounts",
				"Sales",
				"COGS",
				"Profit",
				"Date",
				"MonthNumber",
				"MonthName",
				"Year",
				"BADDATA1",
                //"GrossSales",
                "BAD DATA 2",
			};
			List<object> values = new List<object>() {
				"Segment",
				"Country",
				"Product",
				"DiscountBand",
				2.6m,
				"ManufacturingPrice",
				"SalePrice",
                //"GrossSales",
                "Discounts",
				"Sales",
				"COGS",
				"Profit",
				new DateTime(2018, 5, 2),
				5,
				"MonthName",
				2017,
				"BADDATA1",
                //"GrossSales",
                "BAD DATA 2"
			};
			List<string> valuesStr = new List<string>() {
				"Segment",
				"Country",
				"Product",
				"DiscountBand",
				"2.6",
				"ManufacturingPrice",
				"SalePrice",
                //"GrossSales",
                "Discounts",
				"Sales",
				"COGS",
				"Profit",
				new DateTime(2018, 5, 2).ToString(),
				"5",
				"MonthName",
				"2017",
				"BADDATA1",
                //"GrossSales",
                "BAD DATA 2",
			};

			Test16 test16 = Converters.Converters.ListToObject<object, Test16>(names)(values);
			Test16 test16B = Converters.Converters.ListToObject<string, Test16>(names)(valuesStr);

			Test16S test16s = Converters.Converters.ListToObject<object, Test16S>(names)(values);
			Test16S test16sB = Converters.Converters.ListToObject<string, Test16S>(names)(valuesStr);

			Test6 test6 = Converters.Converters.ListToObject<object, Test6>(names)(values);
			Test6 test6B = Converters.Converters.ListToObject<string, Test6>(names)(valuesStr);
		}
	}
}
