using StreamerBotLib.Enums;

using System;
using System.Globalization;
using System.IO;

namespace StreamerBotLib.Static
{
    public static class LogWriter
    {
        private const string StatusLog = "StatusLog.txt";
        private const string ExceptionLog = "ExceptionLog.txt";
        private const string DataActLog = "DataActionLog.txt";

        public static void WriteLog(LogType ChooseLog, string line)
        {
            lock (StatusLog)
            {
                StreamWriter s = new(ChooseLog == LogType.LogBotStatus ? StatusLog : ChooseLog == LogType.LogExceptions ? ExceptionLog : "", true);
                s.WriteLine(line);
                s.Close();
            }
        }

        public static void LogException(Exception ex, string Method)
        {
            lock (ExceptionLog)
            {
                if (OptionFlags.LogExceptions)
                {
                    WriteLog(LogType.LogExceptions, $"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)} {Method} {ex.GetType()}");
                    WriteLog(LogType.LogExceptions, $"{ex.Message}\nStack Trace: {ex.StackTrace}");
                }
            }
        }

        public static void DataActionLog(string Method, string line)
        {
            lock (DataActLog)
            {
                StreamWriter s = new(DataActLog, true);
                s.WriteLine($"{DateTime.Now.ToLocalTime()} {Method} {line}");
                s.Close();
            }
        }
    }
}
