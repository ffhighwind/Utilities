using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;
using Utilities.Reflection;

namespace Utilities.UnitTests
{
	[TestClass]
	public class ReflectTest
	{
		[TestMethod]
		public void Test()
		{
			Func<Test16> test16ctor1 = Reflect.Constructor<Test16>();
			Ctor<Test16> test16ctor2 = Reflect.Constructor<Test16>(new Type[0]);
			Test16 test16 = test16ctor1();
			Test16 test16_2 = test16ctor2();

			MethodInfo method = typeof(Test16).GetMethod("Equals", new Type[] { typeof(object) });
			Invoker<Test16, bool> f = Reflect.Method<Test16, bool>(method);
			MethodInfo method2 = typeof(Test16).GetMethod("Equals", new Type[] { typeof(Test16) });
			Invoker<Test16, bool> f2 = Reflect.Method<Test16, bool>(method2);
			Invoker<Test16, int> f3 = Reflect.Method<Test16, int>("GetHashCode");

			if (test16ctor1.Equals(test16ctor2)) {
				throw new InvalidOperationException();
			}
			if(!test16.Equals(test16_2)) {
				throw new InvalidOperationException();
			}
			if (!f(test16, test16_2)) {
				throw new InvalidOperationException();
			}
			if (!test16.Equals(test16_2)) {
				throw new InvalidOperationException();
			}
			test16.Date = new DateTime(1999, 1, 1);
			if(test16.Equals(test16_2)) {
				throw new InvalidOperationException();
			}
		}
	}
}
