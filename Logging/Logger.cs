#if !NETFX_451
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Utilities.Logging
{
	public class Logger
	{
		public LogAction Action { get; set; }
#if DEBUG
		public LogLevel Level { get; set; } = LogLevel.DEBUG;
#else
		public LogLevel Level { get; set; } = LogLevel.INFO;
#endif

		public Logger()
		{
			Action = LogAction.Default;
		}

		public Logger(LogAction action)
		{
			Action = action;
		}

		public void Info(
			string message,
			Exception ex = null)
		{
			Log(LogLevel.INFO, message, ex);
		}

		public void Warn(
			string message,
			Exception ex = null)
		{
			Log(LogLevel.WARN, message, ex);
		}

		public void Error(
			string message,
			Exception ex = null)
		{
			Log(LogLevel.ERROR, message, ex);
		}

		public void Trace(
			string message,
			Exception ex = null)
		{
			Log(LogLevel.TRACE, message, ex);
		}

		public void Debug(
			string message,
			Exception ex = null)
		{
			Log(LogLevel.DEBUG, message, ex);
		}

		public void Fatal(
			string message,
			Exception ex = null)
		{
			Log(LogLevel.FATAL, message, ex);
		}

		protected void Log(
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
	}
}
#endif