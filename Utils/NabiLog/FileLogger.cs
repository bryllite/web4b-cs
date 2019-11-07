using Bryllite.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bryllite.Utils.NabiLog
{
    public class FileLogger : INabiLog
    {
        // log file path
        protected string logPath;

        public FileLogger(string logPath)
        {
            this.logPath = logPath;

            logPath.MakeSureDirectoryPathExists();
        }

        protected virtual string GetLogFilePath()
        {
            return logPath;
        }

        public void WriteLog(params object[] args)
        {
            lock (this)
                File.AppendAllText(GetLogFilePath(), Log.BuildString(args));
        }
    }
}
