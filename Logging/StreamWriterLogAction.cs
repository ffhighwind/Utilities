using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
	/// <summary>
	/// A <see cref="LogAction"/> that writes to a file.
	/// </summary>
	public class StreamWriterLogAction : LogAction, IDisposable
	{
		private StreamWriter Writer;

		/// <summary>
		/// Constructs a <see cref="StreamWriterLogAction"/> from the given file.
		/// </summary>
		/// <param name="path">The path or filename of the file to write to.</param>
		/// <param name="append">Determines if the file should be appended to.</param>
		public StreamWriterLogAction(string path, bool append = true)
		{
			FileInfo fi = new FileInfo(path);
			Writer = fi.Exists ? new StreamWriter(path, append) : fi.CreateText();
			Name = fi.FullName;
		}

		/// <summary>
		/// Determines if the log message should be in compact mode.
		/// </summary>
		public bool Compact { get; set; }

		/// <summary>
		/// Writes the log message to the given file.
		/// </summary>
		/// <param name="state">The <see cref="LogState"/>.</param>
		public override void Log(LogState state)
		{
			string msg = Compact ? CompactMessage(state) : DefaultMessage(state);
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
