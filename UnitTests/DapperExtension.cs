﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Dapper.Extension;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Utilities.UnitTests
{
	[TestClass]
	public class DapperExtension
	{
		//private const string ConnString = @"DESKTOP-V0JVTST\SQLEXPRESS";
		//private const string ConnString = @"Server=DESKTOP-V0JVTST\SQLEXPRESS; Database=Test; Trusted_Connection=True;";
		//private const string ConnString = @"Server=(localdb)\MyInstance;Integrated Security=true;"

		//private const string ConnString = @"Data Source=C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS\MSSQL\DATA\Test.mdf; Initial Catalog=Test; Integrated Security=true; User Instance=True;";
		//private const string ConnString = @"Data Source=.; Initial Catalog=Test; Integrated Security=true;";

		private const string ConnString = @"Data Source=DESKTOP-V0JVTST\SQLEXPRESS;Initial Catalog=Test;Integrated Security=True;";

		[TestMethod]
		public void Test()
		{
			List<TestDTO> list = new List<TestDTO>()
			{
				new TestDTO() { ID = -1, CreatedDt = new DateTime(2019, 1, 1), Name = "Wesley" },
				new TestDTO() { ID = -1, CreatedDt = new DateTime(2019, 1, 2), Name = "Wesley2" },
				new TestDTO() { ID = -1, CreatedDt = new DateTime(2019, 1, 3), Name = "Wesley3" }
			};

			TestDTO dto = new TestDTO() { CreatedDt = new DateTime(2019, 1, 15), Name = "Test" };


			using (SqlConnection conn = new SqlConnection(ConnString)) {
				conn.Open();
				using (SqlTransaction trans = conn.BeginTransaction()) {

					// Bulk Insert
					list = conn.BulkInsert(list, trans).OrderBy(x => x.ID).ToList();
					for (int i = 0; i < list.Count; i++) {
						if (list[i].ID == -1) {
							throw new InvalidOperationException();
						}
					}
					// BulkDelete
					int count = conn.BulkDelete(list, trans);

					list = conn.BulkInsert(list, trans).OrderBy(x => x.ID).ToList();
					// Update
					for (int i = 0; i < list.Count; i++) {
						list[i].Name = "Update " + list[i].Name;
						conn.Update(list[i], trans);
					}
					// Get
					for (int i = 0; i < list.Count; i++) {
						TestDTO tmp = conn.Get(list[i], trans);
						if (!tmp.Name.StartsWith("Update")) {
							throw new InvalidOperationException();
						}
					}

					// GetList
					List<TestDTO> newList = conn.GetList<TestDTO>("", null, trans).OrderBy(x => x.ID).ToList();
					if (newList.Count != list.Count) {
						throw new InvalidOperationException();
					}
					for (int i = 0; i < list.Count; i++) {
						if (newList[i].ID != list[i].ID) {
							throw new InvalidOperationException();
						}
					}

					// BulkUpsert
					list.Add(dto);
					for (int i = 0; i < list.Count; i++) {
						list[i].CreatedDt = list[i].CreatedDt.Value.AddDays(5);
					}

					//upsert
					for(int i = 0; i < list.Count; i++) {
						conn.Upsert(list[i], trans);
					}
					//conn.BulkUpsert(list, trans);
					newList = conn.GetList<TestDTO>("", null, trans).OrderBy(x => x.ID).ToList();
					if (newList.Count != list.Count) {
						throw new InvalidOperationException();
					}
					for (int i = 0; i < list.Count; i++) {
						if (newList[i].CreatedDt == list[i].CreatedDt) {
							throw new InvalidOperationException();
						}
					}

					// RecordCount
					count = conn.RecordCount<TestDTO>("", null, trans);
					if (count != 4)
						throw new InvalidOperationException();

					// BulkDelete
					count = conn.BulkDelete(list, trans);
					if (count != 4)
						throw new InvalidOperationException();

					// RecordCount
					count = conn.RecordCount<TestDTO>("", null, trans);
					if (count != 0)
						throw new InvalidOperationException();

					// Upsert
					conn.Upsert(list[0], trans);
					newList = conn.GetList<TestDTO>("", null, trans).OrderBy(x => x.ID).ToList();
					if (newList.Count != 1)
						throw new InvalidOperationException();
					if (list[0].ID != newList[0].ID)
						throw new InvalidOperationException();

					// Insert
					for (int i = 1; i < list.Count; i++) {
						conn.Insert(list[i], trans);
					}

					// DeleteList
					newList = conn.DeleteList<TestDTO>("", null, trans).OrderBy(x => x.ID).ToList();
					if (newList.Count != 4)
						throw new InvalidOperationException();
					for (int i = 0; i < newList.Count; i++) {
						if (list[i].ID != newList[i].ID)
							throw new InvalidOperationException();
					}
				}
			}
		}
	}
}
