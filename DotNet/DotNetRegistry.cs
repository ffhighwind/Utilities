using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Utilities.DotNet
{
	/// <summary>
	/// Windows RegistryKey details about the supported runtimes of the .NET Framework.
	/// </summary>
	public class DotNetRegistry : IComparable<DotNetRegistry>
	{
		internal DotNetRegistry(string key, string subKey, string version, int? release, int? sp)
		{
			Key = key?.Trim();
			SubKey = subKey;
			Version = version?.Trim();
			Release = release;
			SP = sp;
			DotNetVersion = release != null ? DotNetFramework.GetVersion((int) Release) : DotNetFramework.GetVersion(Version ?? Key);
		}

		public string Key { get; private set; }
		public string SubKey { get; private set; }
		public DotNetVersion DotNetVersion { get; private set; }
		public string Version { get; private set; }
		public int? Release { get; private set; }
		public int? SP { get; private set; }
		private string toString;


		public override string ToString()
		{
			return toString ?? CreateToString();
		}

		private string CreateToString()
		{
			StringBuilder sb = new StringBuilder();
			if (Version != null) {
				sb.Append(Version);
			}
			else {
				sb.Append(Key.Substring(1));
			}
			if (SP != null) {
				sb.Append(" SP" + SP);
			}
			if (Release != null) {
				sb.Append(" release " + Release);
			}
			if (SubKey != null) {
				sb.Append(" " + SubKey);
			}
			toString = sb.ToString();
			return toString;
		}

		public int CompareTo(DotNetRegistry other)
		{
			return other == null ? 1 : ToString().CompareTo(other.ToString());
		}
	}
}
