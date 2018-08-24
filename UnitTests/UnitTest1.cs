using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace Utilities.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private const string dir = @"D:\Github\Utilities\bin\Debug\";

        [TestMethod]
        public void TestMethod1()
        {
            string filename = "Financial Sample";
            //ReadTest(filename);
            //ForeachTest(filename);
            ReadWriteTest(filename);
        }

        public void ReadWrite(string filename)
        {
            string path = dir + filename;
            List<Test16> list1 = new List<Test16>();
            List<Test16S> list2 = new List<Test16S>();
            List<Test6> list3 = new List<Test6>();
            DataSet set = new DataSet();
            DataTable table = new DataTable();

            list1.Read(path);
            list2.Read(path);
            list3.Read(path);
            set.Read(path);
            table.Read(path);

            /*
            list1.WriteCsv("t16_" + filename + ".csv");
            list1.WriteXlsx("t16_" + filename + ".xlsx");

            list2.WriteCsv("t16s_" + filename + ".csv");
            list2.WriteXlsx("t16s_" + filename + ".xlsx");

            list3.WriteCsv("t6_" + filename + ".csv");
            list3.WriteXlsx("t6_" + filename + ".xlsx");
            */

            set.WriteXlsx("set_" + filename + ".xlsx");

            //table.WriteCsv("table_" + filename + ".csv");
            table.WriteXlsx("table_" + filename + ".xlsx");
        }

        public void ReadWriteTest(string filename)
        {
            //ReadWrite(filename + ".csv");
            ReadWrite(filename + ".xlsx");
            //ReadWrite(filename + ".xls");
            //ReadWrite(filename + ".tsv");
            //ReadWrite("Empty.csv");
        }

        public void ForeachTest(string filename)
        {
            Foreach(dir + filename + ".csv", 700);
            Foreach(dir + filename + ".xlsx", 700);
            Foreach(dir + filename + ".xls", 700);
            Foreach(dir + filename + ".tsv", 700);
            Foreach(dir + "Empty.csv", 0);
        }

        public void ReadTest(string filename)
        {
            Read(dir + filename + ".csv", 700);
            Read(dir + filename + ".xlsx", 700);
            Read(dir + filename + ".xls", 700);
            Read(dir + filename + ".tsv", 700);
            Read(dir + "Empty.csv", 0);
        }

        public void Read(string path, int count)
        {
            List<Test16> list1 = new List<Test16>();
            List<Test16S> list2 = new List<Test16S>();
            List<Test6> list3 = new List<Test6>();
            DataSet set = new DataSet();
            DataTable table = new DataTable();

            list1.Read(path);
            list2.Read(path);
            list3.Read(path);
            set.Read(path);
            table.Read(path);

            //Console.WriteLine("list1: " + list1.Count);
            //Console.WriteLine("list2: " + list1.Count);
            //Console.WriteLine("list3: " + list1.Count);
            //Console.WriteLine("set:   " + list1.Count);
            //Console.WriteLine("table: " + list1.Count);

            if (list1.Count != count)
                throw new Exception();
            if (list2.Count != count)
                throw new Exception();
            if (list3.Count != count)
                throw new Exception();
            if (set.Tables[0].Rows.Count != count)
                throw new Exception();
            if (table.Rows.Count != count)
                throw new Exception();
        }

        public void Foreach(string path, int count)
        {
            List<string[]> strs = IO.Foreach(path).ToList();
            DataTable table = strs.ToDataTable();
            List<Test16> list1 = IO.Foreach<Test16>(path).ToList();
            List<Test16S> list2 = IO.Foreach<Test16S>(path).ToList();
            List<Test6> list3 = IO.Foreach<Test6>(path).ToList();

            //Console.WriteLine("list1: " + list1.Count);
            //Console.WriteLine("list2: " + list1.Count);
            //Console.WriteLine("list3: " + list1.Count);
            //Console.WriteLine("set:   " + list1.Count);

            if (strs.Count != count)
                throw new Exception();
            if (list1.Count != count)
                throw new Exception();
            if (list2.Count != count)
                throw new Exception();
            if (list3.Count != count)
                throw new Exception();
            if (table.Rows.Count != count)
                throw new Exception();
        }
    }
}