using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
	/// <summary>
	/// The detail to be passed to a <see cref="LogAction"/> when a <see cref="Logger"/> is called.
	/// </summary>
	public class LogState
	{
		/// <summary>
		/// Constructs a <see cref="LogState"/>.
		/// </summary>
		/// <param name="message">The message to write to the log.</param>
		/// <param name="ex">The exception that generated the log.</param>
		/// <param name="method">The calling method that generated the log.</param>
		/// <param name="file">The file that contains the calling method.</param>
		/// <param name="line">The line in the file.</param>
		/// <param name="level">The <see cref="LogLevel"/> or severity of the log.</param>
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

		/// <summary>
		/// The <see cref="LogLevel"/> or severity of the log.
		/// </summary>
		public LogLevel Level { get; private set; }
		/// <summary>
		/// The message to write to the log.
		/// </summary>
		public string Message { get; private set; }
		/// <summary>
		/// The exception that generated the log.
		/// </summary>
		public Exception Exception { get; private set; }
		public int Line { get; private set; }
		/// <summary>
		/// The file that contains the calling method.
		/// </summary>
		public string File { get; private set; }
		/// <summary>
		/// The calling method that generated the log.
		/// </summary>
		public string Method { get; private set; }
		/// <summary>
		/// The time when this <see cref="LogState"/> was created.
		/// </summary>
		public DateTime Time { get; private set; }
	}
}
