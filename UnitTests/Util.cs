﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Utilities.UnitTests
{
	[TestClass]
	public class UtilTests
	{
		[TestMethod]
		public void Parse()
		{
			if ((long)Util.Parse("500") != 500)
				throw new Exception();
			if ((long)Util.Parse("5001") == 5000)
				throw new Exception();
			if (!Util.Parse("500.0").Equals(500.0m))
				throw new Exception();
			if (!Util.Parse("-500.00001").Equals(-500.00001m))
				throw new Exception();
			if (Util.Parse("-500.00001").Equals(400.00001))
				throw new Exception();
			if (!Util.Parse("-0string blah").Equals("-0string blah"))
				throw new Exception();
			if (!Util.Parse("6/5/2018").Equals(new DateTime(2018, 6, 5)))
				throw new Exception();
			if (!Util.Parse("6/5/2018 6:33:15 PM").Equals(new DateTime(2018, 6, 5, 6 + 12, 33, 15)))
				throw new Exception();
			//if (Util.Parse("6:12:14:45.3448") is TimeSpan)
			//    throw new Exception();

			if (Util.Parse("Null") != null)
				throw new Exception();
			if (!Util.Parse("FALSE").Equals(false))
				throw new Exception();
			if (!Util.Parse("TRue").Equals(true))
				throw new Exception();
			ulong x = (ulong)Utilities.Converters.Converters.ChangeType(long.MaxValue, typeof(ulong));
		}
	}
}
