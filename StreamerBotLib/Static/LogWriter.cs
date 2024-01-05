using StreamerBotLib.Enums;

using System;
using System.Globalization;
using System.IO;

namespace StreamerBotLib.Static
{
    /// <summary>
    /// Manages log output across the whole bot.
    /// </summary>
    public static class LogWriter
    {
        private const string StatusLog = "StatusLog.txt";
        private const string ExceptionLog = "ExceptionLog.txt";
        private const string DataActLog = "DataActionLog.txt";
        private const string OverlayLogFile = "OverlayLog.txt";

        private const string DebugLogFile = "DebugLogFile.txt";

        /// <summary>
        /// Write output to a specific log.
        /// </summary>
        /// <param name="ChooseLog">Choose which log to write the data.</param>
        /// <param name="line">The line content to write to the log.</param>
        public static void WriteLog(LogType ChooseLog, string line)
        {
            lock (StatusLog)
            {
                StreamWriter s;
                if (ChooseLog == LogType.LogBotStatus)
                {
                    s = new(StatusLog, true);
                }
                else // if (ChooseLog == LogType.LogExceptions)
                {
                    s = new(ExceptionLog, true);
                }

                s.WriteLine(line);
                s.Close();
            }
        }

        /// <summary>
        /// Specifically log caught exceptions. Internally adds 'DateTime.Now' to the log file output.
        /// </summary>
        /// <param name="ex">The exception caught in the app.</param>
        /// <param name="Method">Name of the method which caught the exception.</param>
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

        /// <summary>
        /// Logs actions performed for managing database information. Internally adds 'DateTime.Now' to the logfile output.
        /// </summary>
        /// <param name="Method">Name of the method logging the current action.</param>
        /// <param name="line">The information line of the performed action.</param>
        public static void DataActionLog(string Method, string line)
        {
            lock (DataActLog)
            {
                StreamWriter s = new(DataActLog, true);
                s.WriteLine($"{DateTime.Now.ToLocalTime()} {Method} {line}");
                s.Close();
            }
        }

        /// <summary>
        /// Logs actions performed related to the Overlay system. Includes "DateTime.Now" with each logfile output.
        /// </summary>
        /// <param name="Method">The name of the method for the specific action.</param>
        /// <param name="line">The details of the current action.</param>
        public static void OverlayLog(string Method, string line)
        {
            lock (OverlayLogFile)
            {
                StreamWriter s = new(OverlayLogFile, true);
                s.WriteLine($"{DateTime.Now.ToLocalTime()} {Method} {line}");
                s.Close();
            }
        }

        /// <summary>
        /// Logs the debug output for bot functions. Enabled by Settings Flags.
        /// </summary>
        /// <param name="Method">The name of the method performing the current functionality.</param>
        /// <param name="debugLogTypes">The type of the current function to log.</param>
        /// <param name="line">The message to save to the log.</param>
        internal static void DebugLog(string Method, DebugLogTypes debugLogTypes, string line)
        {
            string Output = debugLogTypes switch
            {
                DebugLogTypes.OverlayBot => OptionFlags.EnableDebugLogOverlays ? line : "",
                DebugLogTypes.DataManager => OptionFlags.EnableDebugDataManager ? line : "",
                DebugLogTypes.TwitchChatBot => OptionFlags.EnableDebugTwitchChatBot ? line : "",
                DebugLogTypes.TwitchClipBot => OptionFlags.EnableDebugTwitchClipBot ? line : "",
                DebugLogTypes.TwitchLiveBot => OptionFlags.EnableDebugTwitchLiveBot ? line : "",
                DebugLogTypes.TwitchFollowBot => OptionFlags.EnableDebugTwitchFollowBot ? line : "",
                DebugLogTypes.TwitchPubSubBot => OptionFlags.EnableDebugTwitchPubSubBot ? line : "",
                DebugLogTypes.DiscordBot => OptionFlags.EnableDebugDiscordBot ? line : "",
                DebugLogTypes.TwitchTokenBot => OptionFlags.EnableDebugTwitchTokenBot ? line : "",
                _ => "",
            };
            if (Output != "")
            {
                lock (DebugLogFile)
                {
                    StreamWriter s = new(DebugLogFile, true);
                    s.WriteLine($"{DateTime.Now.ToLocalTime()} {Method} {debugLogTypes} {Output}");
                    s.Close();
                }
            }
        }
    }
}
