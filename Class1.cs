using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    class Class1
    {
        public static void Main()
        {
            object o1 = Utilities.Util.Parse("0.01");
            object o2 = Utilities.Util.Parse("02");
            object o3 = Utilities.Util.Parse("2018-9-1");
            object o4 = Utilities.Util.Parse("21:00:21.033");
            object o5 = Utilities.Util.Parse("21:00:21.033 AM");
            Type t1 = o1.GetType();
            Type t2 = o2.GetType();
            Type t3 = o3.GetType();
            Type t4 = o4.GetType();
            Type t5 = o5.GetType();
        }
    }
}
