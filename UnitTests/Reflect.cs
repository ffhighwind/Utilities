using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
		public void TestClass()
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

		[TestMethod]
		public void TestStruct()
		{
			// Constructors
			Func<Test16Struct> test16ctor1 = Reflect.Constructor<Test16Struct>();
			Ctor<Test16Struct> test16ctor2 = Reflect.Constructor<Test16Struct>(new Type[0]);
			Ctor<Test16Struct> test16ctor3 = Reflect.Constructor<Test16Struct>(typeof(int));
			if (test16ctor1 == null)
				throw new InvalidOperationException();
			if (test16ctor2 == null)
				throw new InvalidOperationException();
			if (test16ctor3 == null)
				throw new InvalidOperationException();

			Test16Struct test16 = test16ctor1();
			Test16Struct test16_2 = test16ctor2();
			Test16Struct test16_3 = test16ctor3(1);

			if (!test16.Equals(test16_2))
				throw new InvalidOperationException();
			if (test16.Equals(test16_3))
				throw new InvalidOperationException();

			// Setters
			PropertyInfo prop = typeof(Test16Struct).GetProperty("Segment");
			GetterRef<Test16Struct, string> segmentGet = Reflect.GetterRef<Test16Struct, string>(prop);
			SetterRef<Test16Struct, string> segmentSet = Reflect.SetterRef<Test16Struct, string>(prop);
			segmentSet(ref test16, "abc");
			if (segmentGet(ref test16) != "abc")
				throw new InvalidOperationException();

			// Field Get/Set
			FieldInfo field = typeof(Test16Struct).GetField("Field");
			GetterRef<Test16Struct, int> fieldGet = Reflect.GetterRef<Test16Struct, int>(field);
			SetterRef<Test16Struct, int> fieldSet = Reflect.SetterRef<Test16Struct, int>(field);
			fieldSet(ref test16, 39);
			if (fieldGet(ref test16) != 39)
				throw new InvalidOperationException();

			// Methods
			MethodInfo method = typeof(Test16Struct).GetMethod("Equals", new Type[] { typeof(object) });
			InvokerRef<Test16Struct, bool> eqObj = Reflect.MethodRef<Test16Struct, bool>(method);
			MethodInfo method2 = typeof(Test16Struct).GetMethod("Equals", new Type[] { typeof(Test16Struct) });
			InvokerRef<Test16Struct, bool> eqTest16 = Reflect.MethodRef<Test16Struct, bool>(method2);
			InvokerRef<Test16Struct, int> getHashCode = Reflect.MethodRef<Test16Struct, int>("GetHashCode");
			InvokerRef<Test16Struct> test = Reflect.MethodRef<Test16Struct>("Test");

			if (!eqObj(ref test16, test16_2))
				throw new InvalidOperationException();
			if (!eqTest16(ref test16, test16_2))
				throw new InvalidOperationException();
			if (eqTest16(ref test16, test16_3))
				throw new InvalidOperationException();
			test16.Year = 1;
			int hashCode1 = getHashCode(ref test16);
			int hashCode3 = getHashCode(ref test16_3);
			if (hashCode1 != hashCode3)
				throw new InvalidOperationException();
			//test(test16);
		}


		[TestMethod]
		public void TestPerformance()
		{
			Test2 t2 = new Test2();
			t2.Col1 = 1;
			t2.Col2 = "a";

			Action testGet1 = () =>
			{
				int x = t2.Col1;
				int y = t2.Col1;
			};

			Action testGet2 = () =>
			{
				string x = t2.Col2;
				string y = t2.Col2;
			};

			Action testSet1 = () =>
			{
				t2.Col1 = 5;
				t2.Col1 = 10;
			};

			Action testSet2 = () =>
			{
				t2.Col2 = "a";
				t2.Col2 = "b";
			};

			PropertyInfo pinfo1 = typeof(Test2).GetProperty("Col1");
			PropertyInfo pinfo2 = typeof(Test2).GetProperty("Col2");
			//PropertyInfo pinfo3 = typeof(Test2).GetProperty("Col3");

			Action testPGet1 = () =>
			{
				int x = (int) pinfo1.GetValue(t2);
				//int y = (int) pinfo1.GetValue(t2);
			};

			Action testPGet2 = () =>
			{
				string x = (string) pinfo2.GetValue(t2);
				//string y = (string) pinfo2.GetValue(t2);
			};

			Action testPSet1 = () =>
			{
				pinfo1.SetValue(t2, 5);
				//pinfo1.SetValue(t2, 10);
			};

			Action testPSet2 = () =>
			{
				pinfo2.SetValue(t2, "a");
				//pinfo2.SetValue(t2, "b");
			};


			MethodInfo get1 = pinfo1.GetGetMethod();
			MethodInfo get2 = pinfo2.GetGetMethod();
			//MethodInfo get3 = pinfo1.GetGetMethod();

			MethodInfo set1 = pinfo1.GetSetMethod();
			MethodInfo set2 = pinfo2.GetSetMethod();
			//MethodInfo set3 = pinfo1.GetSetMethod();

			Action testMGet1 = () =>
			{
				int x = (int) get1.Invoke(t2, Array.Empty<object>());
				//int y = (int) get1.Invoke(t2, Array.Empty<object>());
			};

			Action testMGet2 = () =>
			{
				string x = (string) get2.Invoke(t2, Array.Empty<object>());
				//string y = (string) get2.Invoke(t2, Array.Empty<object>());
			};

			Action testMSet1 = () =>
			{
				set1.Invoke(t2, new object[] { 5 });
				//set1.Invoke(t2, new object[] { 10 });
			};

			Action testMSet2 = () =>
			{
				set2.Invoke(t2, new object[] { "a" });
				//set2.Invoke(t2, new object[] { "b" });
			};


			Func<Test2, int> get1_mine = Utilities.Reflection.Reflect.Getter<Test2, int>(pinfo1);
			Action<Test2, int> set1_mine = Utilities.Reflection.Reflect.Setter<Test2, int>(pinfo1);
			Func<Test2, string> get2_mine = Utilities.Reflection.Reflect.Getter<Test2, string>(pinfo2);
			Action<Test2, string> set2_mine = Utilities.Reflection.Reflect.Setter<Test2, string>(pinfo2);

			Action testGet1m = () =>
			{
				int x = get1_mine(t2);
				//int y = get1_mine(t2);
			};

			Action testGet2m = () =>
			{
				string x = get2_mine(t2);
				//string y = get2_mine(t2);
			};

			Action testSet1m = () =>
			{
				set1_mine(t2, 5);
				//set1_mine(t2, 10);
			};

			Action testSet2m = () =>
			{
				set2_mine(t2, "a");
				//set2_mine(t2, "b");
			};

			List<double> list = new List<double>();
			Console.WriteLine("Direct | IL | MInfo | PInfo");
			list.Add(Profile("Mine_Get_Col1", 10000000, testGet1));
			list.Add(Profile("Mine_Get_Col1", 10000000, testGet1m));
			list.Add(Profile("Pinfo_Get_Col1", 10000000, testPGet1));
			list.Add(Profile("Minfo_Get_Col1", 10000000, testMGet1));
			Print("Get_Col1", list);
			list.Clear();

			list.Add(Profile("Mine_Get_Col2", 10000000, testGet2));
			list.Add(Profile("Mine_Get_Col2", 10000000, testGet2m));
			list.Add(Profile("Pinfo_Get_Col2", 10000000, testPGet2));
			list.Add(Profile("Minfo_Get_Col2", 10000000, testMGet2));
			Print("Get_Col2", list);
			list.Clear();

			list.Add(Profile("Mine_Set_Col1", 10000000, testSet1));
			list.Add(Profile("Mine_Set_Col1", 10000000, testSet1m));
			list.Add(Profile("Pinfo_Set_Col1", 10000000, testPSet1));
			list.Add(Profile("Minfo_Set_Col1", 10000000, testMSet1));
			Print("Set_Col1", list);
			list.Clear();

			list.Add(Profile("Mine_Set_Col2", 10000000, testSet2));
			list.Add(Profile("Mine_Set_Col2", 10000000, testSet2m));
			list.Add(Profile("Pinfo_Set_Col2", 10000000, testPSet2));
			list.Add(Profile("Minfo_Set_Col2", 10000000, testMSet2));
			Print("Set_Col2", list);
			list.Clear();
		}

		private static void Print(string description, List<double> values)
		{
			Console.WriteLine(description + " " + string.Join(" | ", values.Select(v => v.ToString("0."))));
		}

		/// <summary>
		/// https://stackoverflow.com/questions/1047218/benchmarking-small-code-samples-in-c-can-this-implementation-be-improved
		/// </summary>
		static double Profile(string description, int iterations, Action func)
		{
			//Run at highest priority to minimize fluctuations caused by other processes/threads
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			// warm up 
			func();

			var watch = new Stopwatch();

			// clean up
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			watch.Start();
			for (int i = 0; i < iterations; i++) {
				func();
			}
			watch.Stop();
			//Console.Write(description);
			//Console.WriteLine(" Time Elapsed {0} ms", watch.Elapsed.TotalMilliseconds);
			return watch.Elapsed.TotalMilliseconds;
		}
	}
}