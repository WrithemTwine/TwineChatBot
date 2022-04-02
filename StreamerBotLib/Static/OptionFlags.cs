using StreamerBotLib.Properties;

using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace StreamerBotLib.Static
{
    public static class OptionFlags
    {
        public static bool LogBotStatus { get; set; }
        public static bool LogExceptions { get; set; }

        public static bool ActiveToken { get; set; }  // whether to process ops or not
        public static bool IsStreamOnline { get; set; } // whether stream is online

        public static bool TwitchAddFollowersStart { get; set; }
        public static bool TwitchPruneNonFollowers { get; set; }
        public static bool TwitchAddFollowerNotification { get; set; }
        public static bool TwitchFollowerAutoRefresh { get; set; }
        public static int TwitchFollowerRefreshHrs { get; set; }

        public static bool FirstUserJoinedMsg { get; set; }
        public static bool FirstUserChatMsg { get; set; }

        public static DateTime TwitchRefreshDate { get; set; }

        public static bool MsgAddMe { get; set; }
        public static bool MsgNoMe { get; set; }
        public static bool MsgPerComMe { get; set; }

        public static bool MsgWelcomeStreamer { get; set; }
        public static bool WelcomeCustomMsg { get; set; }

        public static bool AutoShout { get; set; }
        public static bool TwitchRaidShoutOut { get; set; }

        public static bool RepeatTimer { get; set; }
        public static bool RepeatTimerDilute { get; set; }
        public static bool RepeatWhenLive { get; set; }
        public static bool RepeatLiveReset { get; set; }  // reset repeat timers when stream goes live
        public static bool RepeatLiveResetShow { get; set; }    // enable showing message when repeat timers reset for going live

        public static bool UserPartyStart { get; set; }
        public static bool UserPartyStop { get; set; }

        // Enables or disables posting multiple live messages to social media on the same day, i.e. the stream crashes and restarts and another 'Live' alert is posted.
        public static bool PostMultiLive { get; set; }
        public static string LiveMsg { get; set; }

        public static bool DataLoaded { get; set; } = false;

        public static bool ManageUsers { get; set; }
        public static bool ManageFollowers { get; set; }
        public static bool ManageStreamStats { get; set; }
        public static bool ManageRaidData { get; set; }
        public static bool ManageOutRaidData { get; set; }
        public static bool ManageGiveawayUsers { get; set; }
        public static bool ManageDataArchiveMsg { get; set; }

        public static bool TwitchChatBotConnectOnline { get; set; }
        public static bool TwitchChatBotDisconnectOffline { get; set; }

        public static bool TwitchClipPostChat { get; set; }
        public static bool TwitchClipPostDiscord { get; set; }

        public static bool TwitchCurrencyStart { get; set; }
        public static bool TwitchCurrencyOnline { get; set; }

        public static int GiveawayCount { get; set; }
        public static string GiveawayWinMsg { get; set; }
        public static string GiveawayBegMsg { get; set; }
        public static string GiveawayEndMsg { get; set; }
        public static bool GiveawayMultiUser { get; set; }
        public static int GiveawayMultiEntries { get; set; }

        public static bool TwitchPubSubChannelPoints { get; set; }

        public static string TwitchChannelName { get; set; }
        public static string TwitchBotUserName { get; set; }
        public static string TwitchBotClientId { get; set; }
        public static string TwitchBotAccessToken { get; set; }

        public static string TwitchStreamClientId { get; set; }
        public static string TwitchStreamOauthToken { get; set; }
        public static DateTime TwitchStreamerTokenDate { get; set; }
        public static bool TwitchStreamerValidToken => CurrentToTwitchRefreshDate(TwitchStreamerTokenDate) > new TimeSpan(0, 0, 0);

        /// <summary>
        /// Flag to indicate whether the API should use Streamer Client Id credentials per Twitch requirements.
        /// </summary>
        public static bool TwitchStreamerUseToken { get; set; }

        public static bool ModerateUsersWarn { get; set; }
        public static bool ModerateUsersAction { get; set; }
        public static bool ModerateUserLearnMsgs { get; set; }

        /// <summary>
        /// First saves the settings, then reads the settings into the flag properties. Thread-Safe update.
        /// </summary>
        public static void SetSettings()
        {
            lock (Settings.Default)
            {
                Settings.Default.Save();

                LogBotStatus = Settings.Default.LogBotStatus;
                LogExceptions = Settings.Default.LogExceptions;

                TwitchAddFollowersStart = Settings.Default.TwitchAddFollowersStart;
                TwitchPruneNonFollowers = Settings.Default.TwitchPruneNonFollowers;
                TwitchAddFollowerNotification = Settings.Default.TwitchAddFollowerNotification;
                TwitchFollowerAutoRefresh = Settings.Default.TwitchFollowerAutoRefresh;
                TwitchFollowerRefreshHrs = Settings.Default.TwitchFollowerRefreshHrs;

                FirstUserJoinedMsg = Settings.Default.FirstUserJoinedMsg;
                FirstUserChatMsg = Settings.Default.FirstUserChatMsg;

                TwitchRefreshDate = Settings.Default.TwitchRefreshDate;

                MsgAddMe = Settings.Default.MsgInsertMe;
                MsgNoMe = Settings.Default.MsgNoMe;
                MsgPerComMe = Settings.Default.MsgPerComMe;

                MsgWelcomeStreamer = Settings.Default.MsgWelcomeStreamer;
                WelcomeCustomMsg = Settings.Default.WelcomeCustomMsg;

                AutoShout = Settings.Default.MsgAutoShout;

                TwitchRaidShoutOut = Settings.Default.TwitchRaidShoutOut;

                RepeatTimer = Settings.Default.RepeatTimerCommands;
                RepeatTimerDilute = Settings.Default.RepeatTimerComSlowdown;
                RepeatWhenLive = Settings.Default.RepeatWhenLive;
                RepeatLiveReset = Settings.Default.RepeatLiveReset;
                RepeatLiveResetShow = Settings.Default.RepeatLiveResetShow;

                UserPartyStart = Settings.Default.UserPartyStart;
                UserPartyStop = Settings.Default.UserPartyStop;

                PostMultiLive = Settings.Default.PostMultiLive;
                LiveMsg = Settings.Default.MsgLive;

                ManageUsers = Settings.Default.ManageUsers;
                ManageFollowers = Settings.Default.ManageFollowers;
                ManageStreamStats = Settings.Default.ManageStreamStats;
                ManageRaidData = Settings.Default.ManageRaidData;
                ManageOutRaidData = Settings.Default.ManageOutRaidData;
                ManageGiveawayUsers = Settings.Default.ManageGiveawayUsers;
                ManageDataArchiveMsg = Settings.Default.ManageDataArchiveMsg;

                TwitchChatBotConnectOnline = Settings.Default.TwitchChatBotConnectOnline;
                TwitchChatBotDisconnectOffline = Settings.Default.TwitchChatBotDisconnectOffline;

                TwitchClipPostChat = Settings.Default.TwitchClipPostChat;
                TwitchClipPostDiscord = Settings.Default.TwitchClipPostDiscord;

                TwitchCurrencyStart = Settings.Default.TwitchCurrencyStart;
                TwitchCurrencyOnline = Settings.Default.TwitchCurrencyOnline;

                GiveawayCount = Settings.Default.GiveawayCount;
                GiveawayBegMsg = Settings.Default.GiveawayBegMsg;
                GiveawayEndMsg = Settings.Default.GiveawayEndMsg;
                GiveawayWinMsg = Settings.Default.GiveawayWinMsg;
                GiveawayMultiUser = Settings.Default.GiveawayMultiUser;
                GiveawayMultiEntries = Settings.Default.GiveawayMaxEntries;

                TwitchPubSubChannelPoints = Settings.Default.TwitchPubSubChannelPoints;

                TwitchChannelName = Settings.Default.TwitchChannelName;
                TwitchBotUserName = Settings.Default.TwitchBotUserName;
                TwitchBotClientId = Settings.Default.TwitchClientID;
                TwitchBotAccessToken = Settings.Default.TwitchAccessToken;

                TwitchStreamClientId = Settings.Default.TwitchStreamClientId;
                TwitchStreamerTokenDate = Settings.Default.TwitchStreamerTokenDate;
                TwitchStreamOauthToken = Settings.Default.TwitchStreamOauthToken;

                TwitchStreamerUseToken = Settings.Default.TwitchBotUserName != Settings.Default.TwitchChannelName;

                ModerateUsersWarn = Settings.Default.ModerateUsersWarn;
                ModerateUsersAction = Settings.Default.ModerateUsersAction;
                ModerateUserLearnMsgs = Settings.Default.ModerateUserLearnMsgs;
            }
        }

        /// <summary>
        /// Sets whether the "Join Party" list is started or not, and refreshes the settings.
        /// </summary>
        /// <param name="Start">Whether the party is started.</param>
        public static void SetParty(bool Start = true)
        {
            Settings.Default.UserPartyStart = Start;
            Settings.Default.UserPartyStop = !Start;

            SetSettings();
        }

        /// <summary>
        /// Provides deference between the provided date and "Now".
        /// </summary>
        /// <param name="RefreshDate">The date to use in the calculation for the difference from "Now".</param>
        /// <returns>The difference between <paramref name="RefreshDate"/> and "Now" in a TimeSpan.</returns>
        public static TimeSpan CurrentToTwitchRefreshDate(DateTime RefreshDate)
        {
            return RefreshDate - DateTime.Now.ToLocalTime();
        }

        /// <summary>
        /// Check if the setting is the default value or has changed.
        /// </summary>
        /// <param name="SettingName">The name in the "Settings.Default" to check.</param>
        /// <param name="CheckSettingValue">The value to compare to the default settings.</param>
        /// <returns>DefaultValue(SettingName).Value == CheckSettingValue; true if value is default</returns>
        public static bool CheckSettingIsDefault(string SettingName, string CheckSettingValue)
        {
            DefaultSettingValueAttribute defaultSetting = null;

            foreach (MemberInfo m in from MemberInfo m in typeof(Settings).GetProperties()
                                     where m.Name == SettingName
                                     select m)
            {
                defaultSetting = (DefaultSettingValueAttribute)m.GetCustomAttribute(typeof(DefaultSettingValueAttribute));
            }

            return defaultSetting.Value == CheckSettingValue;
        }
    }
}
