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
			// Constructors
			Func<Test16> test16ctor1 = Reflect.Constructor<Test16>();
			Ctor<Test16> test16ctor2 = Reflect.Constructor<Test16>(new Type[0]);
			Ctor<Test16> test16ctor3 = Reflect.Constructor<Test16>(typeof(int));
			if (test16ctor1 == null)
				throw new InvalidOperationException();
			if (test16ctor2 == null)
				throw new InvalidOperationException();
			if (test16ctor3 == null)
				throw new InvalidOperationException();

			Test16 test16 = test16ctor1();
			Test16 test16_2 = test16ctor2();
			Test16 test16_3 = test16ctor3(1);

			if (!test16.Equals(test16_2))
				throw new InvalidOperationException();
			if (test16.Equals(test16_3))
				throw new InvalidOperationException();

			// Methods
			MethodInfo method = typeof(Test16).GetMethod("Equals", new Type[] { typeof(object) });
			Invoker<Test16, bool> eqObj = Reflect.Method<Test16, bool>(method);
			MethodInfo method2 = typeof(Test16).GetMethod("Equals", new Type[] { typeof(Test16) });
			Invoker<Test16, bool> eqTest16 = Reflect.Method<Test16, bool>(method2);
			Invoker<Test16, int> getHashCode = Reflect.Method<Test16, int>("GetHashCode");
			Invoker<Test16> test = Reflect.Method<Test16>("Test");

			if (!eqObj(test16, test16_2))
				throw new InvalidOperationException();
			if (!eqTest16(test16, test16_2))
				throw new InvalidOperationException();
			if (eqTest16(test16, test16_3))
				throw new InvalidOperationException();
			test16.Year = 1;
			int hashCode1 = getHashCode(test16);
			int hashCode3 = getHashCode(test16_3);
			if (hashCode1 != hashCode3)
				throw new InvalidOperationException();
			//test(test16);

			// Setters
			PropertyInfo prop = typeof(Test16).GetProperty("Segment");
			Func<Test16, string> segmentGet = Reflect.Getter<Test16, string>(prop);
			Action<Test16, string> segmentSet = Reflect.Setter<Test16, string>(prop);
			segmentSet(test16, "abc");
			if (segmentGet(test16) != "abc")
				throw new InvalidOperationException();

			// Field Get/Set
			FieldInfo field = typeof(Test16).GetField("Field");
			Func<Test16, int> fieldGet = Reflect.Getter<Test16, int>(field);
			Action<Test16, int> fieldSet = Reflect.Setter<Test16, int>(field);
			fieldSet(test16, 39);
			if (fieldGet(test16) != 39)
				throw new InvalidOperationException();
		}
	}
}
