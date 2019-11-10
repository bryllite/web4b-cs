using Bryllite.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Bryllite.Utils.NabiLog
{
    public enum LogLevel
    {
        Panic = 0x01,
        Error = 0x02,
        Warning = 0x04,
        Debug = 0x08,
        Trace = 0x10,
        Info = 0x20,
        Verb = 0x40,

        None = 0x00,
        Product = Panic | Error | Warning | Info | Verb,
        All = 0xff,
    }

    /// <summary>
    /// A colorized & chained static log Module
    /// "Nabi" is cat's name of sang-woo shin, Bryllite Team
    /// </summary>
    public class Log
    {
        public static readonly string PANIC = "panic";
        public static readonly string ERROR = "error";
        public static readonly string WARNING = "warning";
        public static readonly string DEBUG = "debug";
        public static readonly string TRACE = "trace";
        public static readonly string VERB = "verb";
        public static readonly string INFO = "info";

        public static readonly string EOL = "\r\n";

        public static string DateCode => DateTime.Now.ToString("yyyy-MM-dd");
        public static string TimeCode => DateTime.Now.ToString("HH:mm:ss.fff");
        public static string DateTimeCode => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // log listeners
        private static List<INabiLog> listeners = new List<INabiLog>();

        // log lock object
        public static object Lock = new object();

        // LogFilter
        // WriteLine(), Write() 와는 관계 없다.
        // Panic(), Error(), Warning(), Debug(), Trace(), Verb(), Info() 메소드에 대한 적용 여부를 설정한다.
#if DEBUG
        public static LogLevel Filter = LogLevel.All;
        public static readonly bool needFileInfo = true;
#else
        public static LogLevel Filter = LogLevel.Product;
        public static readonly bool needFileInfo = false;
#endif

        // callstacks
        public static (string fileName, int lineNumber, string moduleName, string className, string methodName)[] GetCallStacks(int depth = 0)
        {
            List<(string, int, string, string, string)> stack = new List<(string, int, string, string, string)>();

            foreach (var frame in new StackTrace(needFileInfo).GetFrames())
            {
                var frameMethod = frame.GetMethod();

                string fileName = frame.GetFileName();
                int lineNumber = frame.GetFileLineNumber();

                string moduleName = frameMethod.Module.ToString();
                string className = frameMethod.ReflectedType?.Name;
                string methodName = frameMethod.Name;

                // exclude this
                if (className != MethodBase.GetCurrentMethod().DeclaringType.Name)
                    stack.Add((fileName, lineNumber, moduleName, className, methodName));

                if (depth > 0 && stack.Count >= depth)
                    break;
            }

            return stack.ToArray();
        }

        public static (string fileName, int lineNumber, string moduleName, string className, string methodName) Caller
        {
            get
            {
                foreach (var frame in new StackTrace(true).GetFrames())
                {
                    var frameMethod = frame.GetMethod();

                    string className = frameMethod.ReflectedType?.Name;
                    if (className != MethodBase.GetCurrentMethod().DeclaringType.Name)
                        return (frame.GetFileName(), frame.GetFileLineNumber(), frameMethod.Module.ToString(), className, frameMethod.Name);
                }

                throw new Exception("caller not exits!");
            }
        }


        static Log()
        {
            // console attach ( default )
            if (BConsole.IsAttached)
                AddListener(BConsole.Instance);

            // debug console attach if debugger attached
            if (Debugger.IsAttached)
                AddListener(DebugLogger.Instance);
        }

        public static IEnumerable<INabiLog> Listener
        {
            get
            {
                lock (listeners)
                    return listeners.ToArray();
            }
        }

        public static void AddListener(INabiLog logger)
        {
            lock (listeners)
                listeners.Add(logger);
        }

        public static void RemoveListener(INabiLog logger)
        {
            lock (listeners)
                listeners.Add(logger);
        }

        public static void Write(params object[] args)
        {
            lock (Lock)
            {
                foreach (var logger in Listener)
                    logger.WriteLog(args);
            }
        }

        public static void WriteLine(params object[] args)
        {
            Write(args.Append(EOL));
        }

        public static void WriteIf(bool condition, params object[] args)
        {
            if (condition) Write(args);
        }

        public static void WriteLineIf(bool condition, params object[] args)
        {
            if (condition) WriteLine(args);
        }

        public static void Panic(params object[] args)
        {
            // Panic은 filter 정보를 무시하고 항상 실행한다.
            //if ((Filter & LogLevel.Panic) == 0) return;

            lock (Lock)
            {
                WriteTime();
                WriteType(PANIC, Color.DarkRed);
                Write(args); Write(EOL);
                WriteCallStack();

                // panic일때는 프로세스를 종료한다.
                Environment.Exit(-1);
            }
        }

        public static void Error(params object[] args)
        {
            if ((Filter & LogLevel.Error) == 0) return;

            lock (Lock)
            {
                WriteTime();
                WriteType(ERROR, Color.Red);
                Write(args); Write(EOL);
                WriteCallStack();

                // 에러일때는 예외 발생
                throw new Exception(BuildString(args));
            }
        }

        public static void Warning(params object[] args)
        {
            if ((Filter & LogLevel.Warning) == 0) return;

            lock (Lock)
            {
                WriteTime();
                WriteType(WARNING, Color.DarkYellow);
                Write(args); Write(EOL);
            }
        }

        public static void Debug(params object[] args)
        {
            if ((Filter & LogLevel.Debug) == 0) return;

            lock (Lock)
            {
                WriteTime();
                WriteType(DEBUG, Color.Gray);
                Write(args); Write(EOL);
            }
        }

        public static void Trace(params object[] args)
        {
            if ((Filter & LogLevel.Trace) == 0) return;

            lock (Lock)
            {
                WriteTime();
                WriteType(TRACE, Color.White);
                Write(args); Write(EOL);
            }
        }

        public static void Verb(params object[] args)
        {
            if ((Filter & LogLevel.Verb) == 0) return;

            lock (Lock)
            {
                WriteTime();
                WriteType(VERB, Color.Cyan);
                Write(args); Write(EOL);
            }
        }

        public static void Info(params object[] args)
        {
            if ((Filter & LogLevel.Info) == 0) return;

            lock (Lock)
            {
                WriteTime();
                WriteType(INFO, Color.DarkCyan);
                Write(args); Write(EOL);
            }
        }

        // 파라미터로 받은 객체 배열을 문자열로 변환한다.
        // 색상 정보는 제외한다.
        // null 은 "Null" 로, true는 "True"로, false는 "False" 로 변환된다.
        internal static string BuildString(params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var arg in args)
            {
                if (arg is Color || arg is ConsoleColor) continue;

                if (ReferenceEquals(arg, null)) sb.Append("Null");
                else if (arg is byte[] bytes) sb.Append(bytes.ToHexString());
                else sb.Append(arg.ToString());
            }
            return sb.ToString();
        }

        private static void WriteTime()
        {
            Write("[", Color.DarkGreen, TimeCode, "] ");
        }

        private static void WriteType(string type, Color color)
        {
            var caller = Caller;
            string tag = $"{caller.className}.{caller.methodName}";

            Write(color, type, "/", Color.DarkGray, tag, ": ");
        }

        private static void WriteCallStack(int depth = 0)
        {
            WriteLine("callstack={");
            foreach (var callstack in GetCallStacks(depth))
                WriteLine(" ", Color.DarkGray, callstack.className, ".", Color.DarkGray, callstack.methodName, "() (", Color.DarkGray, callstack.moduleName, ")");
            WriteLine("}");
        }
    }
}
