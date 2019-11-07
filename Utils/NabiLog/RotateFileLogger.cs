using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bryllite.Utils.NabiLog
{
    public class RotateFileLogger : FileLogger
    {
        public RotateFileLogger(string logPath) : base(logPath)
        {
        }

        protected override string GetLogFilePath()
        {
            string fileName = $"{Path.GetFileNameWithoutExtension(logPath)}-{Log.DateCode}{Path.GetExtension(logPath)}";
            string path = Path.GetDirectoryName(logPath);
            return string.IsNullOrEmpty(path) ? fileName : $"{path}/{fileName}";
        }
    }
}
