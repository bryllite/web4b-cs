using Bryllite.Extensions;
using Bryllite.Utils.NabiLog;
using Bryllite.Utils.Ntp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Utils.AppBase
{
    // command line interpreter
    public delegate void CommandLineInterpreter(string[] args);

    public abstract class AppBase
    {
        // appsettings configuration filename
        public static readonly string APPSETTINGS_DEV = "appsettings.development.json";
        
        // appsettings configuration filename
        public static readonly string APPSETTINGS = "appsettings.json";

        // args
        protected CommandLineParser args;

        // config
        protected readonly JObject config;

        // cancellation token
        protected CancellationTokenSource cts = new CancellationTokenSource();

        // app exit code
        protected int exitCode = 0;

        // command seperators
        protected static readonly char[] seperators = { ' ', '(', ')', '[', ']', '=', ',', ';', ':', '\'', '\"' };

        public string Prompts = "> ";
        public Color PromptsTextColor = Color.DarkGray;

        // command handler table
        private Dictionary<string, CommandLineInterpreter> handlers = new Dictionary<string, CommandLineInterpreter>();

        // command help message table
        private Dictionary<string, string> descriptions = new Dictionary<string, string>();
        public IEnumerable<KeyValuePair<string, string>> Descriptions
        {
            get
            {
                lock (descriptions)
                    return descriptions.ToList();
            }
        }

        // max commands length to decide desc pos
        private int maxCommandsLength = 0;

        public AppBase(string[] args)
        {
            this.args = new CommandLineParser(args);

#if DEBUG
            string json = File.Exists(APPSETTINGS_DEV) ? File.ReadAllText(APPSETTINGS_DEV) : File.Exists(APPSETTINGS) ? File.ReadAllText(APPSETTINGS) : null;
#else
            string json = File.Exists(APPSETTINGS) ? File.ReadAllText(APPSETTINGS) : null;
#endif
            if (!string.IsNullOrEmpty(json))
            {
                // load config
                config = JObject.Parse(json);

                if (config.ContainsKey("title"))
                    Console.Title = config.Value<string>("title");
            }

            // default commands
            MapCommandHandler("exit, quit, shutdown", "close application", OnCommandExit);
            MapCommandHandler("h, help", "show command list", OnCommandShowHelp);
            MapCommandHandler("cls", "clear screen", OnCommandClearScreen);
            MapCommandHandler("config", "show config values", OnCommandShowConfig);
            MapCommandHandler("args", "show command args", OnCommandShowArgs);
            MapCommandHandler("time", "show current time information", OnCommandShowTime);
            MapCommandHandler("time.sync", "synchronize time", OnCommandSyncTime);

            // synchronize network time
            Task.Run(() =>
            {
                if (!NetTime.Synchronize())
                    Log.WriteLine("[", Color.DarkRed, "FAIL", "] NetTime.Synchronize()...");
            });
        }

        public int Run(object context = null)
        {
            // ctrl+c
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                Stop(1);
            };

            try
            {
                // app initialize
                if (!OnAppInitialize())
                    return -1;

                // do command line process
                OnMain();

                // app cleanup
                OnAppCleanup();
            }
            catch (Exception ex)
            {
                Log.Warning("Exception! ex=", ex);
                return -99;
            }

            return exitCode;
        }

        public Task<int> RunAsync(object context = null)
        {
            return Task.Run(() =>
            {
                return Run(context);
            });
        }

        // stop
        public void Stop(int exitCode = 0)
        {
            if (!cts.IsCancellationRequested)
            {
                this.exitCode = exitCode;
                cts.Cancel();
            }
        }

        // app 초기화될 때 실행
        public virtual bool OnAppInitialize()
        {
            return true;
        }

        // app 종료될 때 실행
        public virtual void OnAppCleanup()
        {
        }

        protected CommandLineInterpreter GetHandler(string command)
        {
            string cmd = command.ToLower();
            lock (handlers)
                return handlers.ContainsKey(cmd) ? handlers[cmd] : null;
        }

        protected void MapCommandHandler(string commands, string description, CommandLineInterpreter handler)
        {
            var cmds = commands.Split(seperators);
            foreach (var cmd in cmds)
            {
                lock (handlers)
                    handlers[cmd.ToLower()] = handler;
            }

            lock (descriptions)
                descriptions[commands] = description;

            maxCommandsLength = Math.Max(maxCommandsLength, commands.Length);
        }

        protected void MapCommandHandler(string command, CommandLineInterpreter handler)
        {
            MapCommandHandler(command, string.Empty, handler);
        }


        protected void OnMain()
        {
            Thread.Sleep(100);

            BConsole.WriteLine();
            BConsole.WriteLine(Color.White, "Welcome to the Bryllite console!");

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    // show prompts
                    ShowPrompts();

                    // intput stream
                    string input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input)) continue;

                    // read console input
                    string[] tokens = input.Trim().Split(seperators);
                    if (tokens.Length == 0) continue;

                    // command( operator )
                    string command = tokens[0];
                    if (command.Length == 0) continue;

                    // arguments
                    List<string> args = new List<string>();
                    foreach (var arg in tokens.Skip(1).ToArray())
                        if (arg.Length > 0) args.Add(arg);

                    // invoke command handler
                    var handler = GetHandler(command);
                    if (!ReferenceEquals(handler, null))
                        handler.Invoke(args.ToArray());
                    else BConsole.WriteLine(Color.DarkYellow, "what? '", Color.DarkGreen, command, Color.DarkYellow, "' unknown!", " 'h' or 'help' for commands list");
                }
                catch (Exception e)
                {
                    BConsole.WriteLine(ConsoleColor.Red, "console exception! e=", e.Message);
                }
            }
        }

        protected void ShowPrompts()
        {
            ShowPrompts(Prompts);
        }

        protected void ShowPrompts(string prompts)
        {
            BConsole.Write(PromptsTextColor, prompts);
        }

        protected virtual void OnCommandExit(string[] args)
        {
            Stop();
        }

        protected virtual void OnCommandShowHelp(string[] args)
        {
            string format = $"{{0,-{maxCommandsLength}}}";
            foreach (var entry in Descriptions)
                BConsole.WriteLine(string.Format(format, entry.Key), " - ", Color.DarkGray, entry.Value);
        }

        protected virtual void OnCommandClearScreen(string[] args)
        {
            BConsole.Clear();
        }

        protected virtual void OnCommandShowConfig(string[] args)
        {
            BConsole.WriteLine(config);
        }

        protected virtual void OnCommandShowArgs(string[] args)
        {
            bool json = args.Length > 0 ? Convert.ToBoolean(args[0]) : false;

            if (json)
                BConsole.WriteLine(this.args.ToJObject());
            else BConsole.WriteLine(this.args.ToString());
        }

        protected virtual void OnCommandShowTime(string[] args)
        {
            BConsole.WriteLine("NetTime=", GetNetTime());
        }

        protected virtual void OnCommandSyncTime(string[] args)
        {
            Task.Run(() =>
            {
                NetTime.Synchronize();
                BConsole.WriteLine("NetTime=", GetNetTime());
            });
        }

        protected JObject GetNetTime()
        {
            var time = new JObject();
            time.Put("synchronized", NetTime.Synchronized);
            time.Put("provider", NetTime.ActiveTimeServer);
            time.Put("utc", NetTime.UtcNow);
            time.Put("now", DateTime.Now);
            time.Put("unixtime", NetTime.UnixTime);
            time.Put("timediff", NetTime.TimeDiff);

            return time;
        }
    }
}
