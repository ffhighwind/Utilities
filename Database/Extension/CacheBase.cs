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
			SetValue(value);
		}

		private T _Value { get; set; }

		public T Value {
			get => _Value;
			set => SetValue(value);
		}

		protected virtual void SetValue(T value)
		{
			_Value = value;
		}
	}
}