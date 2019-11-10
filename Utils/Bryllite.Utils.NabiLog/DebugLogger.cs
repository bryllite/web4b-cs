using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bryllite.Utils.NabiLog
{
    public class DebugLogger : INabiLog
    {
        public static readonly DebugLogger Instance = new DebugLogger();

        private DebugLogger()
        {
        }

        public void WriteLog(params object[] args)
        {
            lock (Instance)
                Debug.Write(Log.BuildString(args));
        }
    }
}
