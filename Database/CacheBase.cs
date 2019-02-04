using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Database
{
	public class CacheBase<T> where T : class
	{
		public CacheBase() { }

		public CacheBase(T value)
		{
			Value = value;
		}

		protected T _Value { get; set; }
		public virtual T Value {
			get => _Value;
			set { _Value = value; }
		}
	}
}