using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
	/// <summary>
	/// A <see cref="LogAction"/> that writes to a <see cref="StringBuilder"/>.
	/// </summary>
	public class StringWriterLogAction : LogAction
	{
		public readonly StringWriter Writer;

		/// <summary>
		/// Constructs a <see cref="StringWriterLogAction"/> from the given <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="sb">The <see cref="StringBuilder"/> to write to.</param>
		public StringWriterLogAction(StringBuilder sb)
		{
			Writer = new StringWriter(sb);
		}

		/// <summary>
		/// Determines if the log message should be in compact mode.
		/// </summary>
		public bool Compact { get; set; }

		/// <summary>
		/// Writes the log message to the given <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="state">The <see cref="LogState"/>.</param>
		public override void Log(LogState state)
		{
			string msg = Compact ? CompactMessage(state) : DefaultMessage(state);
			Writer.Write(msg);
		}
	}
}
