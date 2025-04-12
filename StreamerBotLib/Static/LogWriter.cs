#define AUTO_FLUSH

using StreamerBotLib.Enums;

using System.Globalization;
using System.IO;

#if !AUTO_FLUSH
using System.Reflection;
using System.Text;
#endif

namespace StreamerBotLib.Static
{
    /// <summary>
    /// Manages log output across the whole bot.
    /// </summary>
    public static class LogWriter
    {

        // file names & thread lock strings
        private const string StatusLog = "StatusLog.txt";
        private const string ExceptionLog = "ExceptionLog.txt";
        private const string DebugLogFile = "DebugLogFile.txt";

#if AUTO_FLUSH

        // streamwriters
        private static StreamWriter StatusLogWriter = new(StatusLog, true) { AutoFlush = true };
        private static StreamWriter ExceptionLogWriter = new(ExceptionLog, true) { AutoFlush = true };
        private static StreamWriter DebugLogFileWriter = new(DebugLogFile, true) { AutoFlush = true };

        /// <summary>
        /// Closes logs when the application exits. Be sure to call to avoid possible corrupted files.
        /// </summary>
        public static void ExitCloseLogs()
        {
            lock (StatusLog)
            {
                if (OptionFlags.LogBotStatus)
                {
                    StatusLogWriter.WriteLine($"Ending bot session at {DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}");
                    StatusLogWriter.WriteLine();
                }
                StatusLogWriter.Close();
            }
            lock (ExceptionLog)
            {
                ExceptionLogWriter.Close();
            }
            lock (DebugLogFile)
            {
                DebugLogFileWriter.WriteLine($"Ending bot session at {DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}");
                DebugLogFileWriter.WriteLine();

                DebugLogFileWriter.Close();
            }
        }

        /// <summary>
        /// Reverses the format of the WriteLog intended for a log started message to change the shape in the output file for easier identification.
        /// </summary>
        /// <param name="startDate">The date provided as when the bot session started.</param>
        /// <param name="line">A specific message tied to starting the bot session.</param>
        public static void WriteLog(DateTime startDate, string line)
        {
            lock (StatusLog)
            {
                if (OptionFlags.LogBotStatus)
                {
                    try
                    {
                        StatusLogWriter.WriteLine($"{line}====={startDate.ToLocalTime().ToString(CultureInfo.CurrentCulture)}");
                    }
                    catch (ObjectDisposedException ex)
                    {
                        LogException(ex, "WriteLog");
                        StatusLogWriter = new(StatusLog, true) { AutoFlush = true };
                        StatusLogWriter.WriteLine($"{line}====={startDate.ToLocalTime().ToString(CultureInfo.CurrentCulture)}");
                        StatusLogWriter.Close();
                    }
                }

                try
                {
                    DebugLogFileWriter.WriteLine($"{line}====={startDate.ToLocalTime().ToString(CultureInfo.CurrentCulture)}");
                }
                catch (ObjectDisposedException ex)
                {
                    LogException(ex, "WriteLog");

                    DebugLogFileWriter = new(DebugLogFile, true) { AutoFlush = true };
                    DebugLogFileWriter.WriteLine($"{line}====={startDate.ToLocalTime().ToString(CultureInfo.CurrentCulture)}");
                    DebugLogFileWriter.Close();
                }
            }
        }

        /// <summary>
        /// Write output to a specific log.
        /// </summary>
        /// <param name="ChooseLog">Choose which log to write the data.</param>
        /// <param name="line">The line Content to write to the log.</param>
        public static void WriteLog(string line)
        {
            lock (StatusLog)
            {
                if (OptionFlags.LogBotStatus)
                {
                    try
                    {
                        StatusLogWriter.WriteLine($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}-{line}");
                    }
                    catch (ObjectDisposedException ex)
                    {
                        LogException(ex, "WriteLog");
                        StatusLogWriter = new(StatusLog, true) { AutoFlush = true };
                        StatusLogWriter.WriteLine($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}-{line}");
                        StatusLogWriter.Close();
                    }
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
            lock (ExceptionLog)
            {
                if (OptionFlags.LogExceptions)
                {
                    try
                    {
                        ExceptionLogWriter.WriteLine($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}-{Method}-{ex.GetType()}");
                        ExceptionLogWriter.WriteLine($"{ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                    catch (ObjectDisposedException Eex)
                    {
                        ExceptionLogWriter = new(ExceptionLog, true) { AutoFlush = true };
                        ExceptionLogWriter.WriteLine($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}-{Method}-{ex.GetType()}");
                        ExceptionLogWriter.WriteLine($"{ex.Message}\nStack Trace: {ex.StackTrace}");

                        ExceptionLogWriter.WriteLine($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}-{Method}-{Eex.GetType()}");
                        ExceptionLogWriter.WriteLine($"{Eex.Message}\nStack Trace: {Eex.StackTrace}");
                        ExceptionLogWriter.Close();
                    }
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
            #region DebugLogTypes Switch
            string Output = debugLogTypes switch
            {
#if DEBUG
                DebugLogTypes.SpecialPurpose => OptionFlags.EnableDebugSpecialPurpose ? line : "",
#endif
                DebugLogTypes.OverlayBot => OptionFlags.EnableDebugLogOverlays ? line : "",
                DebugLogTypes.DataManager => OptionFlags.EnableDebugDataManager ? line : "",
                DebugLogTypes.TwitchBots => OptionFlags.EnableDebugTwitchBots ? line : "",
                DebugLogTypes.TwitchStreamerEventSubBot => OptionFlags.EnableDebugTwitchStreamerEventSubBot ? line : "",
                DebugLogTypes.TwitchClipBot => OptionFlags.EnableDebugTwitchClipBot ? line : "",
                DebugLogTypes.TwitchMultiLiveBot => OptionFlags.EnableDebugTwitchMultiBot ? line : "",
                DebugLogTypes.TwitchHelixBot => OptionFlags.EnableDebugTwitchHelixBot ? line : "",
                DebugLogTypes.TwitchBotEventSubBot => OptionFlags.EnableDebugTwitchBotEventSubBot ? line : "",
                DebugLogTypes.TwitchEventSub => OptionFlags.EnableDebugTwitchEventSub ? line : "",
                DebugLogTypes.TwitchBotSendChat => OptionFlags.EnableDebugTwitchBotSendChat ? line : "",
                DebugLogTypes.TwitchTokenBot => OptionFlags.EnableDebugTwitchTokenBot ? line : "",
                DebugLogTypes.DiscordBot => OptionFlags.EnableDebugDiscordBot ? line : "",
                DebugLogTypes.SystemController => OptionFlags.EnableDebugSystemController ? line : "",
                DebugLogTypes.BotController => OptionFlags.EnableDebugBotController ? line : "",
                DebugLogTypes.CommandSystem => OptionFlags.EnableDebugCommandSystem ? line : "",
                DebugLogTypes.RepeatCommandSystem => OptionFlags.EnableDebugRepeatCommandSystem ? line : "",
                DebugLogTypes.StatSystem => OptionFlags.EnableDebugStatSystem ? line : "",
                DebugLogTypes.CurrencySystem => OptionFlags.EnableDebugCurrencySystem ? line : "",
                DebugLogTypes.ManageStreamViewers => OptionFlags.EnableDebugManageStreamViewers ? line : "",
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
                DebugLogTypes.GUIMultiLive => OptionFlags.EnableDebugGUIMultiLive ? line : "",
                DebugLogTypes.TwitchStreamerNoScopesEventSubBot => OptionFlags.EnableDebugTwitchStreamerNoScopesEventSubBot ? line : "",
                _ => "",
            };
            #endregion

            if (Output != "")
            {
                lock (DebugLogFile)
                {
                    try
                    {
                        DebugLogFileWriter.WriteLine($"{DateTime.Now.ToLocalTime()}-{Method}-{debugLogTypes}-{Output}");
                    }
                    catch (ObjectDisposedException ex)
                    {
                        LogException(ex, "WriteLog");
                        DebugLogFileWriter = new(DebugLogFile, true) { AutoFlush = true };
                        DebugLogFileWriter.WriteLine($"{DateTime.Now.ToLocalTime()}-{Method}-{debugLogTypes}-{Output}");
                        DebugLogFileWriter.Close();
                    }
                }
            }
        }

#else
        // stream flush parameters
        private static TimeSpan StreamFlush = new(0, 10, 0);
        private static readonly DateTime FlushTime = DateTime.Now;

        // streamwriters
        private static StreamWriter StatusLogWriter;
        private static StreamWriter ExceptionLogWriter;
        private static StreamWriter DebugLogFileWriter;

        private static bool started;
     
        private static StringBuilder StatusLogSB = new();
        private static StringBuilder ExceptionLogSB = new();
        private static StringBuilder DebugLogSB = new();

        /// <summary>
        /// Start a flush thread, to check and flush the streamwriter every <code>StreamFlush</code> amount of time.
        /// </summary>
        private static void StaticFlush()
        {
            if (!started)
            {
                started = true;
                ThreadManager.CreateThreadStart("StaticFlush", () =>
                {
                    while (OptionFlags.ActiveToken)
                    {
                        if (DateTime.Now > FlushTime)
                        {
                            FlushTime = DateTime.Now + StreamFlush;
                            lock (StatusLog)
                            {
                                StatusLogWriter = new(StatusLog, true);
                                StatusLogWriter.Write(StatusLogSB);
                                StatusLogSB.Clear();
                                StatusLogWriter.Close();
                            }
                            lock (ExceptionLog)
                            {
                                ExceptionLogWriter = new(ExceptionLog, true);
                                ExceptionLogWriter.Write(ExceptionLogSB);
                                ExceptionLogSB.Clear();
                                ExceptionLogWriter.Close();
                            }
                            lock (DebugLogFile)
                            {
                                DebugLogFileWriter = new(DebugLogFile, true);
                                DebugLogFileWriter.Write(DebugLogSB);
                                DebugLogSB.Clear();
                                DebugLogFileWriter.Close();
                            }
                        }
                        Thread.Sleep(500); // sleep for 10 minutes = app stays open, flush timespan
                    }
                });
            }
        }

        /// <summary>
        /// Closes logs when the application exits. Be sure to call to avoid possible corrupted files.
        /// </summary>
        public static void ExitCloseLogs()
        {
            lock (StatusLog)
            {
                StatusLogWriter = new(StatusLog, true);
                StatusLogWriter.Write(StatusLogSB);
                StatusLogSB.Clear();
                StatusLogWriter.Close();
            }
            lock (ExceptionLog)
            {
                ExceptionLogWriter = new(ExceptionLog, true);
                ExceptionLogWriter.Write(ExceptionLogSB);
                ExceptionLogSB.Clear();
                ExceptionLogWriter.Close();
            }
            lock (DebugLogFile)
            {
                DebugLogFileWriter = new(DebugLogFile, true);
                DebugLogFileWriter.Write(DebugLogSB);
                DebugLogSB.Clear();
                DebugLogFileWriter.Close();
            }
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
                    try
                    {
                        StatusLogSB.AppendLine($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}-{line}");
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
                    try
                    {
                        ExceptionLogSB.AppendLine($"{DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture)}-{Method}-{ex.GetType()}");
                        ExceptionLogSB.AppendLine($"{ex.Message}\nStack Trace: {ex.StackTrace}");
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
                    try
                    {
                        DebugLogSB.AppendLine($"{DateTime.Now.ToLocalTime()}-{Method}-{debugLogTypes}-{Output}");
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

#endif

    }
}
