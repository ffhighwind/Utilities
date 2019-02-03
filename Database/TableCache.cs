using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Database
{
	public class TableCache<T> where T : class
	{
		//handle foreign keys?
		public TableCache(string connString, string whereConditions = "")
		{
			ConnString = connString;
			WhereConditions = whereConditions;
		}

		public string ConnString { get; set; }
		public string WhereConditions { get; set; }



		//check if stale then pull everything
		//store the ConnString and Query
		//should include all of the queries but as virtual
		//should have a force update of an item
	}
}
