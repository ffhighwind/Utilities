using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public class CacheBase<T> where T : class
	{
		public CacheBase() { }

		public CacheBase(T value)
		{
			Value = value;
		}

		public virtual T Value { get; set; }
	}
}