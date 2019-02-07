using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
	public interface ICacheBase<T> where T : class
	{
		T Value { get; set; }
	}
}