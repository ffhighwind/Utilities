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
	/// Determines the supported runtime of the .NET Framework. It is not gaurenteed to be 100% accurate, 
	/// but it will be close to the expected Framework version. This can be replaced with 
	/// <see cref="System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription"/>
	/// on Frameworks 4.7.1 and higher. FrameworkDescription also supports .NET Core and .NET Native.
	/// </summary>
	public static class DotNetFramework
	{
		/// <summary>
		/// Returns the highest runtime <see cref="DotNetVersion"/>.
		/// </summary>
		/// <see cref="https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/versions-and-dependencies"/> 
		/// <see cref="https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed"/>
		public static DotNetVersion GetVersion()
		{
			try {
				string subKey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
				using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subKey)) {
					if (ndpKey == null) {
						return _GetVersion();
					}
					object releaseKey = ndpKey.GetValue("Release");
					if (releaseKey != null) {
						int release = Convert.ToInt32(releaseKey);
						return GetVersion(release);
					}
				}
			}
			catch {
				// ignore, probably an access violation
			}
			return _GetVersion();
		}

		/// <summary>
		/// Returns the <see cref="DotNetVersion"/> from a given release number.
		/// </summary>
		/// <param name="release"></param>
		/// <returns></returns>
		public static DotNetVersion GetVersion(int release)
		{
			// Windows 10 and below (Windows 7)
			if (release >= 528040) {
				// 528040 on Windows 10
				// 528049 on all other Windows OS
				return DotNetVersion.NET48;
			}
			if (release >= 461808) {
				// 461808 on Windows 10 Server
				// 461814 on all other Windows OS
				return DotNetVersion.NET472;
			}
			if (release >= 461308) {
				// 461308 on Windows 10
				// 461310 on all other Windows OS
				return DotNetVersion.NET471;
			}
			if (release >= 460798) {
				// 460798 on Windows 10
				// 460805 on all other Windows OS
				return DotNetVersion.NET47;
			}
			if (release >= 394802) {
				// 394802 on Windows 10
				// 394806 on all other Windows OS
				return DotNetVersion.NET462;
			}
			if (release >= 394254) {
				// 394254 on Windows 10 
				// 394271 on all other Windows OS
				return DotNetVersion.NET461;
			}
			if (release >= 393295) {
				// 393297 on all other Windows OS
				// 393295 on Windows 10
				return DotNetVersion.NET46;
			}
			// Windows 8.1 and below (Vista)
			if (release >= 379893) {
				return DotNetVersion.NET452;
			}
			if (release >= 378675) {
				// 378758 on Windows 8, Windows 7 SP1, or Windows Vista SP2
				// 378675 on Windows 8.1
				return DotNetVersion.NET451;
			}
			if (release >= 378389) {
				return DotNetVersion.NET45;
			}
			return DotNetVersion.UNKNOWN;
		}

		/// <summary>
		/// Backup strategy for getting the <see cref="DotNetVersion"/> if there is an <see cref="AccessViolationException"/> from
		/// trying to access the registry.
		/// </summary>
		/// <see cref="https://stackoverflow.com/questions/951856/is-there-an-easy-way-to-check-the-net-framework-version"/> 
		private static DotNetVersion _GetVersion()
		{
			// Can also get AppDomain.CurrentDomain.GetAssemblies() and check the Version of the FullName for mscorlib
			if (Type.GetType("System.Data.SqlClient.SqlEnclaveSession, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null) {
				return DotNetVersion.NET472;
			}
			if (Type.GetType("System.Runtime.CompilerServices.IsReadOnlyAttribute, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null) {
				return DotNetVersion.NET471;
			}
			if (Type.GetType("System.Web.Caching.CacheInsertOptions, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false) != null) {
				return DotNetVersion.NET47;
			}
			if (Type.GetType("System.Security.Cryptography.AesCng, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null) {
				return DotNetVersion.NET462;
			}
			if (Type.GetType("System.Data.SqlClient.SqlColumnEncryptionCngProvider, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null) {
				return DotNetVersion.NET461;
			}
			if (Type.GetType("System.AppContext, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null) {
				return DotNetVersion.NET46;
			}
			// 4.5.1 and 4.5.2 are only improvements and can't be tested
			if (Type.GetType("System.Reflection.ReflectionContext, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null) {
				return DotNetVersion.NET45;
			}
			if (Type.GetType("System.Dynamic.CallInfo, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) == null) {
				return DotNetVersion.NET4;
			}
			if (Type.GetType("System.Linq.Expressions.Expression, System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null) {
				return DotNetVersion.NET35;
			}
			if (Type.GetType("System.Workflow.Runtime.CorrelationToken, System.Workflow.Runtime, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", false) != null) {
				return DotNetVersion.NET35;
			}
			if (Type.GetType("System.Collections.Generic.List`1, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null) {
				return DotNetVersion.NET2;
			}
			return DotNetVersion.NET4;
		}

		/// <summary>
		/// Returns the <see cref="FileVersionInfo"/> that the application was compiled under.
		/// </summary>
		/// <returns></returns>
		public static FileVersionInfo GetAssemblyVersion()
		{
			return FileVersionInfo.GetVersionInfo(typeof(Uri).Assembly.Location);
		}

		/// <summary>
		/// Returns a list of <see cref="DotNetRegistry"/> that contain details about what .NET Framworks versions are supported 
		/// by the system. This should be the highest version per major version.
		/// </summary>
		/// <see cref="https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed"/>
		public static IEnumerable<DotNetRegistry> GetVersions()
		{
			const string registryKey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\";
			// using (RegistryKey ndpKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").OpenSubKey(registryKey)) {
			using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(registryKey)) {
				foreach (DotNetRegistry vers in _GetVersions(ndpKey)) {
					yield return vers;
				}
			}
		}

		/// <summary>
		/// Returns a list of <see cref="DotNetRegistry"/> that contain details about what .NET Framworks versions are supported 
		/// by the system. This should be the highest version per major version.
		/// </summary>
		/// <see cref="https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed"/>
		private static IEnumerable<DotNetRegistry> _GetVersions(RegistryKey registryKey)
		{
			if (registryKey == null)
				yield break;
			foreach (string keyName in registryKey.GetSubKeyNames()) {
				if (keyName[0] == 'v') {
					RegistryKey versionKey = registryKey.OpenSubKey(keyName);
					string version = (string) versionKey.GetValue("Version");
					int? sp = (int?) versionKey.GetValue("SP");
					int? release = (int?) versionKey.GetValue("Release");
					//string install = versionKey.GetValue("Install").ToString();

					if (version != null) {
						yield return new DotNetRegistry(keyName, null, version, release, sp);
						continue;
					}
					string[] subKeyNames = versionKey.GetSubKeyNames();
					if (!subKeyNames.Any()) {
						yield return new DotNetRegistry(keyName, null, version, release, sp);
						continue;
					}
					foreach (string subKeyName in versionKey.GetSubKeyNames()) {
						RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
						version = (string) subKey.GetValue("Version");
						release = (int?) subKey.GetValue("Release");
						int? subSP = version == null ? sp : (int?) subKey.GetValue("SP");
						yield return new DotNetRegistry(keyName, subKeyName, version, release, subSP);
					}
				}
			}
		}

		/// <summary>
		/// Returns a list of .NET Framework updates.
		/// </summary>
		/// <see cref="https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-net-framework-updates-are-installed"/>
		public static IEnumerable<string> GetUpdates()
		{
			string registryKey = @"SOFTWARE\Microsoft\Updates";
			using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(registryKey)) {
				if (baseKey == null)
					yield break;
				foreach (string baseKeyName in baseKey.GetSubKeyNames()) {
					if (baseKeyName.Contains(".NET Framework")) {
						using (RegistryKey updateKey = baseKey.OpenSubKey(baseKeyName)) {
							string[] subKeyNames = updateKey.GetSubKeyNames();
							if (!subKeyNames.Any()) {
								yield return baseKeyName;
							}
							else {
								foreach (string kbKeyName in subKeyNames) {
									yield return baseKeyName + " " + kbKeyName;
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Attempts to get the <see cref="DotNetVersion"/> from the given version string (e.g. v4.1.0235). 
		/// It uses basic parsing to convert the version string and is therefore not accurate.
		/// </summary>
		/// <param name="version">The string representation of the .NET Framework version (e.g. v4.1.0235).</param>
		/// <returns>The <see cref="DotNetVersion"/> representation of the version string.</returns>
		public static DotNetVersion GetVersion(string version)
		{
			int[] v = new int[3];

			int ordinal = 0;
			int i = version[0] == 'v' ? 1 : 0;
			for (; i < version.Length; i++) {
				char c = version[i];
				if (c == '.') {
					if (ordinal >= 2) {
						break;
					}
					ordinal++;
				}
				else if (char.IsDigit(c)) {
					v[ordinal] = v[ordinal] * 10;
					v[ordinal] = v[ordinal] + (c - '0');
					if (ordinal == 2) {
						break;
					}
				}
				else {
					break;
				}
			}
			return GetVers(v);
		}

		private static DotNetVersion GetVers(int[] v)
		{
			if (v[0] == 4) {
				switch (v[1]) {
					case 0:
					case 1:
					case 2:
					case 3:
					case 4:
						return DotNetVersion.NET4;
					case 5:
						//if (v[2] == 2) {
						//	return DotNetVersion.NET452;
						//}
						if (v[2] == 1) {
							return DotNetVersion.NET451;
						}
						if (v[2] == 0) {
							return DotNetVersion.NET45;
						}
						return DotNetVersion.NET452;
					case 6:
						//if (v[2] == 2) {
						//	return DotNetVersion.NET462;
						//}
						if (v[2] == 1) {
							return DotNetVersion.NET461;
						}
						if (v[2] == 0) {
							return DotNetVersion.NET46;
						}
						return DotNetVersion.NET462;
					case 7:
						//if (v[2] == 2) {
						//	return DotNetVersion.NET472;
						//}
						if (v[2] == 1) {
							return DotNetVersion.NET471;
						}
						if (v[2] == 0) {
							return DotNetVersion.NET47;
						}
						return DotNetVersion.NET472;
					case 8:
						if (v[2] == 0) {
							return DotNetVersion.NET48;
						}
						return DotNetVersion.NET48;
					default:
						return DotNetVersion.NET48;
				}
			}
			else if (v[0] == 3) {
				if (v[1] < 5) {
					return DotNetVersion.NET3;
				}
				return DotNetVersion.NET35;
			}
			else if (v[0] == 2) {
				return DotNetVersion.NET2;
			}
			else if (v[0] == 1) {
				if (v[1] == 0) {
					return DotNetVersion.NET1;
				}
				return DotNetVersion.NET11;
			}
			return DotNetVersion.UNKNOWN;
		}
	}
}
