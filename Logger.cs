using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Utilities.Log
{
    public enum LogStyle
    {
        MethodFileLine = 0,
        MessageOnly = 1,
        DateTime = 2,
        DateTimeMethodFileLine = 3,
        Custom = 4,
    }

    public delegate void LogAction(TextWriter writer, string message, string methodName, string fileName, int lineNumber);

    public interface ILogger
    {
        LogStyle DefaultStyle { get; set; }
        LogAction CustomStyle { get; set; }
        TextWriter Add();
        TextWriter Add(string path);
        bool Add(TextWriter writer);
        bool Remove(TextWriter writer);
        void Log(
            string message,
            LogAction logAction,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0);

        void Log(
            string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0);

        void Log(
            LogStyle style,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0);

        void Log(
            string message,
            LogStyle style,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0);

        void Write(string message);
        void WriteLine();
        void WriteLine(string message);
        void PrintStackTrace();
        void PrintStackTrace(string message);
        void Flush();
    }

    public class Logger : ILogger
    {
        protected List<TextWriter> Writers { get; set; } = new List<TextWriter>();
        protected LogAction[] LogStyleActions { get; set; }

        public Logger()
        {
            LogStyleActions = new LogAction[] {
                Log_MessageOnly,
                Log_MethodFileLine,
                Log_DateTime,
                Log_DateTimeMethodFileLine,
                Log_CustomDefault,
            };
        }

        public Logger(TextWriter writer) : this()
        {
            Add(writer);
        }

        public static Logger Instance { get; } = new Logger(Console.Error);
        public LogStyle DefaultStyle { get; set; }

        public LogAction CustomStyle {
            get => LogStyleActions[(int) LogStyle.Custom];
            set => LogStyleActions[(int) LogStyle.Custom] = value ?? Log_CustomDefault;
        }

        public TextWriter Add()
        {
            FileStream fs = new FileStream(".\\" + DateTime.Now.ToString("MM-dd-yyyy HH_mm_ss"), FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fs);
            Add(writer);
            return writer;
        }

        public TextWriter Add(string path)
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fs);
            Add(writer);
            return writer;
        }

        public bool Add(TextWriter writer)
        {
            if (writer != null) {
                writer.Flush();
                Writers.Add(writer);
                return true;
            }
            return false;
        }

        public bool Remove(TextWriter writer)
        {
            return writer != null && Writers.Remove(writer);
        }

        public void Log(
            string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Log(message, LogStyleActions[(int) DefaultStyle], methodName, filePath, lineNumber);
        }

        public void Log(
            LogStyle style,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Log(null, LogStyleActions[(int) style], methodName, filePath, lineNumber);
        }

        public void Log(
            string message,
            LogStyle style,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Log(message, LogStyleActions[(int) style], methodName, filePath, lineNumber);
        }

        public void Write(string message)
        {
            foreach (TextWriter writer in Writers) {
                writer.Write(message);
            }
        }

        public void WriteLine()
        {
            foreach (TextWriter writer in Writers) {
                writer.WriteLine();
            }
        }

        public void WriteLine(string message)
        {
            foreach (TextWriter writer in Writers) {
                writer.WriteLine(message);
            }
        }

        public void PrintStackTrace(string message)
        {
            foreach (TextWriter writer in Writers) {
                writer.WriteLine(message);
                writer.WriteLine(System.Environment.StackTrace);
            }
        }

        public void PrintStackTrace()
        {
            foreach (TextWriter writer in Writers) {
                writer.WriteLine(System.Environment.StackTrace);
            }
        }

        public void Flush()
        {
            foreach (TextWriter writer in Writers) {
                writer.Flush();
            }
        }

        public void FlushAsync()
        {
            foreach (TextWriter writer in Writers) {
                writer.FlushAsync();
            }
        }

        public static int Line([CallerLineNumber] int lineNumber = 0)
        {
            return lineNumber;
        }

        public static string File([CallerFilePath] string filePath = "")
        {
            return filePath;
        }

        public static string Method([CallerMemberName] string methodName = "")
        {
            return methodName;
        }

        public static string StackTrace => System.Environment.StackTrace;

        public void Log(string message, LogAction logAction, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            foreach (TextWriter writer in Writers) {
                logAction(writer, message, methodName, filePath, lineNumber);
            }
        }

        /*
        public void Log2(string message = null)
        {
            StackFrame stackFrame = new System.Diagnostics.StackTrace(1).GetFrame(1);
            string methodName = stackFrame.GetMethod().ToString();
            string filePath = stackFrame.GetFileName();
            int lineNumber = stackFrame.GetFileLineNumber();

            foreach (var writer in writers) {
                writer.WriteLine("{0}({1}:{2})", methodName, Path.GetFileName(filePath), lineNumber);
                if (message != null && message.Length > 0) {
                    writer.WriteLine(message);
                }
            }
        }
        */

        #region Private Data
        private static void Log_MessageOnly(
            TextWriter writer,
            string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (message != null) {
                writer.WriteLine(message);
            }
        }

        private static void Log_MethodFileLine(
            TextWriter writer,
            string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            writer.WriteLine("{0}({1}:{2})}", methodName, Path.GetFileName(filePath), lineNumber);
            if (message != null && message.Length > 0) {
                writer.WriteLine(message);
            }
        }

        private static void Log_DateTime(
            TextWriter writer,
            string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            writer.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
            if (message != null && message.Length > 0) {
                writer.WriteLine(message);
            }
        }

        private static void Log_DateTimeMethodFileLine(
            TextWriter writer,
            string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            writer.WriteLine("[{0,-19}] {1}({2}:{3})}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), methodName, Path.GetFileName(filePath), lineNumber);
            if (message != null && message.Length > 0) {
                writer.WriteLine(message);
            }
        }

        private static void Log_CustomDefault(
            TextWriter writer,
            string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        { }
        #endregion
    }
}