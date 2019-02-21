using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Dapper.Extension;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Utilities.UnitTests
{
	[TestClass]
	public class Database
	{
		private const string ConnString = @"Data Source=DESKTOP-V0JVTST\SQLEXPRESS;Initial Catalog=Test;Integrated Security=True;";

		[TestMethod]
		public void Test()
		{
			List<TestDTO> test = new List<TestDTO>();
			using (SqlConnection conn = new SqlConnection(ConnString)) {
				conn.Open();
				SqlTransaction trans = null;
				//using (SqlTransaction trans = conn.BeginTransaction()) {
					Test2 test2 = new Test2()
					{
						Col1 = 15,
						Col2 = "Testing",
						Col3 = 5.23f
					};

					for (int i = 0; i < 10; i++) {
						conn.Insert(test2, trans);
					}

					TableData<Test2>.Queries.RemoveDuplicates(conn, trans);

					int k = 0;
				//}
			}
		}
	}
}
