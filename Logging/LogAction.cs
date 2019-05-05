using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
	public abstract class LogAction
	{
		public string Name { get; protected set; }

		public static readonly LogAction ConsoleOut = new TextWriterLogAction(System.Console.Out, "Console.Out");
		public static readonly LogAction ConsoleError = new TextWriterLogAction(System.Console.Error, "Console.Error");
		public static readonly LogAction Default = new StreamWriterLogAction("log.txt");
		public static readonly LogAction None = new NoLogAction();

		public abstract void Log(LogState state);

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

		public static string CompactMessage(LogState state)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append($"[{state.Time.ToString("MM/dd hh:mm")}] ");
			if (state.Exception != null) {
				sb.Append($@"{state.Exception.GetType().Name}  ");
			}
			if (!string.IsNullOrWhiteSpace(state.Message)) {
				sb.AppendLine(state.Message);
			}
			return sb.ToString();
		}
	}
}
