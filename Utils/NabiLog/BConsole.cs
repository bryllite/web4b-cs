using System;
using System.Linq;

namespace Bryllite.Utils.NabiLog
{
    public enum Color
    {
        Black = ConsoleColor.Black,
        DarkBlue = ConsoleColor.DarkBlue,
        DarkGreen = ConsoleColor.DarkGreen,
        DarkCyan = ConsoleColor.DarkCyan,
        DarkRed = ConsoleColor.DarkRed,
        DarkMagenta = ConsoleColor.DarkMagenta,
        DarkYellow = ConsoleColor.DarkYellow,
        Gray = ConsoleColor.Gray,
        DarkGray = ConsoleColor.DarkGray,
        Blue = ConsoleColor.Blue,
        Green = ConsoleColor.Green,
        Cyan = ConsoleColor.Cyan,
        Red = ConsoleColor.Red,
        Magenta = ConsoleColor.Magenta,
        Yellow = ConsoleColor.Yellow,
        White = ConsoleColor.White,
        Default = Gray
    }

    public class BConsole : INabiLog
    {
        public static string EOL => Log.EOL;
        public static readonly BConsole Instance = new BConsole();

        // text color
        public static ConsoleColor TextColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }

        // background color
        public static ConsoleColor BackColor
        {
            get { return Console.BackgroundColor; }
            set { Console.BackgroundColor = value; }
        }

        // console title
        public static string Title
        {
            get { return Console.Title; }
            set { Console.Title = value; }
        }

        private BConsole()
        {
        }

        public static bool IsAttached
        {
            get
            {
                try
                {
                    return Environment.UserInteractive && Console.Title.Length > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static void Clear()
        {
            lock (Instance)
                Console.Clear();
        }

        public static void Beep()
        {
            Console.Beep();
        }

        public static void Beep(int frequency, int duration)
        {
            Console.Beep(frequency, duration);
        }

        public static ConsoleKeyInfo Pause(params object[] args)
        {
            if (args.Length > 0)
                WriteLine(args);
            else WriteLine("press any key to continue...");

            return Console.ReadKey();
        }

        public static string ReadLine()
        {
            return ReadLine(string.Empty);
        }


        public static string ReadLine(string prompts)
        {
            // write prompts
            if (!string.IsNullOrEmpty(prompts))
                Write(prompts);

            return Console.ReadLine();
        }

        public static string ReadPassword()
        {
            return ReadPassword("password: ");
        }

        public static string ReadPassword(string prompts)
        {
            // write prompts
            if (!string.IsNullOrEmpty(prompts))
                Write(prompts);

            string password = string.Empty;
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        return null;
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        return password;
                    case ConsoleKey.Backspace:
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                        break;
                    default:
                        password += key.KeyChar;
                        Console.Write("*");
                        break;
                }
            }
        }

        public static void ResetColor()
        {
            lock (Instance)
                Console.ResetColor();
        }

        public void WriteLog(params object[] args)
        {
            Write(args);
        }

        public static void Write(params object[] args)
        {
            lock (Instance)
            {
                foreach (var arg in args)
                {
                    if (arg is ConsoleColor cr)
                    {
                        TextColor = cr;
                        continue;
                    }

                    if (arg is Color color)
                    {
                        TextColor = (ConsoleColor)color;
                        continue;
                    }

                    if (ReferenceEquals(arg, null)) Console.Write("Null");
                    else if (arg is byte[] bytes) Console.Write(Hex.ToString(bytes));
                    else Console.Write(arg.ToString());

                    ResetColor();
                }
            }
        }

        public static void WriteLine(params object[] args)
        {
            Write(args.Concat(new object[] { EOL }).ToArray());
        }

        public static void WriteIf(bool condition, params object[] args)
        {
            if (condition) Write(args);
        }

        public static void WriteLineIf(bool condition, params object[] args)
        {
            if (condition) WriteLine(args);
        }

        public static void WriteFormat(string format, params object[] args)
        {
            lock (Instance)
                Console.WriteLine(format, args);
        }

    }
}
