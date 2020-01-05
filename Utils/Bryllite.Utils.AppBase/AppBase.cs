using Bryllite.Extensions;
using Bryllite.Utils.NabiLog;
using Bryllite.Utils.Ntp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Utils.AppBase
{
    // command line interpreter
    public delegate void CommandLineInterpreter(string[] args);

    public abstract class AppBase
    {
        public static readonly string TITLE = "Title";

        // appsettings configuration filename
        public static readonly string APPSETTINGS_DEV = "config/appsettings.development.json";
        
        // appsettings configuration filename
        public static readonly string APPSETTINGS = "config/appsettings.json";

        // args
        protected CommandLineParser args;
        public CommandLineParser Args => args;

        // config
        protected readonly JObject config;
        public JObject Config => config;

        // environment variables
        protected readonly JObject env;
        public JObject Env => env;

        // cancellation token
        protected CancellationTokenSource cts = new CancellationTokenSource();

        // app exit code
        protected int exitCode = 0;

        // console enabled?
        public bool console;

        // command seperators
        //protected static readonly char[] seperators = { ' ', '(', ')', '[', ']', '=', ',', ';', ':', '\'', '\"' };
        protected static readonly char[] seperators = { ' ', '(', ')', '[', ']', ',', '\'', '\"' };

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

        public AppBase(string[] arguments)
        {
            // command line
            args = new CommandLineParser(arguments);

            // console enabled?
            console = args.Contains("console");

            // configuration
#if DEBUG
            string cfg = args.Value("config", File.Exists(APPSETTINGS_DEV) ? APPSETTINGS_DEV : APPSETTINGS);
#else
            string cfg = args.Value("config", APPSETTINGS);
#endif

            if (File.Exists(cfg))
            {
                // load config
                config = JObject.Parse(File.ReadAllText(cfg));

                // set title
                string title = config.GetValue("title", StringComparison.OrdinalIgnoreCase)?.Value<string>();
                Console.Title = !string.IsNullOrEmpty(title) ? title : Assembly.GetEntryAssembly().GetName().Name;
            }

            // environment variables
            env = new JObject();
            foreach (DictionaryEntry kv in Environment.GetEnvironmentVariables())
                env.Add(kv.Key.ToString(), kv.Value.ToString());

            // default commands
            MapCommandHandler("exit, quit, shutdown", "close application", OnCommandExit);
            MapCommandHandler("h, help", "show command list", OnCommandShowHelp);
            MapCommandHandler("cls", "clear screen", OnCommandClearScreen);
            MapCommandHandler("config", "show config values", OnCommandShowConfig);
            MapCommandHandler("env", "show environment values", OnCommandShowEnv);
            MapCommandHandler("args", "show command line args", OnCommandShowArgs);
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
            Log.Info("Hello, ", Assembly.GetEntryAssembly().GetName().Name, "!");

            // ctrl+c
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                Stop(1);
            };

            try
            {
                // map command handlers
                OnMapCommandHandlers();

                // app initialize
                if (!OnAppInitialize())
                    return (exitCode = -1);

                // do command line process
                OnMain();

                // app cleanup
                OnAppCleanup();

                return exitCode;
            }
            catch (Exception ex)
            {
                Log.Warning("Exception! ex=", ex);
                return (exitCode = -99);
            }
            finally
            {
                Log.Info("Bye, ", Assembly.GetEntryAssembly().GetName().Name, "!");
            }
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
                if (string.IsNullOrEmpty(cmd)) continue;

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

        public virtual void OnMapCommandHandlers()
        {
        }


        protected void OnMain()
        {
            // console options
            if (console)
            {
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
                    catch (Exception ex)
                    {
                        BConsole.WriteLine(Color.Red, "console exception! ex.Message=", ex.Message);
                    }
                }
            }
            else
            {
                while (!cts.IsCancellationRequested)
                    Thread.Sleep(10);
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

        protected virtual void OnCommandShowEnv(string[] args)
        {
            BConsole.WriteLine(args.Length > 0 ? env.GetValue(args[0], StringComparison.OrdinalIgnoreCase)?.Value<string>() : env.ToString());
        }

        protected virtual void OnCommandShowArgs(string[] args)
        {
            bool json = args.Length > 0 ? Convert.ToBoolean(args[0]) : true;

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
            time.Put<bool>("synchronized", NetTime.Synchronized);
            time.Put<string>("provider", NetTime.ActiveTimeServer);
            time.Put<DateTime>("utc", NetTime.UtcNow);
            time.Put<DateTime>("now", DateTime.Now);
            time.Put<int>("unixtime", NetTime.UnixTime);
            time.Put<TimeSpan>("timediff", NetTime.TimeDiff);

            return time;
        }
    }
}
