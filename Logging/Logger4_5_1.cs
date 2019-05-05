#if NETFX_451
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;


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
			Exception ex = null,
			[CallerMemberName] string methodName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Log(message, ex, methodName, filePath, lineNumber, LogLevel.INFO);
		}

		public void Warn(
			string message,
			Exception ex = null,
			[CallerMemberName] string methodName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Log(message, ex, methodName, filePath, lineNumber, LogLevel.WARN);
		}

		public void Error(
			string message,
			Exception ex = null,
			[CallerMemberName] string methodName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Log(message, ex, methodName, filePath, lineNumber, LogLevel.ERROR);
		}

		public void Fatal(
			string message,
			Exception ex = null,
			[CallerMemberName] string methodName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Log(message, ex, methodName, filePath, lineNumber, LogLevel.FATAL);
		}

		public void Debug(
			string message,
			Exception ex = null,
			[CallerMemberName] string methodName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Log(message, ex, methodName, filePath, lineNumber, LogLevel.DEBUG);
		}

		public void Trace(
			string message,
			Exception ex = null,
			[CallerMemberName] string methodName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Log(message, ex, methodName, filePath, lineNumber, LogLevel.TRACE);
		}

		protected void Log(
			string message,
			Exception ex,
			string methodName,
			string filePath,
			int lineNumber,
			LogLevel level)
		{
			if (Level <= level) {
				LogState state = new LogState(message, ex, methodName, filePath, lineNumber, level);
				Action.Log(state);
			}
		}
	}

}
#endif