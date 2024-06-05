using StreamerBotLib.Enums;

using System.Collections.Concurrent;
using System.Globalization;
using System.IO;

namespace StreamerBotLib.Static
{
    /// <summary>
    /// Manages log output across the whole bot.
    /// </summary>
    public static class LogWriter
    {
        // stream flush parameters
        private static TimeSpan StreamFlush = new(0, 10, 0);
        private static DateTime FlushTime = DateTime.Now;

        // file names & thread lock strings
        private const string StatusLog = "StatusLog.txt";
        private const string ExceptionLog = "ExceptionLog.txt";
        private const string DebugLogFile = "DebugLogFile.txt";

        private static ConcurrentQueue<string> StatusLogData = new();
        private static ConcurrentQueue<string> ExceptionLogData = new();
        private static ConcurrentQueue<string> DebugLogData = new();

        private static bool started;

        /// <summary>
        /// Start a flush thread. Refer to strings for each log file saving the messages to write to each logfile, then close the file.
        /// </summary>
        private static void StaticFlush()
        {
            static void DataWrite()
            {
                if (!StatusLogData.IsEmpty)
                {
                    StreamWriter StatusLogWriter = new(StatusLog, true);
                    while (!StatusLogData.IsEmpty)
                    {
                        if (StatusLogData.TryDequeue(out string data))
                        {
                            StatusLogWriter.WriteLine(data);
                        }
                    }
                    StatusLogWriter.Close();
                }

                if (!ExceptionLogData.IsEmpty)
                {
                    StreamWriter ExceptionLogWriter = new(ExceptionLog, true);
                    while (!ExceptionLogData.IsEmpty)
                    {
                        if (ExceptionLogData.TryDequeue(out string data))
                        {
                            ExceptionLogWriter.WriteLine(data);
                        }
                    }
                    ExceptionLogWriter.Close();
                }

                if (!DebugLogData.IsEmpty)
                {
                    StreamWriter DebugLogFileWriter = new(DebugLogFile, true);
                    while (!DebugLogData.IsEmpty)
                    {
                        if (DebugLogData.TryDequeue(out string data))
                        {
                            DebugLogFileWriter.WriteLine(data);
                        }
                    }
                    DebugLogFileWriter.Close();
                }
            }

            if (!started)
            {
                started = true;
                ThreadManager.CreateThreadStart(() =>
                {
                    while (OptionFlags.ActiveToken)
                    {
                        if (DateTime.Now > FlushTime)
                        {
                            FlushTime = DateTime.Now + StreamFlush;
                            DataWrite();
                        }

                        Thread.Sleep(500);
                    }
                });
            }
            else if (!OptionFlags.ActiveToken)
            {
                DataWrite();
            }
        }

        /// <summary>
        /// Application shutting down, finish writing all available data.
        /// </summary>
        public static void ExitLogs()
        {
            StaticFlush();
        }

        /// <summary>
        /// Write output to a specific log. Saves content to a string for writing in regular intervals.
        /// </summary>
        /// <param name="ChooseLog">Choose which log to write the data.</param>
        /// <param name="line">The line Content to write to the log.</param>
        public static void WriteLog(string line)
        {
            StaticFlush();
            if (OptionFlags.LogBotStatus)
            {
                try
                {
                    StatusLogData.Enqueue($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}-{line}");
                }
                catch (ObjectDisposedException ex)
                {
                    LogException(ex, "WriteLog");
                    StreamWriter sr = new(StatusLog, true);
                    sr.WriteLine($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}-{line}");
                    sr.Close();
                }
            }
        }

        /// <summary>
        /// Specifically log caught exceptions. Internally adds 'DateTime.Now' to the log file output. Saves content to a string for writing in regular intervals.
        /// </summary>
        /// <param name="ex">The exception caught in the app.</param>
        /// <param name="Method">Name of the method which caught the exception.</param>
        public static void LogException(Exception ex, string Method)
        {
            StaticFlush();
            if (OptionFlags.LogExceptions)
            {
                try
                {
                    ExceptionLogData.Enqueue($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}-{Method}-{ex.GetType()}");
                    ExceptionLogData.Enqueue($"{ex.Message}\nStack Trace: {ex.StackTrace}");
                }
                catch (ObjectDisposedException Eex)
                {
                    StreamWriter sr = new(ExceptionLog, true);
                    sr.WriteLine($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}-{Method}-{ex.GetType()}");
                    sr.WriteLine($"{ex.Message}\nStack Trace: {ex.StackTrace}");

                    sr.WriteLine($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}-{Method}-{Eex.GetType()}");
                    sr.WriteLine($"{Eex.Message}\nStack Trace: {Eex.StackTrace}");
                    sr.Close();
                }
            }
        }

        /// <summary>
        /// Logs the debug output for bot functions. Enabled by Settings Flags. Saves content to a string for writing in regular intervals.
        /// </summary>
        /// <param name="Method">The name of the method performing the current functionality.</param>
        /// <param name="debugLogTypes">The type of the current function to log. If user doesn't enable this log setting, it won't emit.</param>
        /// <param name="line">The message to save to the log.</param>
        public static void DebugLog(string Method, DebugLogTypes debugLogTypes, string line)
        {
            StaticFlush();
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
                DebugLogTypes.TwitchBotUserSvc => OptionFlags.EnableDebugTwitchUserSvcBot ? line : "",
                DebugLogTypes.SystemController => OptionFlags.EnableDebugSystemController ? line : "",
                DebugLogTypes.BotController => OptionFlags.EnableDebugBotController ? line : "",
                DebugLogTypes.CommandSystem => OptionFlags.EnableDebugCommandSystem ? line : "",
                DebugLogTypes.StatSystem => OptionFlags.EnableDebugStatSystem ? line : "",
                DebugLogTypes.TwitchMultiLiveBot => OptionFlags.EnableDebugTwitchMultiLiveBot ? line : "",
                DebugLogTypes.CurrencySystem => OptionFlags.EnableDebugCurrencySystem ? line : "",
                DebugLogTypes.ModerationSystem => OptionFlags.EnableDebugModerationSystem ? line : "",
                DebugLogTypes.OverlaySystem => OptionFlags.EnableDebugOverlaySystem ? line : "",
                DebugLogTypes.CommonSystem => OptionFlags.EnableDebugCommonSystem ? line : "",
                DebugLogTypes.BlackjackGame => OptionFlags.EnableDebugBlackjackGame ? line : "",
                DebugLogTypes.LocalizedMessages => OptionFlags.EnableDebugLocalizedMessages ? line : "",
                DebugLogTypes.FormatData => OptionFlags.EnableDebugFormatData ? line : "",
                DebugLogTypes.ThreadManager => OptionFlags.EnableDebugThreadManager ? line : "",
                DebugLogTypes.OutputMsgParsing => OptionFlags.EnableDebugOutputMsgParsing ? line : "",
                DebugLogTypes.GUIProcessWatcher => OptionFlags.EnableDebugGUIProcessWatcher ? line : "",
                DebugLogTypes.GUITabSizes => OptionFlags.EnableDebugGUITabSizes ? line : "",
                DebugLogTypes.GUIThemes => OptionFlags.EnableDebugGUIThemes ? line : "",
                DebugLogTypes.GUITwitchTokenAuth => OptionFlags.EnableDebugGUITwitchTokenAuth ? line : "",
                DebugLogTypes.GUIEvents => OptionFlags.EnableDebugGUIEvents ? line : "",
                DebugLogTypes.GUIHelpers => OptionFlags.EnableDebugGUIHelpers ? line : "",
                DebugLogTypes.GUIDataViews => OptionFlags.EnableDebugGUIDataViews ? line : "",
                DebugLogTypes.GUIBotComs => OptionFlags.EnableDebugGUIBotComs ? line : "",
                DebugLogTypes.TwitchBots => OptionFlags.EnableDebugTwitchBots ? line : "",
                _ => "",
            };
            if (Output != "")
            {
                try
                {
                    DebugLogData.Enqueue($"{DateTime.Now.ToLocalTime()}-{Method}-{debugLogTypes}-{Output}");
                }
                catch (ObjectDisposedException ex)
                {
                    LogException(ex, "WriteLog");
                    StreamWriter sr = new(DebugLogFile, true);
                    sr.WriteLine($"{DateTime.Now.ToLocalTime()}-{Method}-{debugLogTypes}-{Output}");
                    sr.Close();
                }
            }
        }
    }
}
