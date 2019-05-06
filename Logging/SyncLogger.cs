using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
	/// <summary>
	/// Writes information to a log.
	/// </summary>
	public class SyncLogger : Logger
	{
		/// <summary>
		/// Constructs a <see cref="SyncLogger"/> with the default <see cref="LogAction"/>.
		/// </summary>
		public SyncLogger() : base() { }

#if NETFX_451
		/// <summary>
		/// Calls <see cref="LogAction.Log(LogState)"/> with the given <see cref="LogState"/>.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		/// <param name="methodName">The calling method's name.</param>
		/// <param name="filePath">The file that this method was called.</param>
		/// <param name="lineNumber">The line number that this method was called.</param>
		/// <param name="level">The <see cref="LogLevel"/> of the message.</param>
		protected override void Log(
			string message,
			Exception ex,
			string methodName,
			string filePath,
			int lineNumber,
			LogLevel level)
		{
			if (Level <= level) {
				LogState state = new LogState(message, ex, methodName, filePath, lineNumber, level);
				lock (this) {
					Action.Log(state);
				}
			}
		}
#else
		/// <summary>
		/// Calls <see cref="LogAction.Log(LogState)"/> with the given <see cref="LogState"/>.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		/// <param name="level">The <see cref="LogLevel"/> of the message.</param>
		protected override void Log(
			LogLevel level,
			string message,
			Exception ex = null)
		{
			if (Level <= level) {
				StackFrame stackFrame = new StackTrace(2, true).GetFrame(0);
				string filePath = stackFrame.GetFileName();
				string methodName = stackFrame.GetMethod().ToString();
				int lineNumber = stackFrame.GetFileLineNumber();
				LogState state = new LogState(message, ex, methodName, filePath, lineNumber, level);
				Action.Log(state);
			}
		}
#endif
	}
}
