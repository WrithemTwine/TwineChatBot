using StreamerBotLib.Enums;

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

        private static bool started;

        // streamwriters
        private static readonly StreamWriter StatusLogWriter = new(StatusLog, true);
        private static readonly StreamWriter ExceptionLogWriter = new(ExceptionLog, true);
        private static readonly StreamWriter DebugLogFileWriter = new(DebugLogFile, true);

        /// <summary>
        /// Start a flush thread, to check and flush the streamwriter every <code>StreamFlush</code> amount of time.
        /// </summary>
        private static void StaticFlush()
        {
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
                            StatusLogWriter.Flush();
                            ExceptionLogWriter.Flush();
                            DebugLogFileWriter.Flush();
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Closes logs when the application exits. Be sure to call to avoid possible corrupted files.
        /// </summary>
        public static void ExitCloseLogs()
        {
            StatusLogWriter.Close();
            StatusLogWriter.Dispose();
            ExceptionLogWriter.Close();
            ExceptionLogWriter.Dispose();
            DebugLogFileWriter.Close();
            DebugLogFileWriter.Dispose();
        }

        /// <summary>
        /// Write output to a specific log.
        /// </summary>
        /// <param name="ChooseLog">Choose which log to write the data.</param>
        /// <param name="line">The line Content to write to the log.</param>
        public static void WriteLog(string line)
        {
            StaticFlush();
            lock (StatusLog)
            {
                if (OptionFlags.LogBotStatus)
                {
                    StatusLogWriter.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Specifically log caught exceptions. Internally adds 'DateTime.Now' to the log file output.
        /// </summary>
        /// <param name="ex">The exception caught in the app.</param>
        /// <param name="Method">Name of the method which caught the exception.</param>
        public static void LogException(Exception ex, string Method)
        {
            StaticFlush();
            lock (ExceptionLog)
            {
                if (OptionFlags.LogExceptions)
                {
                    ExceptionLogWriter.WriteLine($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)} {Method} {ex.GetType()}");
                    ExceptionLogWriter.WriteLine($"{ex.Message}\nStack Trace: {ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Logs the debug output for bot functions. Enabled by Settings Flags.
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
                lock (DebugLogFile)
                {
                    DebugLogFileWriter.WriteLine($"{DateTime.Now.ToLocalTime()} {Method} {debugLogTypes} {Output}");
                }
            }
        }
    }
}
