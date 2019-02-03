using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Database
{
	public class CacheData<T> where T : class
	{
		public CacheData(T value, DateTime staleTime)
		{
			Value = value;
			StaleTime = staleTime;
		}

		public T Value { get; internal set; }
		public DateTime StaleTime { get; internal set; }

		internal bool IsStale(DateTime now)
		{
			return now > StaleTime;
		}

		public bool IsStale()
		{
			return DateTime.Now > StaleTime;
		}
	}
}
