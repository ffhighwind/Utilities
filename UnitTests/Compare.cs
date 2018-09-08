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
            object x = 4;
            object y = 5.0m;
            object z = "6";

            if (Utilities.Comparers.Compare.To(x, y) != -1)
                throw new Exception();
            if (Utilities.Comparers.Compare.To(x, z) != -1)
                throw new Exception();
            if (Utilities.Comparers.Compare.To(y, z) != -1)
                throw new Exception();
            if (Utilities.Comparers.Compare.To(y, y) != 0)
                throw new Exception();

            if (Utilities.Comparers.Compare.To(y, x) != 1)
                throw new Exception();
            if (Utilities.Comparers.Compare.To(z, x) != 1)
                throw new Exception();
            if (Utilities.Comparers.Compare.To(z, y) != 1)
                throw new Exception();
        }
    }
}
