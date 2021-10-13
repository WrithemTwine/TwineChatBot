using StreamerBot.Enum;

using System;
using System.Globalization;
using System.IO;

namespace StreamerBot.Static
{
    public static class LogWriter
    {
        private const string StatusLog = "StatusLog.txt";
        private const string ExceptionLog = "ExceptionLog.txt";

        public static void WriteLog(LogType ChooseLog, string line)
        {
            StreamWriter s = new(ChooseLog == LogType.LogBotStatus ? StatusLog : ChooseLog == LogType.LogExceptions ? ExceptionLog : "", true);
            s.WriteLine(line);
            s.Close();
        }

        public static void LogException(Exception ex, string Method)
        {
            if (OptionFlags.LogExceptions)
            {
                WriteLog(LogType.LogExceptions, DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture) + " " + Method);
                WriteLog(LogType.LogExceptions, ex.Message + "\nStack Trace: " + ex.StackTrace);
            }
        }
    }
}
