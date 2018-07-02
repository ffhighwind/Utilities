using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Excel;

#if net472
test
#endif

namespace Utilities
{

    public class X
    {
        public DateTime OrderDate { get; set; }
        public string Region { get; set; }
        public string Rep { get; set; }
        public string Item { get; set; }
        public int Units { get; set; }
        public double UnitCost { get; set; }
        public decimal Total { get; set; }
        public string Time { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try {
                /*
                DataTable table1 = new DataTable();
                table1.ReadCsv("Sample-Spreadsheet-500000-rows.csv");
                //table1.WriteXlsx("Sample-Spreadsheet-500000-rows.xlsx");

                DataTable table2 = new DataTable();
                table2.ReadCsv("PoE All Uniques - Sheet.csv");
                //table2.WriteXlsx("PoE All Uniques - Sheet.xlsx");

                DataTable table3 = new DataTable();
                table3.ReadXlsx("Book1.xlsx");
                //table3.WriteXlsx("Book2.xlsx");


                DataTable table4 = new DataTable();
                table4.ReadCsv("Book3.csv");
                table4.ReadXlsx("Book3.xlsx");
                //table4.WriteXlsx("Book4.xlsx");
                /*
                DataTable table = Util.DataTable<Class1>();
                var converter = Util.DataRowConverter<Class1>(table);
                table.ToList<Class1>();
                */
                /*DataTable table5 = new DataTable();
                table5.ReadXlsx("Book5.xlsx");
                */
                //Log.Logger.Instance.DefaultStyle = Log.LogStyle.DateTimeMethodFileLine;
                //Log.Logger.Instance.Log("text");

                var x = IO.XlsxForeach<X>("Book3.xlsx");
                var y = x.ToList();
                y.WriteXlsx("tmp.xlsx");

                //var x = IO.ReadXlsx("Book4 - Copy.xlsx");
                // x.Print(true);
            }
            catch (Exception ex) {
                System.Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
