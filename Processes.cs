using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;

namespace Utilities
{
    public static class Processes
    {
        /// <summary>
        /// Executes a command from the command-line and then waits for the command to finish before returning.
        /// </summary>
        /// <param name="path">The path of the executable.</param>
        /// <param name="args">The arguments to pass to the executable.</param>
        /// <param name="redirectOutput">Determines if output should be redirected.
        /// This enables reading from the process's standard input and error.</param>
        /// <param name="maxWaitMillis">The maximum number of milliseconds to wait. Negative numbers mean infinite.</param>
        /// <param name="showWindow">Determines if the console window is shown.</param>
        /// <returns>The process that ran the command or null on error.</returns>
        public static Process Exec(string path, string args = "", bool redirectOutput = false, int maxWaitMillis = -1, bool showWindow = false)
        {
            try {
                Process proc = ExecAsync(path, args, redirectOutput, showWindow);
                if (proc != null) {
                    proc.WaitForExit(maxWaitMillis);
                    return proc;
                }
            }
            catch (Exception ex) {
                Console.Error.WriteLine(ex.Message);
            }
            return null; // EXIT_FAILURE
        }

        /// <summary>
        /// Executes a command from the command-line and asynchronously.
        /// </summary>
        /// <param name="path">The path of the executable.</param>
        /// <param name="args">The arguments to pass to the executable.</param>
        /// <param name="redirectOutput">Determines if output should be redirected.
        /// This enables reading from the process's standard input and error.</param>
        /// <param name="showWindow">Determines if the console window is shown.</param>
        /// <returns>The process that ran the command.</returns>
        public static Process ExecAsync(string path, string args = "", bool redirectOutput = false, bool showWindow = false)
        {
            ProcessStartInfo proc = new ProcessStartInfo();
            if (!showWindow) {
                proc.WindowStyle = ProcessWindowStyle.Hidden;
                proc.CreateNoWindow = true;
            }
            proc.UseShellExecute = false;
            proc.RedirectStandardOutput = redirectOutput; // allows you to read from the process's stdoutput
            proc.RedirectStandardError = redirectOutput;
            proc.FileName = path;
            proc.Arguments = args;
            ////proc.FileName = @"C:\Windows\System32\cmd.exe";
            ////proc.Arguments = "/C " + command; // /C terminates the process when it's done executing the command
            ////proc.WorkingDirectory = Directory.GetCurrentDirectory();
            return Process.Start(proc);
        }

        /// <summary>
        /// Forces the current process to restart as an Administrator.
        /// </summary>
        /// <param name="args">The arguments to pass to the process.</param>
        public static void ForceAdmin(string[] args = null)
        {
            if (!IsAdministrator) {
                ProcessStartInfo info = new ProcessStartInfo(
                    Assembly.GetEntryAssembly().Location, args == null ? "" : "\"" + string.Join("\" \"", args) + "\"") {
                    Verb = "runas", // indicates to elevate privileges
                };
                Process process = new Process {
                    EnableRaisingEvents = true, // enable WaitForExit()
                    StartInfo = info
                };
                if (process.Start())
                    process.WaitForExit(); // sleep calling process thread until evoked process exit
                System.Environment.Exit(process.ExitCode);
            }
        }

        /// <summary>
        /// Determines if the current process is an Administrator.
        /// </summary>
        public static bool IsAdministrator => new WindowsPrincipal(WindowsIdentity.GetCurrent())
            .IsInRole(WindowsBuiltInRole.Administrator);

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
                if (!skipIfStartsWith.Contains(path[0])) {
                    if (File.Exists(path))
                        expanded.Add(path);
                    else if (Directory.Exists(path)) {
                        if (includeDirs)
                            expanded.Add(path);
                    }
                    else
                        expanded.AddRange(ExpandPath(path, includeDirs, expandEnvVars));
                }
            }
            return expanded;
        }

        /// <summary>
        /// Expands the wildcards (? and *) and environment variables (%var%) in a path.
        /// Paths that match the search pattern will be returned.
        /// </summary>
        /// <param name="path">The path to expand. This can include wildcards (* ?) and environment variables (%var%).</param>
        /// <param name="includeDirs">Determines if directories matching the search pattern</param>
        /// <param name="expandEnvVars">Determines if environment variables should be expanded (%var)%.</param>
        /// <returns>The paths of file system entries that match the search pattern.</returns>
        public static IEnumerable<string> ExpandPath(string path, bool includeDirs = true, bool expandEnvVars = true)
        {
            string envExpanded = expandEnvVars ? Environment.ExpandEnvironmentVariables(path) : path;
            // skip initial slashes in case it's a network drive.
            string[] pathParts = SplitPath(envExpanded);
            List<DirectoryInfo> dirs = GetDirectories(pathParts);
            string pattern = pathParts.Last();
            string patternExt = Path.GetExtension(pattern);
            if (patternExt.Length == 3 && !patternExt.Contains('*') && !patternExt.Contains('?')) {
                string patternFile = Path.GetFileNameWithoutExtension(pattern);
                foreach (DirectoryInfo dir in dirs) {
                    IEnumerable<string> files = includeDirs ?
                        Directory.GetFileSystemEntries(dir.FullName, patternFile)
                        : Directory.GetFiles(dir.FullName, patternFile);
                    foreach (string file in files.Where(item => item.EndsWith(patternExt, StringComparison.InvariantCultureIgnoreCase))) {
                        yield return file;
                    }
                }
            }
            else {
                foreach (DirectoryInfo dir in dirs) {
                    string[] files = includeDirs ?
                        Directory.GetFileSystemEntries(dir.FullName, pattern)
                        : Directory.GetFiles(dir.FullName, pattern);
                    for (int i = 0; i < files.Length; i++) {
                        yield return files[i];
                    }
                }
            }
        }

        private static readonly char[] DirSeparators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        /// <summary>
        /// Splits a path by directory separators \ and / so that they can be iterated. Network drives starting with \\ are split after these characters.
        /// </summary>
        /// <param name="path">The path to split.</param>
        /// <returns>An array of path parts.</returns>
        public static string[] SplitPath(string path)
        {
            if (path.Length == 0)
                return new string[] { "" };
            if (path[0] == Path.DirectorySeparatorChar || path[0] == Path.AltDirectorySeparatorChar) {
                List<string> pathParts = new List<string>();
                int indexOfSep;
                if (path.Length > 1 && (path[1] == Path.DirectorySeparatorChar || path[1] == Path.AltDirectorySeparatorChar)) {
                    indexOfSep = path.IndexOfAny(DirSeparators, 2);
                    if (indexOfSep > 2)
                        indexOfSep = path.IndexOfAny(DirSeparators, indexOfSep + 1);
                }
                else
                    indexOfSep = path.IndexOfAny(DirSeparators, 1);
                if (indexOfSep > 0) {
                    pathParts.Add(path.Substring(0, indexOfSep));
                    pathParts.AddRange(path.Substring(indexOfSep + 1).Split(DirSeparators));
                    return pathParts.ToArray();
                }
                else
                    return new string[] { path };
            }
            return path.Split(DirSeparators);
        }

        /// <summary>
        /// Gets a List of directories matching a search pattern.
        /// </summary>
        /// <param name="pathParts">The path parts returned by <see cref="SplitPath"/>.</param>
        /// <returns>A List of directories matching the search pattern.</returns>
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
        /// Recursively gets a List of directories matching a search pattern.
        /// </summary>
        /// <param name="dir">The directory to match from.</param>
        /// <param name="pathParts">The path parts returned by <see cref="SplitPath"/></param>
        /// <param name="index">The index into path parts.</param>
        /// <param name="infos">The directories to append to.</param>
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
