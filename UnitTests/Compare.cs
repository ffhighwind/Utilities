using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Utilities.UnitTests
{
    [TestClass]
    public class Compare
    {
        [TestMethod]
        public void Test()
        {
            object a = 4;
            object b = 5.0m;
            object c = "6";

            if (Utilities.Comparers.Comparers.Compare(a, b) != -1)
                throw new Exception();
            if (Utilities.Comparers.Comparers.Compare(a, c) != -1)
                throw new Exception();
            if (Utilities.Comparers.Comparers.Compare(b, c) != -1)
                throw new Exception();
            if (Utilities.Comparers.Comparers.Compare(b, b) != 0)
                throw new Exception();

            if (Utilities.Comparers.Comparers.Compare(b, a) != 1)
                throw new Exception();
            if (Utilities.Comparers.Comparers.Compare(c, a) != 1)
                throw new Exception();
            if (Utilities.Comparers.Comparers.Compare(c, b) != 1)
                throw new Exception();
        }
    }
}
