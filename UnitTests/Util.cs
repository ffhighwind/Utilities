using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class UtilTests
    {
        [TestMethod]
        public void Test()
        {
        }

        [TestMethod]
        public void Parse()
        {
            if (!Util.Parse("500").Equals(500))
                throw new Exception();
            if (Util.Parse("5001").Equals(5000))
                throw new Exception();
            if (!Util.Parse("500.0").Equals(500.0))
                throw new Exception();
            if (!Util.Parse("-500.00001").Equals(-500.00001))
                throw new Exception();
            if (Util.Parse("-500.00001").Equals(400.00001))
                throw new Exception();

            if (!Util.Parse("-0string blah").Equals("-0string blah"))
                throw new Exception();
            if (!Util.Parse("-0string blah").Equals("-0string blah"))
                throw new Exception();

            if (!Util.Parse("6/5/2018").Equals(new DateTime(2018, 6, 5)))
                throw new Exception();
            if (!Util.Parse("6/5/2018 6:33:15 PM").Equals(new DateTime(2018, 6, 5, 6 + 12, 33, 15)))
                throw new Exception();
            if (Util.Parse("6:12:14:45.3448").GetType() != typeof(TimeSpan))
                throw new Exception();

            if (Util.Parse("Null") != null)
                throw new Exception();
            if (!Util.Parse("FALSE").Equals(false))
                throw new Exception();
            if (!Util.Parse("TRue").Equals(true))
                throw new Exception();
        }
    }
}
