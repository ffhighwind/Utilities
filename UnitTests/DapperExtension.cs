using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dapper.Extension;

namespace Utilities.UnitTests
{
	[TestClass]
	public class DapperExtension
	{
		[TestMethod]
		public void Test()
		{

			TestDTO dto = new TestDTO() { ID = 1, CreatedDt = new DateTime(), Email = "wesley.hamilton@gmail.com", Name = "Wesley" };

			string name = TableData<TestDTO>.TableName;
			Dapper.TableAttribute attr = TableData<TestDTO>.TableAttribute;
			string[] cols = TableData<TestDTO>.Columns;

			TableDataImpl<TestDTO> x = TableData<TestDTO>.Queries as TableDataImpl<TestDTO>;

		}
	}
}
