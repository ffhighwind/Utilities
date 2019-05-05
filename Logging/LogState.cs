using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
	public class LogState
	{
		public LogState(string message, Exception ex, string method, string file, int line, LogLevel level)
		{
			Message = message;
			Method = method;
			File = file;
			Line = line;
			Time = DateTime.Now;
			Level = level;
			Exception = ex;
		}

		public LogLevel Level { get; private set; }
		public string Message { get; private set; }
		public Exception Exception { get; private set; }
		public int Line { get; private set; }
		public string File { get; private set; }
		public string Method { get; private set; }
		public DateTime Time { get; private set; }
	}
}
