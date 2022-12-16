using StreamerBotLib.Properties;

using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace StreamerBotLib.Static
{
    /// <summary>
    /// Holds settings and flags for access across the application.
    /// </summary>
    public static class OptionFlags
    {
        /// <summary>
        /// Specifies whether to record bot status messages in the log file.
        /// </summary>
        public static bool LogBotStatus { get; set; }
        /// <summary>
        /// Specifies whether to record exceptions during bot operation.
        /// </summary>
        public static bool LogExceptions { get; set; }

        /// <summary>
        /// The app-wide token to keep threads running, setting it false indicates it's all shut down to close the application.
        /// </summary>
        public static bool ActiveToken { get; set; }
        /// <summary>
        /// Specifies the watched stream is online, to permit operations to perform only during a livestream.
        /// </summary>
        public static bool IsStreamOnline { get; set; }

        /// <summary>
        /// Specifies to add Twitch followers for the given channel upon starting the bot.
        /// </summary>
        public static bool TwitchAddFollowersStart { get; set; }
        /// <summary>
        /// Specifies to clear Twitch followers in the database if they are no longer followers.
        /// </summary>
        public static bool TwitchPruneNonFollowers { get; set; }
        /// <summary>
        /// Turns off notification during the bulk follower add operation, prevents sending chats to the channel.
        /// </summary>
        public static bool TwitchAddFollowerNotification { get; set; }
        /// <summary>
        /// Enables an auto refresh for adding Twitch followers again after a certain amount of time, since Twitch doesn't provide notification when a user no longer follows a channel.
        /// </summary>
        public static bool TwitchFollowerAutoRefresh { get; set; }
        /// <summary>
        /// Specifies the hours interval to check and add all Twitch followers regarding the given channel.
        /// </summary>
        public static int TwitchFollowerRefreshHrs { get; set; }
        /// <summary>
        /// Enables mechanisms to limit the number of messages the bot sends to the channel when a number of new followers occurs within the followers polling time.
        /// </summary>
        public static bool TwitchFollowerEnableMsgLimit { get; set; }
        /// <summary>
        /// Specifies the 1-100 limit of new followers, after which individual messages are condensed into fewer follower messages.
        /// </summary>
        public static int TwitchFollowerMsgLimit { get; set; }
        /// <summary>
        /// Setting to ban bots performing Twitch follow storms - hundreds of follows in a few seconds.
        /// </summary>
        public static bool TwitchFollowerAutoBanBots { get; set; }
        /// <summary>
        /// Specifies threshold above which is considered a bot follow storm, and invokes banning the accounts included within the follow storm.
        /// </summary>
        public static int TwitchFollowerAutoBanCount { get; set; }
        /// <summary>
        /// Specifies whether bot starts when stream is detected online.
        /// </summary>
        public static bool TwitchFollowerConnectOnline { get; set; }
        /// <summary>
        /// Specifies whether bot stops when stream is detected offline.
        /// </summary>
        public static bool TwitchFollowerDisconnectOffline { get; set; }

        /// <summary>
        /// Specifies whether to welcome a user when they first join the given channel.
        /// </summary>
        public static bool FirstUserJoinedMsg { get; set; }
        /// <summary>
        /// Specifies whether to welcome a user when they first chat in the given (live) channel.
        /// </summary>
        public static bool FirstUserChatMsg { get; set; }
        /// <summary>
        /// The datetime of the earliest expiring Twitch token, to help limit operations and prevent crashing from using an outdated access token.
        /// </summary>
        public static DateTime TwitchRefreshDate { get; set; }

        /// <summary>
        /// Adds /me to all outgoing bot messages.
        /// </summary>
        public static bool MsgAddMe { get; set; }
        
        /// <summary>
        /// Enable the bot to emit "Command Not Found" response message when command is not found.
        /// </summary>
        public static bool MsgCommandNotFound { get; set; }
        /// <summary>
        /// Does not include /me on any message.
        /// </summary>
        public static bool MsgNoMe { get; set; }
        /// <summary>
        /// Adds /me per each command setting in the database.
        /// </summary>
        public static bool MsgPerComMe { get; set; }

        /// <summary>
        /// Specifies whether to not welcome the streamer if they type a message.
        /// </summary>
        public static bool MsgWelcomeStreamer { get; set; }
        /// <summary>
        /// Enables a custom welcome message depending on the user entering the channel, is active alongside the welcome user setting.
        /// </summary>
        public static bool WelcomeCustomMsg { get; set; }

        /// <summary>
        /// Auto shout specific users, per the welcome user setting (when they first join or when they first chat).
        /// </summary>
        public static bool AutoShout { get; set; }
        /// <summary>
        /// Setting for the bot to emit the "!so username" to the given channel chat, such that other bots can pick-up a shout-out for additional notifications.
        /// </summary>
        public static bool MsgSendSOToChat { get; set; }
        /// <summary>
        /// Enables whether the bot should shout out all users who raid the given channel.
        /// </summary>
        public static bool TwitchRaidShoutOut { get; set; }

        /// <summary>
        /// Flags whether the streamer channel started a raid - could be anyone based on command rights
        /// </summary>
        public static bool TwitchOutRaidStarted { get; set; } = false;

        /// <summary>
        /// Enables repeat timer commands.
        /// </summary>
        public static bool RepeatTimer { get; set; }
        /// <summary>
        /// Whether to dilute the repeat timer commands, they'll run at a later time to not clog up the channel's chat.
        /// </summary>
        public static bool RepeatTimerDilute { get; set; }
        /// <summary>
        /// Setting for not adjusting repeat timers or limiting messages.
        /// </summary>
        public static bool RepeatNoAdjustment { get; set; }
        /// <summary>
        /// Setting for repeat timers to send messages based on thresholds.
        /// </summary>
        public static bool RepeatUseThresholds { get; set; }
        /// <summary>
        /// Setting to use the users threadhold, whether to send repeat messages.
        /// </summary>
        public static bool RepeatAboveUserCount { get; set; }
        /// <summary>
        /// Setting to use the chat count threshold, whether to send repeat messages.
        /// </summary>
        public static bool RepeatAboveChatCount { get; set; }
        /// <summary>
        /// Enables performing the repeat timer commands only when live, otherwise the messages would run all the time.
        /// </summary>
        public static bool RepeatWhenLive { get; set; }
        /// <summary>
        /// Enables resetting the repeat times once the stream is live, as a basis for how much time to elapse before the next repeat. For when the bot has been on for days.
        /// </summary>
        public static bool RepeatLiveReset { get; set; }  // reset repeat timers when stream goes live
        /// <summary>
        /// Enables actually sending the message to chat once the times are reset once the given channel is live.
        /// </summary>
        public static bool RepeatLiveResetShow { get; set; }    // enable showing message when repeat timers reset for going live
        /// <summary>
        /// The about of minutes to check the number of users for the dilution calculation.
        /// </summary>
        public static int RepeatUserMinutes { get; set; }
        /// <summary>
        /// The amount of users in the time interval, under this threshold is in dilution calculation.
        /// </summary>
        public static int RepeatUserCount { get; set; }
        /// <summary>
        /// The time interval to check the number of chats for the time dultion calculation.
        /// </summary>
        public static int RepeatChatMinutes { get; set; }
        /// <summary>
        /// The amount of chat in time interval, under this threshold is in dilution calculation.
        /// </summary>
        public static int RepeatChatCount { get; set; }


        /// <summary>
        /// Indicates the user join list is active to accept users to the join list.
        /// </summary>
        public static bool UserPartyStart { get; set; }
        /// <summary>
        /// Stops the user join list, doesn't accept any more join requests.
        /// </summary>
        public static bool UserPartyStop { get; set; }

        /// <summary>
        /// Enables or disables posting multiple live messages to social media on the same day, i.e. the stream crashes and restarts and another 'Live' alert is posted.
        /// </summary>
        public static bool PostMultiLive { get; set; }
        /// <summary>
        /// The social media message to use to notify the given channel is live.
        /// </summary>
        public static string LiveMsg { get; set; }

        /// <summary>
        /// Indicates database xml file successfully loaded.
        /// </summary>
        public static bool DataLoaded { get; set; } = false;

        /// <summary>
        /// Specifies to save user data in the database.
        /// </summary>
        public static bool ManageUsers { get; set; }
        /// <summary>
        /// Specifies to save follower data in the database.
        /// </summary>
        public static bool ManageFollowers { get; set; }
        /// <summary>
        /// Specifies to save stream statistic data in the database.
        /// </summary>
        public static bool ManageStreamStats { get; set; }
        /// <summary>
        /// Specifies to save incoming raid data in the database.
        /// </summary>
        public static bool ManageRaidData { get; set; }
        /// <summary>
        /// Specifies to save outgoing raid data in the database.
        /// </summary>
        public static bool ManageOutRaidData { get; set; }
        /// <summary>
        /// Specifies to save giveaway data in the database.
        /// </summary>
        public static bool ManageGiveawayUsers { get; set; }
        /// <summary>
        /// Specifies to user about they need to manage archiving the saved data.
        /// </summary>
        public static bool ManageDataArchiveMsg { get; set; }
        /// <summary>
        /// Enables 'clear data' buttons in the GUI, to prevent accidental clicking.
        /// </summary>
        public static bool ManageClearButtonEnabled { get; set; }

        /// <summary>
        /// Specifies to save OverlayTicker data in the database.
        /// </summary>
        public static bool ManageOverlayTicker { get; set; }

        /// <summary>
        /// Enables starting the Twitch Chat Bot once the stream goes live. Depends on the live service detecting going live.
        /// </summary>
        public static bool TwitchChatBotConnectOnline { get; set; }
        /// <summary>
        /// Enables whether to stop Twitch Chat Bot once the stream goes offline.
        /// </summary>
        public static bool TwitchChatBotDisconnectOffline { get; set; }

        /// <summary>
        /// Specifies whether to post a channel clip link to the given channel chat.
        /// </summary>
        public static bool TwitchClipPostChat { get; set; }
        /// <summary>
        /// Specifies whether to post a channel clip link to Discord, and Discord webhooks need a 'clips' link.
        /// </summary>
        public static bool TwitchClipPostDiscord { get; set; }
        /// <summary>
        /// Specifies whether the clip bot connects when the stream is detected online.
        /// </summary>
        public static bool TwitchClipConnectOnline { get; set; }
        /// <summary>
        /// Specifies whether the clip bot disconnects when the stream is detected offline.
        /// </summary>
        public static bool TwitchClipDisconnectOffline { get; set; }
        /// <summary>
        /// Specifies starting currency accruals.
        /// </summary>
        public static bool TwitchCurrencyStart { get; set; }
        /// <summary>
        /// Specifies whether currency accrual should only occur when stream is live (online), for testing purposes.
        /// </summary>
        public static bool TwitchCurrencyOnline { get; set; }

        /// <summary>
        /// Specifies how many selections to make for the giveaway.
        /// </summary>
        public static int GiveawayCount { get; set; }
        /// <summary>
        /// The message to send the channel chat for a user winning the giveaway.
        /// </summary>
        public static string GiveawayWinMsg { get; set; }
        /// <summary>
        /// The message for when the giveaway starts/begins.
        /// </summary>
        public static string GiveawayBegMsg { get; set; }
        /// <summary>
        /// The message for when the giveaway ends.
        /// </summary>
        public static string GiveawayEndMsg { get; set; }
        /// <summary>
        /// Determines if the user can submit multiple entries.
        /// </summary>
        public static bool GiveawayMultiUser { get; set; }
        /// <summary>
        /// The number of giveaway entries a user can submit.
        /// </summary>
        public static int GiveawayMultiEntries { get; set; }

        /// <summary>
        /// Specifies Twitch Pub Sub scope, to include Channel Points.
        /// </summary>
        public static bool TwitchPubSubChannelPoints { get; set; }
        /// <summary>
        /// Enables the PubSub to start when the stream is online and stop when the stream is offline.
        /// </summary>
        public static bool TwitchPubSubOnlineMode { get; set; }

        /// <summary>
        /// Specifies the Twitch Channel Name to monitor.
        /// </summary>
        public static string TwitchChannelName { get; set; }
        /// <summary>
        /// Specifies the Twitch Bot User Name for this application to chat through.
        /// </summary>
        public static string TwitchBotUserName { get; set; }
        /// <summary>
        /// Specifies the bot account client ID, to use in authentication calls.
        /// </summary>
        public static string TwitchBotClientId { get; set; }
        /// <summary>
        /// Specifies the bot account access token, used in authentication calls.
        /// </summary>
        public static string TwitchBotAccessToken { get; set; }

        /// <summary>
        /// Some Twitch PubSub and Twitch API calls require the streamer account client ID, to perform the function call.
        /// </summary>
        public static string TwitchStreamClientId { get; set; }
        /// <summary>
        /// Some Twitch PubSub and Twitch API calls require the streamer account access token, to perform the function call.
        /// </summary>
        public static string TwitchStreamOauthToken { get; set; }
        /// <summary>
        /// Some Twitch PubSub and Twitch API calls require the streamer account access token expiration date.
        /// </summary>
        public static DateTime TwitchStreamerTokenDate { get; set; }
        /// <summary>
        /// Determines whether the stream account access token expiration date exceeds current date, which would throw an exception for an expired token.
        /// </summary>
        public static bool TwitchStreamerValidToken => CurrentToTwitchRefreshDate(TwitchStreamerTokenDate) > new TimeSpan(0, 0, 0);

        /// <summary>
        /// Flag to indicate whether the API should use Streamer Client Id credentials per Twitch requirements.
        /// </summary>
        public static bool TwitchStreamerUseToken { get; set; }

        /// <summary>
        /// Flag to warn users of auto-moderation actions.
        /// </summary>
        public static bool ModerateUsersWarn { get; set; }
        /// <summary>
        /// Perform a bot moderation action.
        /// </summary>
        public static bool ModerateUsersAction { get; set; }
        /// <summary>
        /// Enable learning messages from the chat, to have a list of safe messages and unsafe messages needing moderation.
        /// </summary>
        public static bool ModerateUserLearnMsgs { get; set; }
        /// <summary>
        /// Specify number of minutes for when a moderator approval action will expire.
        /// </summary>
        public static int ModeratorApprovalTimeout { get; set; }

        /// <summary>
        /// Enable the Media Overlay Services.
        /// </summary>
        public static bool MediaOverlayEnabled { get; set; }
        /// <summary>
        /// Enable the Media Overlay (Twitch) channel points redemption for an overlay action.
        /// </summary>
        public static bool MediaOverlayChannelPoints { get; set; }
        /// <summary>
        /// Saves the last selected file path when specifying a file for the media overlay action.
        /// </summary>
        public static string MediaOverlayMRUPathSelect { get; set; }

        /// <summary>
        /// Enables whether shouting out a user shows a random clip from their channel
        /// </summary>
        public static bool MediaOverlayShoutoutClips { get; set; }
        public static int MediaOverlayMediaPort { get; set; }

        public static bool MediaOverlayLogExceptions { get; set; }

        public static bool MediaOverlayUseSameStyle { get; set; }
        public static bool MediaOverlayAutoStart { get; set; }

        /// <summary>
        /// First saves the settings, then reads the settings into the flag properties. Thread-Safe update.
        /// </summary>
        public static void SetSettings()
        {
            lock (Settings.Default)
            {
                Settings.Default.Save();

                #region LogAction

                LogBotStatus = Settings.Default.LogBotStatus;
                LogExceptions = Settings.Default.LogExceptions;

                #endregion

                #region Twitch

                TwitchRefreshDate = Settings.Default.TwitchRefreshDate;

                TwitchPubSubChannelPoints = Settings.Default.TwitchPubSubChannelPoints;
                TwitchPubSubOnlineMode = Settings.Default.TwitchPubSubOnlineMode;

                TwitchChannelName = Settings.Default.TwitchChannelName;
                TwitchBotUserName = Settings.Default.TwitchBotUserName;
                TwitchBotClientId = Settings.Default.TwitchClientID;
                TwitchBotAccessToken = Settings.Default.TwitchAccessToken;

                TwitchStreamClientId = Settings.Default.TwitchStreamClientId;
                TwitchStreamerTokenDate = Settings.Default.TwitchStreamerTokenDate;
                TwitchStreamOauthToken = Settings.Default.TwitchStreamOauthToken;

                TwitchStreamerUseToken = Settings.Default.TwitchBotUserName != Settings.Default.TwitchChannelName;

                TwitchRaidShoutOut = Settings.Default.TwitchRaidShoutOut;

                TwitchChatBotConnectOnline = Settings.Default.TwitchChatBotConnectOnline;
                TwitchChatBotDisconnectOffline = Settings.Default.TwitchChatBotDisconnectOffline;

                TwitchClipPostChat = Settings.Default.TwitchClipPostChat;
                TwitchClipPostDiscord = Settings.Default.TwitchClipPostDiscord;
                TwitchClipConnectOnline = Settings.Default.TwitchClipConnectOnline;
                TwitchClipDisconnectOffline = Settings.Default.TwitchClipDisconnectOffline;

                TwitchCurrencyStart = Settings.Default.TwitchCurrencyStart;
                TwitchCurrencyOnline = Settings.Default.TwitchCurrencyOnline;

                #region Followers - Twitch

                TwitchAddFollowersStart = Settings.Default.TwitchAddFollowersStart;
                TwitchPruneNonFollowers = Settings.Default.TwitchPruneNonFollowers;
                TwitchAddFollowerNotification = Settings.Default.TwitchAddFollowerNotification;
                TwitchFollowerAutoRefresh = Settings.Default.TwitchFollowerAutoRefresh;
                TwitchFollowerRefreshHrs = Settings.Default.TwitchFollowerRefreshHrs;
                TwitchFollowerEnableMsgLimit = Settings.Default.TwitchFollowerEnableMsgLimit;
                TwitchFollowerMsgLimit = Settings.Default.TwitchFollowerMsgLimit;
                TwitchFollowerAutoBanBots = Settings.Default.TwitchFollowerAutoBanBots;
                TwitchFollowerAutoBanCount = Settings.Default.TwitchFollowerAutoBanCount;
                TwitchFollowerConnectOnline = Settings.Default.TwitchFollowerConnectOnline;
                TwitchFollowerDisconnectOffline = Settings.Default.TwitchFollowerDisconnectOffline;

                #endregion

                #endregion

                #region Messages

                FirstUserJoinedMsg = Settings.Default.FirstUserJoinedMsg;
                FirstUserChatMsg = Settings.Default.FirstUserChatMsg;

                MsgAddMe = Settings.Default.MsgInsertMe;
                MsgCommandNotFound = Settings.Default.MsgCommandNotFound;
                MsgNoMe = Settings.Default.MsgNoMe;
                MsgPerComMe = Settings.Default.MsgPerComMe;

                MsgWelcomeStreamer = Settings.Default.MsgWelcomeStreamer;
                WelcomeCustomMsg = Settings.Default.WelcomeCustomMsg;

                AutoShout = Settings.Default.MsgAutoShout;
                MsgSendSOToChat = Settings.Default.MsgSendSOToChat;

                #endregion

                #region Repeat Commands

                RepeatTimer = Settings.Default.RepeatTimerCommands;
                RepeatTimerDilute = Settings.Default.RepeatTimerComSlowdown;
                RepeatWhenLive = Settings.Default.RepeatWhenLive;
                RepeatLiveReset = Settings.Default.RepeatLiveReset;
                RepeatLiveResetShow = Settings.Default.RepeatLiveResetShow;
                RepeatUserCount = Settings.Default.RepeatUserCount;
                RepeatUserMinutes = Settings.Default.RepeatUserMinutes;
                RepeatChatCount = Settings.Default.RepeatChatCount;
                RepeatChatMinutes = Settings.Default.RepeatChatMinutes;
                RepeatAboveChatCount = Settings.Default.RepeatAboveChatCount;
                RepeatAboveUserCount = Settings.Default.RepeatAboveUserCount;
                RepeatUseThresholds = Settings.Default.RepeatUseThresholds;
                RepeatNoAdjustment = Settings.Default.RepeatNoAdjustment;

                #endregion

                #region UserParty

                UserPartyStart = Settings.Default.UserPartyStart;
                UserPartyStop = Settings.Default.UserPartyStop;

                #endregion

                #region Live Messages

                PostMultiLive = Settings.Default.PostMultiLive;
                LiveMsg = Settings.Default.MsgLive;

                #endregion

                #region Data Manage

                ManageUsers = Settings.Default.ManageUsers;
                ManageFollowers = Settings.Default.ManageFollowers;
                ManageStreamStats = Settings.Default.ManageStreamStats;
                ManageRaidData = Settings.Default.ManageRaidData;
                ManageOutRaidData = Settings.Default.ManageOutRaidData;
                ManageGiveawayUsers = Settings.Default.ManageGiveawayUsers;
                ManageDataArchiveMsg = Settings.Default.ManageDataArchiveMsg;
                ManageClearButtonEnabled = Settings.Default.ManageClearButtonEnabled;
                ManageOverlayTicker = Settings.Default.ManageOverlayTicker;

                #endregion

                #region Giveaway

                GiveawayCount = Settings.Default.GiveawayCount;
                GiveawayBegMsg = Settings.Default.GiveawayBegMsg;
                GiveawayEndMsg = Settings.Default.GiveawayEndMsg;
                GiveawayWinMsg = Settings.Default.GiveawayWinMsg;
                GiveawayMultiUser = Settings.Default.GiveawayMultiUser;
                GiveawayMultiEntries = Settings.Default.GiveawayMaxEntries;

                #endregion

                #region Moderation

                ModerateUsersWarn = Settings.Default.ModerateUsersWarn;
                ModerateUsersAction = Settings.Default.ModerateUsersAction;
                ModerateUserLearnMsgs = Settings.Default.ModerateUserLearnMsgs;
                ModeratorApprovalTimeout = Settings.Default.ModeratorApprovalTimeout;

                #endregion

                #region Media Overlay 

                MediaOverlayEnabled = Settings.Default.MediaOverlayEnabled;
                MediaOverlayChannelPoints = Settings.Default.MediaOverlayChannelPoints;
                MediaOverlayMRUPathSelect = Settings.Default.MediaOverlayMRUPathSelect;
                MediaOverlayShoutoutClips = Settings.Default.MediaOverlayShoutoutClips;
                MediaOverlayMediaPort = Settings.Default.MediaOverlayPort;
                MediaOverlayLogExceptions = Settings.Default.LogExceptions;
                MediaOverlayUseSameStyle = Settings.Default.MediaOverlayUseSameStyle;
                MediaOverlayAutoStart = Settings.Default.MediaOverlayAutoStart;

                #endregion

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
