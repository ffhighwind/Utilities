using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
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
