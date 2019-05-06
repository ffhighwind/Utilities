using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
	/// <summary>
	/// An action to take when a <see cref="Logger"/> is being called.
	/// </summary>
	public abstract class LogAction
	{
		/// <summary>
		/// Constructs a <see cref="LogAction"/> with a default name.
		/// </summary>
		public LogAction()
		{
			Name = this.GetType().FullName + " " + DateTime.Now.Ticks.ToString();
		}

		/// <summary>
		/// Constructs a <see cref="LogAction"/> with the given name.
		/// </summary>
		/// <param name="name">The name of this <see cref="LogAction"/>.</param>
		public LogAction(string name)
		{
			Name = name;
		}

		/// <summary>
		/// The name of the <see cref="LogAction"/>. This can be used to distinguish it from other actions.
		/// </summary>
		public string Name { get; protected set; }

		/// <summary>
		/// A <see cref="LogAction"/> that writes to <see cref="Console.Out"/>.
		/// </summary>
		public static readonly LogAction ConsoleOut = new TextWriterLogAction(System.Console.Out, "Console.Out");
		/// <summary>
		/// A <see cref="LogAction"/> that writes to <see cref="Console.Error"/>.
		/// </summary>
		public static readonly LogAction ConsoleError = new TextWriterLogAction(System.Console.Error, "Console.Error");
		/// <summary>
		/// A <see cref="LogAction"/> that writes to the file "log.txt".
		/// </summary>
		public static readonly Lazy<LogAction> Default = new Lazy<LogAction>(() => new StreamWriterLogAction("log.txt"));

		/// <summary>
		/// The method that handles a call from a <see cref="Logger"/>.
		/// </summary>
		/// <param name="state">The <see cref="LogState"/> passed by the <see cref="Logger"/>.</param>
		public abstract void Log(LogState state);

		/// <summary>
		/// Returns the default message from a given <see cref="LogState"/>.
		/// </summary>
		/// <param name="state">The <see cref="LogState"/> passed by the <see cref="Logger"/>.</param>
		/// <returns>A message based on a given <see cref="LogState"/>.</returns>
		public static string DefaultMessage(LogState state)
		{
			StringBuilder sb = new StringBuilder();
			string level = state.Level.ToString();
			sb.AppendLine($"[{state.Time.ToString("MM/dd/yyyy hh:mm:ss")} {level}] {Path.GetFileName(state.File)}::{state.Method}({state.Line})");
			if (state.Exception != null) {
				sb.AppendLine($@"{state.Exception.GetType().Name}  {state.Exception.Message}");
			}
			if (!string.IsNullOrWhiteSpace(state.Message)) {
				sb.AppendLine(state.Message);
			}
			return sb.ToString();
		}

		/// <summary>
		/// Returns a compact message from a given <see cref="LogState"/>.
		/// </summary>
		/// <param name="state">The <see cref="LogState"/> passed by the <see cref="Logger"/>.</param>
		/// <returns>A message based on a given <see cref="LogState"/>.</returns>
		public static string CompactMessage(LogState state)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append($"[{state.Time.ToString("MM/dd hh:mm")}] ");
			if (state.Exception != null) {
				sb.Append($@"{state.Exception.GetType().Name}  ");
			}
			if (!string.IsNullOrWhiteSpace(state.Message)) {
				sb.Append(state.Message);
			}
			return sb.AppendLine().ToString();
		}
	}
}
