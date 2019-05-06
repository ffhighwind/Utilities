using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
	/// <summary>
	/// Internal <see cref="LogAction"/> for use with <see cref="Console.Out"/> and <see cref="Console.Error"/>.
	/// </summary>
	internal class TextWriterLogAction : LogAction
	{
		private readonly TextWriter Writer;
		public TextWriterLogAction(TextWriter writer, string name)
		{
			Name = name;
			Writer = writer;
		}

		public override void Log(LogState state)
		{
			string msg = DefaultMessage(state);
			Writer.Write(msg);
		}
	}
}
