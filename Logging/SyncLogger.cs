using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Utilities.Logging
{
    public class SyncLogger : ILogger
    {
        private readonly object l = new object();
        private readonly Logger logger = new Logger();

        public SyncLogger() { }

        public SyncLogger(TextWriter writer)
        {
            logger.Add(writer);
        }

        public static SyncLogger Instance { get; } = new SyncLogger(Console.Error);

        public LogStyle DefaultStyle {
            get => logger.DefaultStyle;
            set {
                lock (l) {
                    logger.DefaultStyle = value;
                }
            }
        }

        public LogAction CustomStyle {
            get => logger.CustomStyle;
            set {
                lock (l) {
                    logger.CustomStyle = value;
                }
            }
        }

        public TextWriter Add()
        {
            lock (l) {
                return logger.Add();
            }
        }

        public TextWriter Add(string path)
        {
            lock (l) {
                return logger.Add(path);
            }
        }

        public bool Add(TextWriter writer)
        {
            lock (l) {
                return logger.Add(writer);
            }
        }

        public void Log(string message, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            lock (l) {
                logger.Log(message, methodName, filePath, lineNumber);
                logger.Flush();
            }
        }

        public void Log(LogStyle style, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            lock (l) {
                logger.Log(style, methodName, filePath, lineNumber);
                logger.Flush();
            }
        }

        public void Log(string message, LogStyle style, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            lock (l) {
                logger.Log(message, style, methodName, filePath, lineNumber);
                logger.Flush();
            }
        }

        public void Log(string message, LogAction logAction, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            lock (l) {
                logger.Log(message, logAction, methodName, filePath, lineNumber);
                logger.Flush();
            }
        }

        public bool Remove(TextWriter writer)
        {
            lock (l) {
                return logger.Remove(writer);
            }
        }

        public void Write(string message)
        {
            lock (l) {
                logger.Write(message);
                logger.Flush();
            }
        }

        public void WriteLine()
        {
            lock (l) {
                logger.WriteLine();
                logger.Flush();
            }
        }

        public void WriteLine(string message)
        {
            lock (l) {
                logger.WriteLine(message);
                logger.Flush();
            }
        }

        public void PrintStackTrace(string message)
        {
            lock (l) {
                logger.PrintStackTrace(message);
                logger.Flush();
            }
        }

        public void PrintStackTrace()
        {
            lock (l) {
                logger.PrintStackTrace();
                logger.Flush();
            }
        }

        public void Flush()
        {
            // pointless -- flush is automatic
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
    }
}
