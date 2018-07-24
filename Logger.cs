using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

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
        void Log(string message, LogAction logAction,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0);

        void Log(string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0);

        void Log(LogStyle style,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0);

        void Log(string message,
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
        private static Logger singleton = new Logger(Console.Error);
        protected LogAction customStyle = _P.Log_CustomDefault;
        protected List<TextWriter> writers = new List<TextWriter>();
        protected LogAction[] LogStyleActions;

        public Logger() { _Initialize(); }

        public Logger(TextWriter writer)
        {
            LogStyleActions = new LogAction[] {
                _P.Log_MessageOnly,
                _P.Log_MethodFileLine,
                _P.Log_DateTime,
                _P.Log_DateTimeMethodFileLine,
                customStyle,
            };
            Add(writer);
        }

        public static Logger Instance {
            get { return singleton; }
        }

        public LogStyle DefaultStyle { get; set; }

        public LogAction CustomStyle {
            get {
                return LogStyleActions[(int) LogStyle.Custom];
            }
            set {
                LogStyleActions[(int) LogStyle.Custom] = value ?? customStyle;
            }
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
                writers.Add(writer);
                return true;
            }
            return false;
        }

        public bool Remove(TextWriter writer)
        {
            return writer != null && writers.Remove(writer);
        }

        public void Log(string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Log(message, LogStyleActions[(int) DefaultStyle], methodName, filePath, lineNumber);
        }

        public void Log(LogStyle style,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Log(null, LogStyleActions[(int) style], methodName, filePath, lineNumber);
        }

        public void Log(string message,
            LogStyle style,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Log(message, LogStyleActions[(int) style], methodName, filePath, lineNumber);
        }

        public void Write(string message)
        {
            foreach (var writer in writers) {
                writer.Write(message);
            }
        }

        public void WriteLine()
        {
            foreach (var writer in writers) {
                writer.WriteLine();
            }
        }

        public void WriteLine(string message)
        {
            foreach (var writer in writers) {
                writer.WriteLine(message);
            }
        }

        public void PrintStackTrace(string message)
        {
            foreach (var writer in writers) {
                writer.WriteLine(message);
                writer.WriteLine(System.Environment.StackTrace);
            }
        }

        public void PrintStackTrace()
        {
            foreach (var writer in writers) {
                writer.WriteLine(System.Environment.StackTrace);
            }
        }

        public void Flush()
        {
            foreach (var writer in writers) {
                writer.Flush();
            }
        }

        public void FlushAsync()
        {
            foreach (var writer in writers) {
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

        public static string StackTrace {
            get { return System.Environment.StackTrace; }
        }

        public void Log(string message, LogAction logAction, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            foreach (var writer in writers) {
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
        private static class _P
        {
            public static void Log_MessageOnly(TextWriter writer, string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
            {
                if (message != null) {
                    writer.WriteLine(message);
                }
            }

            public static void Log_MethodFileLine(TextWriter writer, string message,
                [CallerMemberName] string methodName = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0)
            {
                writer.WriteLine("{0}({1}:{2})}", methodName, Path.GetFileName(filePath), lineNumber);
                if (message != null && message.Length > 0) {
                    writer.WriteLine(message);
                }
            }

            public static void Log_DateTime(TextWriter writer, string message,
                [CallerMemberName] string methodName = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0)
            {
                writer.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                if (message != null && message.Length > 0) {
                    writer.WriteLine(message);
                }
            }

            public static void Log_DateTimeMethodFileLine(TextWriter writer, string message,
                [CallerMemberName] string methodName = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0)
            {
                writer.WriteLine("[{0,-19}] {1}({2}:{3})}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), methodName, Path.GetFileName(filePath), lineNumber);
                if (message != null && message.Length > 0) {
                    writer.WriteLine(message);
                }
            }

            public static void Log_CustomDefault(TextWriter writer, string message,
                [CallerMemberName] string methodName = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0)
            { }
        }
        #endregion
    }
}