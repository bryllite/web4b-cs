using Bryllite.Utils.NabiLog;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.App.Sample
{
    // command line interpreter
    public delegate void CommandLineInterpreter(string[] args);

    public abstract class AppBase
    {
        // appsettings configuration filename
        public static readonly string APPSETTINGS = "appsettings.json";

        // args
        protected string[] args;

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
            this.args = args;

            // load config
            if (File.Exists(APPSETTINGS))
            {
                config = JObject.Parse(File.ReadAllText(APPSETTINGS));
                Console.Title = config.Value<string>("title");
            }

            // default commands
            MapCommandHandler("exit, quit, shutdown", "프로그램을 종료합니다", OnCommandExit);
            MapCommandHandler("h, help", "도움말을 출력합니다", OnCommandShowHelp);
            MapCommandHandler("cls", "콘솔 화면의 내용을 모두 지웁니다", OnCommandClearScreen);
            MapCommandHandler("config", "설정 정보를 출력합니다", OnCommandShowConfig);
        }

        public int Run(object context = null)
        {
            // app initialize
            if (!OnAppInitialize())
                return -1;

            // ctrl+c
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                Stop(1);
            };

            // do command line process
            OnMain();

            // app cleanup
            OnAppCleanup();

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
                lock(handlers)
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
    }
}
