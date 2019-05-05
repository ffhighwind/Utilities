using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
	public enum LogLevel
	{
		ALL = int.MinValue,
		TRACE = -100000,
		DEBUG = -1000,
		INFO  = 0,
		WARN  = 1000,
		ERROR = 100000,
		FATAL = 10000000,
		OFF = int.MaxValue
	}
}
