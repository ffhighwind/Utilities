﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.UnitTests
{
	public class TestDTO
	{
		[Key]
		public int ID { get; set; }

		[Column("FirstName")]
		public string Name { get; set; }
		public string Email { get; set; }
		public DateTime? CreatedDt { get; set; }
	}
}
