using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
	public class StringWriterLogAction : LogAction
	{
		public readonly StringWriter Writer;

		public StringWriterLogAction(StringBuilder sb)
		{
			Writer = new StringWriter(sb);
			Name = this.GetType().FullName + " " + DateTime.Now.Ticks.ToString();
		}

		public override void Log(LogState state)
		{
			string msg = DefaultMessage(state);
			Writer.Write(msg);
		}
	}
}
