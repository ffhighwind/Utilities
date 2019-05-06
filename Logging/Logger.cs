using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Utilities.Logging
{
	/// <summary>
	/// Writes information to a log.
	/// </summary>
	public class Logger
	{
		/// <summary>
		/// The <see cref="LogAction"/> to call.
		/// </summary>
		public LogAction Action { get; set; }
		/// <summary>
		/// The minimum <see cref="LogLevel"/> to before calling the <see cref="LogAction"/>.
		/// </summary>
		public LogLevel Level { get; set; } =
#if DEBUG
			LogLevel.DEBUG;
#else
			LogLevel.INFO;
#endif

		/// <summary>
		/// Constructs a <see cref="Logger"/> with the default <see cref="LogAction"/>.
		/// </summary>
		public Logger()
		{
			Action = LogAction.Default.Value;
		}

		/// <summary>
		/// Constructs a <see cref="Logger"/> with the given <see cref="LogAction"/>.
		/// </summary>
		/// <param name="action">The <see cref="LogAction"/> to call.</param>
		public Logger(LogAction action)
		{
			Action = action;
		}

#if NETFX_451
		/// <summary>
		/// Creates a <see cref="LogLevel.INFO"/> message.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		/// <param name="methodName">The calling method's name.</param>
		/// <param name="filePath">The file that this method was called.</param>
		/// <param name="lineNumber">The line number that this method was called.</param>
		public void Info(
			string message,
			Exception ex = null,
			[CallerMemberName] string methodName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Log(message, ex, methodName, filePath, lineNumber, LogLevel.INFO);
		}

		/// <summary>
		/// Creates a <see cref="LogLevel.WARN"/> message.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		/// <param name="methodName">The calling method's name.</param>
		/// <param name="filePath">The file that this method was called.</param>
		/// <param name="lineNumber">The line number that this method was called.</param>
		public void Warn(
			string message,
			Exception ex = null,
			[CallerMemberName] string methodName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Log(message, ex, methodName, filePath, lineNumber, LogLevel.WARN);
		}

		/// <summary>
		/// Creates a <see cref="LogLevel.ERROR"/> message.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		/// <param name="methodName">The calling method's name.</param>
		/// <param name="filePath">The file that this method was called.</param>
		/// <param name="lineNumber">The line number that this method was called.</param>
		public void Error(
			string message,
			Exception ex = null,
			[CallerMemberName] string methodName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Log(message, ex, methodName, filePath, lineNumber, LogLevel.ERROR);
		}

		/// <summary>
		/// Creates a <see cref="LogLevel.FATAL"/> message.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		/// <param name="methodName">The calling method's name.</param>
		/// <param name="filePath">The file that this method was called.</param>
		/// <param name="lineNumber">The line number that this method was called.</param>
		public void Fatal(
			string message,
			Exception ex = null,
			[CallerMemberName] string methodName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Log(message, ex, methodName, filePath, lineNumber, LogLevel.FATAL);
		}

		/// <summary>
		/// Creates a <see cref="LogLevel.DEBUG"/> message.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		/// <param name="methodName">The calling method's name.</param>
		/// <param name="filePath">The file that this method was called.</param>
		/// <param name="lineNumber">The line number that this method was called.</param>
		public void Debug(
			string message,
			Exception ex = null,
			[CallerMemberName] string methodName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Log(message, ex, methodName, filePath, lineNumber, LogLevel.DEBUG);
		}

		/// <summary>
		/// Creates a <see cref="LogLevel.TRACE"/> message.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		/// <param name="methodName">The calling method's name.</param>
		/// <param name="filePath">The file that this method was called.</param>
		/// <param name="lineNumber">The line number that this method was called.</param>
		public void Trace(
			string message,
			Exception ex = null,
			[CallerMemberName] string methodName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Log(message, ex, methodName, filePath, lineNumber, LogLevel.TRACE);
		}

		/// <summary>
		/// Calls <see cref="LogAction.Log(LogState)"/> with the given <see cref="LogState"/>.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		/// <param name="methodName">The calling method's name.</param>
		/// <param name="filePath">The file that this method was called.</param>
		/// <param name="lineNumber">The line number that this method was called.</param>
		/// <param name="level">The <see cref="LogLevel"/> of the message.</param>
		protected virtual void Log(
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
#else
		/// <summary>
		/// Creates a <see cref="LogLevel.INFO"/> message.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Info(
			string message,
			Exception ex = null)
		{
			Log(message, ex, LogLevel.INFO);
		}

		/// <summary>
		/// Creates a <see cref="LogLevel.WARN"/> message.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Warn(
			string message,
			Exception ex = null)
		{
			Log(message, ex, LogLevel.WARN);
		}

		/// <summary>
		/// Creates a <see cref="LogLevel.ERROR"/> message.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Error(
			string message,
			Exception ex = null)
		{
			Log(message, ex, LogLevel.ERROR);
		}

		/// <summary>
		/// Creates a <see cref="LogLevel.TRACE"/> message.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Trace(
			string message,
			Exception ex = null)
		{
			Log(message, ex, LogLevel.TRACE);
		}

		/// <summary>
		/// Creates a <see cref="LogLevel.DEBUG"/> message.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Debug(
			string message,
			Exception ex = null)
		{
			Log(message, ex, LogLevel.DEBUG);
		}

		/// <summary>
		/// Creates a <see cref="LogLevel.FATAL"/> message.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Fatal(
			string message,
			Exception ex = null)
		{
			Log(message, ex, LogLevel.FATAL);
		}

		/// <summary>
		/// Calls <see cref="LogAction.Log(LogState)"/> with the given <see cref="LogState"/>.
		/// </summary>
		/// <param name="message">The message to pass to the <see cref="LogAction"/>.</param>
		/// <param name="ex">The <see cref="Exception"/> to pass to the <see cref="LogAction"/>.</param>
		/// <param name="level">The <see cref="LogLevel"/> of the message.</param>
		protected virtual void Log(
			string message,
			Exception ex,
			LogLevel level)
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