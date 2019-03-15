using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dapper;

namespace Utilities.UnitTests
{
	class Program
	{
		public static void Main()
		{
			Test2 t2 = new Test2();
			t2.Col1 = 1;
			t2.Col2 = "a";
			List<double> list = new List<double>();


			Action testGet1 = () =>
			{
				int x = t2.Col1;
			};

			Action testGet2 = () =>
			{
				string x = t2.Col2;
			};

			Action testSet1 = () =>
			{
				t2.Col1 = 5;
			};

			Action testSet2 = () =>
			{
				t2.Col2 = "a";
			};

			PropertyInfo pinfo1 = typeof(Test2).GetProperty("Col1");
			PropertyInfo pinfo2 = typeof(Test2).GetProperty("Col2");

			Action testPGet1 = () =>
			{
				int x = (int) pinfo1.GetValue(t2);
			};

			Action testPGet2 = () =>
			{
				string x = (string) pinfo2.GetValue(t2);
			};

			Action testPSet1 = () =>
			{
				pinfo1.SetValue(t2, 5);
			};

			Action testPSet2 = () =>
			{
				pinfo2.SetValue(t2, "a");
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
			};

			Action testMGet2 = () =>
			{
				string x = (string) get2.Invoke(t2, Array.Empty<object>());
			};

			Action testMSet1 = () =>
			{
				set1.Invoke(t2, new object[] { 5 });
			};

			Action testMSet2 = () =>
			{
				set2.Invoke(t2, new object[] { "a" });
			};


			Func<Test2, int> pget1_mine = Utilities.Reflection.Reflect.Getter<Test2, int>(pinfo1);
			Action<Test2, int> pset1_mine = Utilities.Reflection.Reflect.Setter<Test2, int>(pinfo1);
			Func<Test2, string> pget2_mine = Utilities.Reflection.Reflect.Getter<Test2, string>(pinfo2);
			Action<Test2, string> pset2_mine = Utilities.Reflection.Reflect.Setter<Test2, string>(pinfo2);

			Action testGet1p = () =>
			{
				int x = pget1_mine(t2);
			};

			Action testGet2p = () =>
			{
				string x = pget2_mine(t2);
			};

			Action testSet1p = () =>
			{
				pset1_mine(t2, 5);
			};

			Action testSet2p = () =>
			{
				pset2_mine(t2, "a");
			};

			FieldInfo finfo1 = typeof(Test2).GetField("Col3");
			FieldInfo finfo2 = typeof(Test2).GetField("Col4");

			Func<Test2, int> fget1_mine = Utilities.Reflection.Reflect.Getter<Test2, int>(finfo1);
			Action<Test2, int> fset1_mine = Utilities.Reflection.Reflect.Setter<Test2, int>(finfo1);
			Func<Test2, string> fget2_mine = Utilities.Reflection.Reflect.Getter<Test2, string>(finfo2);
			Action<Test2, string> fset2_mine = Utilities.Reflection.Reflect.Setter<Test2, string>(finfo2);

			Action testFGet1 = () =>
			{
				int x = (int) finfo1.GetValue(t2);
			};

			Action testFGet2 = () =>
			{
				string x = (string) finfo2.GetValue(t2);
			};

			Action testFSet1 = () =>
			{
				finfo1.SetValue(t2, 5);
			};

			Action testFSet2 = () =>
			{
				finfo2.SetValue(t2, "a");
			};

			// Mine

			Action testGet1f = () =>
			{
				int x = fget1_mine(t2);
			};

			Action testGet2f = () =>
			{
				string x = fget2_mine(t2);
			};

			Action testSet1f = () =>
			{
				fset1_mine(t2, 5);
			};

			Action testSet2f = () =>
			{
				fset2_mine(t2, "a");
			};


			List<string> headers = new List<string>() { "Direct", "FieldIL", "FieldInfo", "PropertyIL", "MethodInfo", "PropertyInfo" };
			Console.WriteLine("            " + string.Join(" | ", headers.Select(v => String.Format("{0,10}", v))));
			list.Add(Profile("Direct_Get_Col1", 10000000, testGet1));
			list.Add(Profile("FMine_Get_Col1", 10000000, testGet1f));
			list.Add(Profile("Finfo_Get_Col1", 10000000, testFGet1));
			list.Add(Profile("PMine_Get_Col1", 10000000, testGet1p));
			list.Add(Profile("Pinfo_Get_Col1", 10000000, testPGet1));
			list.Add(Profile("Minfo_Get_Col1", 10000000, testMGet1));
			Print("Get_Int    ", list);
			list.Clear();

			list.Add(Profile("Direct_Get_Col2", 10000000, testGet2));
			list.Add(Profile("FMine_Get_Col2", 10000000, testGet2f));
			list.Add(Profile("Finfo_Get_Col2", 10000000, testFGet2));
			list.Add(Profile("PMine_Get_Col2", 10000000, testGet2p));
			list.Add(Profile("Pinfo_Get_Col2", 10000000, testPGet2));
			list.Add(Profile("Minfo_Get_Col2", 10000000, testMGet2));
			Print("Get_String ", list);
			list.Clear();

			list.Add(Profile("Direct_Set_Col1", 10000000, testSet1));
			list.Add(Profile("FMine_Get_Col1", 10000000, testSet1f));
			list.Add(Profile("Finfo_Get_Col1", 10000000, testFSet1));
			list.Add(Profile("PMine_Set_Col1", 10000000, testSet1p));
			list.Add(Profile("Pinfo_Set_Col1", 10000000, testPSet1));
			list.Add(Profile("Minfo_Set_Col1", 10000000, testMSet1));
			Print("Set_Int    ", list);
			list.Clear();

			list.Add(Profile("Direct_Set_Col2", 10000000, testSet2));
			list.Add(Profile("FMine_Get_Col1", 10000000, testSet2f));
			list.Add(Profile("Finfo_Get_Col1", 10000000, testFSet2));
			list.Add(Profile("PMine_Set_Col2", 10000000, testSet2p));
			list.Add(Profile("Pinfo_Set_Col2", 10000000, testPSet2));
			list.Add(Profile("Minfo_Set_Col2", 10000000, testMSet2));
			Print("Set_String ", list);
			list.Clear();

			ConstructorInfo ctorInfo = typeof(Test2).GetConstructor(new Type[0]);
			ConstructorInfo ctorInfo2 = typeof(Test2).GetConstructor(new Type[] { typeof(string), typeof(int) });
			Func<Test2> func = Utilities.Reflection.Reflect.Constructor<Test2>();
			Utilities.Reflection.Ctor<Test2> ctor = Utilities.Reflection.Reflect.Constructor<Test2>(new Type[0]);
			Utilities.Reflection.Ctor<Test2> ctor2 = Utilities.Reflection.Reflect.Constructor<Test2>(typeof(string), typeof(int));

			Action testNew = () =>
			{
				Test2 test = new Test2();
			};

			Action testNew2 = () =>
			{
				Test2 test = new Test2("a", 5);
			};

			Action testFunc = () =>
			{
				Test2 test = func();
			};

			Action testCtorInfo = () =>
			{
				Test2 test = (Test2) ctorInfo.Invoke(Array.Empty<object>());
			};

			Action testCtorInfo2 = () =>
			{
				Test2 test = (Test2) ctorInfo2.Invoke(new object[] { "a", 5 });
			};

			Action testCtor = () =>
			{
				Test2 test = ctor();
			};

			Action testCtor2 = () =>
			{
				Test2 test = ctor2("a", 5);
			};

			list.Add(Profile("New       ", 10000000, testNew));
			list.Add(Profile("New2      ", 10000000, testNew2));
			list.Add(Profile("Func      ", 10000000, testFunc));
			list.Add(Profile("Ctor      ", 10000000, testCtor));
			list.Add(Profile("Ctor2     ", 10000000, testCtor2));
			list.Add(Profile("CtorInfo  ", 10000000, testCtorInfo));
			list.Add(Profile("CtorInfo2 ", 10000000, testCtorInfo2));
			Print("Set_String ", list);
			list.Clear();

			Console.ReadLine();
		}

		private static void Print(string description, List<double> values)
		{
			Console.WriteLine(description + " " + string.Join(" | ", values.Select(v => String.Format("{0,10:#####0.}", v))));
		}

		/// <summary>
		/// https://stackoverflow.com/questions/1047218/benchmarking-small-code-samples-in-c-can-this-implementation-be-improved
		/// </summary>
		private static double Profile(string description, int iterations, Action func)
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
