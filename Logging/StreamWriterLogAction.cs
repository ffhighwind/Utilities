using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
	public class StreamWriterLogAction : LogAction, IDisposable
	{
		private StreamWriter Writer;
		public StreamWriterLogAction(string path, bool append = true)
		{
			FileInfo fi = new FileInfo(path);
			Writer = fi.Exists ? new StreamWriter(path, append) : fi.CreateText();
			Name = fi.FullName;
		}

		public override void Log(LogState state)
		{
			string msg = DefaultMessage(state);
			Writer.Write(msg);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue) {
				if (disposing) {
					Writer.Dispose();
					Writer = null;
				}
				disposedValue = true;
			}
		}

		// ~FileWriterLogAction() {
		//   Dispose(false);
		// }

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
