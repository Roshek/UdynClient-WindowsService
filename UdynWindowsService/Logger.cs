using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UdynWindowsService
{
    public enum LogLevel
    {
        MUTE = 0,
        FATAL,
        ERROR,
        WARNING,
        INFO,
        DEBUG,
        TOTAL
    }

    static class Logger
    {
        public static LogLevel LogLevel = LogLevel.INFO;
        public static string LogPath = "noPath";

        public static void Log(string logMessage, LogLevel level = LogLevel.INFO)
        {
            if (LogLevel < level)
                return;
            using (StreamWriter w = File.AppendText(LogPath))
            {
                w.Write("Log level : [" + level.ToString() + "] -- Time : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine(": {0}", logMessage);
                w.WriteLine("\r\n");
            }
        }
    }
}
