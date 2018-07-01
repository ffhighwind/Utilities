using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Processes
    {
        /// <summary>
        /// Executes a command from the command-line and then waits for the command to finish before returning.
        /// </summary>
        /// <param name="path">The path of the executable.</param>
        /// <param name="args">The arguments to pass to the executable.</param>
        /// <param name="showWindow">Determines if the console window is shown.</param>
        /// <param name="redirectOutput"></param>
        /// <param name="maxWaitMilis">The maximum number of miliseconds to wait. Negative numbers mean infinite.</param>
        /// <returns>The process that ran the command or null on error.</returns>
        public static Process Exec(string path, string args = "", bool showWindow = false, bool redirectOutput = false, int maxWaitMilis = -1)
        {
            try {
                Process proc = ExecAsync(path, args, showWindow, redirectOutput);
                if (proc != null) {
                    proc.WaitForExit(maxWaitMilis);
                    return proc;
                }
            }
            catch (Exception ex) {
                Console.Error.WriteLine(ex.Message);
            }
            return null; //EXIT_FAILURE
        }


        /// <summary>
        /// Executes a command from the command-line and asynchronously.
        /// </summary>
        /// <param name="path">The path of the executable.</param>
        /// <param name="args">The arguments to pass to the executable.</param>
        /// <param name="showWindow">Determines if the console window is shown.</param>
        /// <param name="redirectOutput"></param>
        /// <returns>The process that ran the command or null on error.</returns>
        public static Process ExecAsync(string path, string args = "", bool showWindow = false, bool redirectOutput = false)
        {
            try {
                ProcessStartInfo proc = new ProcessStartInfo();
                if (!showWindow) {
                    proc.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.CreateNoWindow = !showWindow;
                }
                proc.UseShellExecute = false;
                proc.RedirectStandardOutput = redirectOutput; //allows you to read from the process's stdoutput
                proc.FileName = path;
                proc.Arguments = args;
                //proc.FileName = @"C:\Windows\System32\cmd.exe";
                //proc.Arguments = "/C " + command; // /C terminates the process when it's done executing the command
                //proc.WorkingDirectory = Directory.GetCurrentDirectory();
                return Process.Start(proc);
            }
            catch (Exception ex) {
                Console.Error.WriteLine(ex.Message);
                return null;
            }
        }
        
        /// <summary>
        /// Forces the current process to restart as an Administrator.
        /// </summary>
        /// <param name="args">The arguments to pass to the process.</param>
        public static void ForceAdmin(string[] args = null)
        {
            if (!IsAdministrator) {
                try {
                    string arg = string.Empty;
                    if (args == null)
                        arg = string.Empty;
                    else {
                        for (int i = 0; i < args.Length; i++) {
                            arg = "\"" + args[i] + "\" ";
                        }
                    }
                    var info = new ProcessStartInfo(
                        Assembly.GetEntryAssembly().Location, args == null ? "" : "\"" + string.Join("\" \"", args) + "\"") {
                        Verb = "runas", // indicates to elevate privileges
                    };

                    var process = new Process {
                        EnableRaisingEvents = true, // enable WaitForExit()
                        StartInfo = info
                    };

                    if (process.Start())
                        process.WaitForExit(); // sleep calling process thread until evoked process exit
                    System.Environment.Exit(process.ExitCode);
                }
                catch (Exception ex) {
                    Console.Error.WriteLine("Error RunAsAdmin: " + ex.Message);
                }
                System.Environment.Exit(1);
            }
        }

        /// <summary>
        /// Determines if the current process is an Administrator.
        /// </summary>
        public static bool IsAdministrator {
            get {
                return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                          .IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        #region Arguments
        /// <summary>
        /// Expands the wildcards (? and *) and environment variables (%var%) in a list of paths. 
        /// Paths to files and directories that match the search pattern will be returned including paths that are skipped.
        /// </summary>
        /// <param name="paths">The paths to expand.</param>
        /// <param name="skipIfStartsWith">If an argument starts with any of these characters then it will not be expanded.</param>
        /// <param name="includeDirs">Determines if directories matching the search pattern</param>
        /// <param name="expandEnvVars">Determines if environment variables should be expanded (%var)%.</param>
        /// <returns>The paths of file system entries that match the search pattern.</returns>
        public static List<string> ExpandPaths(IEnumerable<string> paths, string skipIfStartsWith = "-", bool includeDirs = true, bool expandEnvVars = true)
        {
            List<string> expanded = new List<string>();
            foreach (string path in paths) {
                if (skipIfStartsWith.Contains(path[0]))
                    continue;
                if (Directory.Exists(path)) {
                    if (includeDirs)
                        continue;
                }
                else if (File.Exists(path))
                    expanded.Add(path);
                else
                    expanded.AddRange(ExpandPath(path, includeDirs, expandEnvVars));
            }
            return expanded;
        }

        /// <summary>
        /// Expands the wildcards (? and *) and environment variables (%var%) in a path. 
        /// Paths that match the search pattern will be returned.
        /// </summary>
        /// <param name="path">The path to expand. This can include wildcards (* ?) and environment variables (%var%).</param>
        /// <param name="skipIfStartsWith">If an argument starts with any of these characters then it will not be expanded.</param>
        /// <param name="includeDirs">Determines if directories matching the search pattern</param>
        /// <param name="expandEnvVars">Determines if environment variables should be expanded (%var)%.</param>
        /// <returns>The paths of file system entries that match the search pattern.</returns>
        public static IEnumerable<string> ExpandPath(string path, bool includeDirs = true, bool expandEnvVars = true)
        {
            string envExpanded = expandEnvVars ? Environment.ExpandEnvironmentVariables(path) : path;
            //skip initial slashes in case it's a network drive.
            string[] pathParts = SplitPath(envExpanded);
            List<DirectoryInfo> dirs = GetDirectories(pathParts);
            string pattern = pathParts.Last();
            string patternExt = Path.GetExtension(pattern);
            List<string> results = new List<string>();
            if (patternExt.Length == 3 && !patternExt.Contains('*') && !patternExt.Contains('?')) {
                string patternFile = Path.GetFileNameWithoutExtension(pattern);
                foreach (DirectoryInfo dir in dirs) {
                    IEnumerable<string> files = includeDirs ?
                        Directory.GetFileSystemEntries(dir.FullName, patternFile)
                        : Directory.GetFiles(dir.FullName, patternFile);
                    results.AddRange(files.Where(item => item.EndsWith(patternExt, StringComparison.InvariantCultureIgnoreCase)));
                }
            }
            else {
                foreach (DirectoryInfo dir in dirs) {
                    results.AddRange(includeDirs ?
                        Directory.GetFileSystemEntries(dir.FullName, pattern)
                        : Directory.GetFiles(dir.FullName, pattern));
                }
            }
            return results;
        }

        private static char[] dirSeparators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        /// <summary>
        /// Splits a path by the '\'. Special case for network drives that start with '\\'.
        /// </summary>
        /// <param name="path">The path to split.</param>
        /// <returns>An array of path parts.</returns>
        public static string[] SplitPath(string path)
        {
            if (path.Length == 0)
                return new string[] { "" };
            if (path[0] == Path.DirectorySeparatorChar || path[0] == Path.AltDirectorySeparatorChar) {
                int start = 1;
                if (path.Length > 1 && (path[1] == Path.DirectorySeparatorChar || path[1] == Path.AltDirectorySeparatorChar))
                    start = 2;
                int indexOfSep = path.IndexOfAny(dirSeparators, start);
                if (indexOfSep > 0) {
                    List<string> pathParts = new List<string>();
                    pathParts.Add(path.Substring(0, indexOfSep));
                    pathParts.AddRange(path.Substring(indexOfSep + 1).Split(dirSeparators));
                    return pathParts.ToArray();
                }
                else
                    return new string[] { path };
            }
            return path.Split(dirSeparators);
        }

        /// <summary>
        /// Gets a list of directories matching a search pattern.
        /// </summary>
        private static List<DirectoryInfo> GetDirectories(string[] pathParts)
        {
            List<DirectoryInfo> infos = new List<DirectoryInfo>();
            if (pathParts.Length > 1) {
                DirectoryInfo startDir = new DirectoryInfo(pathParts[0]);
                GetDirectories(startDir, pathParts, 1, infos);
            }
            else
                infos.Add(new DirectoryInfo(Directory.GetCurrentDirectory()));
            return infos;
        }

        /// <summary>
        /// Recursively gets a list of directories matching a search pattern.
        /// </summary>
        private static void GetDirectories(DirectoryInfo dir, string[] pathParts, int index, List<DirectoryInfo> infos)
        {
            if (index < pathParts.Length - 1) {
                DirectoryInfo[] dirs = dir.GetDirectories(pathParts[index]);
                index++;
                foreach (DirectoryInfo adir in dirs) {
                    GetDirectories(adir, pathParts, index, infos);
                }
            }
            else
                infos.Add(dir);
        }
        #endregion
    }
}
