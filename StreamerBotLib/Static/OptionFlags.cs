using StreamerBotLib.Properties;

using System.Configuration;
using System.Reflection;

namespace StreamerBotLib.Static
{
    /// <summary>
    /// Holds settings and flags for access across the application.
    /// </summary>
    public static class OptionFlags
    {
        /// <summary>
        /// Indicates database xml file successfully loaded.
        /// </summary>
        public static bool DataLoaded { get; set; } = false;
        /// <summary>
        /// Indicates database xml file successfully loaded.
        /// </summary>
        public static bool MultiDataLoaded { get; set; } = false;
        /// <summary>
        /// The app-wide token to keep threads running, setting it false indicates it's all shut down to close the application.
        /// </summary>
        public static bool ActiveToken { get; set; } = false;
        /// <summary>
        /// Specifies the watched stream is online, to permit operations to perform only during a livestream.
        /// </summary>
        public static bool IsStreamOnline { get; set; } = false;

        /// <summary>
        /// Flags whether the streamer channel ProcessFollowQueuestarted a raid - could be anyone based on command rights
        /// </summary>
        public static bool TwitchOutRaidStarted { get; set; } = false;
        /// <summary>
        /// Determines whether the stream account access token expiration date exceeds current date, which would throw an exception for an expired token.
        /// </summary>
        public static bool TwitchStreamerValidToken => CurrentToTwitchRefreshDate(TwitchStreamerTokenDate) > new TimeSpan(0, 0, 0);
        /// <summary>
        /// Flag to indicate whether the API should use Streamer Client Id credentials per Twitch requirements.
        /// </summary>
        public static bool TwitchStreamerUseToken => Settings.Default.TwitchBotUserName != Settings.Default.TwitchChannelName;

        /// <summary>
        /// User specified in the GUI, the user selects whether they want to use the "Twitch Authentication Code flow"
        /// to automatically get access tokens as the token expires.
        /// </summary>
        public static bool TwitchTokenUseAuth => Settings.Default.TwitchTokenUseAuth;

        #region Twitch Credential Scopes

        /// <summary>
        /// Retrieves the Twitch Credential Scope Resource for when using a bot account different than streamer account - this is the bot access token scope listing
        /// </summary>
        public static string CredentialsTwitchScopesDiffOauthBot => Resources.CredentialsTwitchScopesDiffOauthBot;

        /// <summary>
        /// Retrieves the Twitch Credential Scope Resource for using a bot account different than streamer account - this is the channel access token scope listing
        /// </summary>
        public static string CredentialsTwitchScopesDiffOauthChannel => Resources.CredentialsTwitchScopesDiffOauthChannel;

        /// <summary>
        /// Retrieves the Twitch Credential Scope Resource for using a bot account the same as streamer account - this is the full access token scope listing
        /// </summary>
        public static string CredentialsTwitchScopesOauthSame => Resources.CredentialsTwitchScopesOauthSame;

        #endregion

        /// <summary>
        /// Specifies whether to record bot status messages in the log file.
        /// </summary>
        public static bool LogBotStatus => Settings.Default.LogBotStatus;
        /// <summary>
        /// Specifies whether to record exceptions during bot operation.
        /// </summary>
        public static bool LogExceptions => Settings.Default.LogExceptions;
        /// <summary>
        /// Specifies to add Twitch followers for the given channel upon starting the bot.
        /// </summary>
        public static bool TwitchAddFollowersStart => Settings.Default.TwitchAddFollowersStart;
        /// <summary>
        /// Specifies to clear Twitch followers in the database if they are no longer followers.
        /// </summary>
        public static bool TwitchPruneNonFollowers
        {
            get => Settings.Default.TwitchPruneNonFollowers; set => Settings.Default.TwitchPruneNonFollowers = value;
        }
        /// <summary>
        /// Turns off notification during the bulk follower add operation, prevents sending chats to the channel.
        /// </summary>
        public static bool TwitchAddFollowerNotification => Settings.Default.TwitchAddFollowerNotification;
        /// <summary>
        /// Enables an auto refresh for adding Twitch followers again after a certain amount of time, since Twitch doesn't provide notification when a user no longer follows a channel.
        /// </summary>
        public static bool TwitchFollowerAutoRefresh => Settings.Default.TwitchFollowerAutoRefresh;
        /// <summary>
        /// Specifies the hours interval to check and add all Twitch followers regarding the given channel.
        /// </summary>
        public static int TwitchFollowerRefreshHrs => Settings.Default.TwitchFollowerRefreshHrs;
        /// <summary>
        /// Enables mechanisms to limit the number of messages the bot sends to the channel when a number of new followers occurs within the followers polling time.
        /// </summary>
        public static bool TwitchFollowerEnableMsgLimit => Settings.Default.TwitchFollowerEnableMsgLimit;
        /// <summary>
        /// Specifies the 1-100 limit of new followers, after which individual messages are condensed into fewer follower messages.
        /// </summary>
        public static int TwitchFollowerMsgLimit => Settings.Default.TwitchFollowerMsgLimit;
        /// <summary>
        /// Setting to ban bots performing Twitch follow storms - hundreds of follows in a few seconds.
        /// </summary>
        public static bool TwitchFollowerAutoBanBots => Settings.Default.TwitchFollowerAutoBanBots;
        /// <summary>
        /// Specifies threshold above which is considered a bot follow storm, and invokes banning the accounts included within the follow storm.
        /// </summary>
        public static int TwitchFollowerAutoBanCount => Settings.Default.TwitchFollowerAutoBanCount;
        /// <summary>
        /// Specifies whether bot starts when stream is detected online.
        /// </summary>
        public static bool TwitchFollowerConnectOnline => Settings.Default.TwitchFollowerConnectOnline;
        /// <summary>
        /// Specifies whether bot stops when stream is detected offline.
        /// </summary>
        public static bool TwitchFollowerDisconnectOffline => Settings.Default.TwitchFollowerDisconnectOffline;

        /// <summary>
        /// Specifies whether to welcome a user when they first join the given channel.
        /// </summary>
        public static bool FirstUserJoinedMsg
        {
            get => Settings.Default.FirstUserJoinedMsg; set => Settings.Default.FirstUserJoinedMsg = value;
        }
        /// <summary>
        /// Specifies whether to welcome a user when they first chat in the given (live) channel.
        /// </summary>
        public static bool FirstUserChatMsg
        {
            get => Settings.Default.FirstUserChatMsg; set => Settings.Default.FirstUserChatMsg = value;
        }

        /// <summary>
        /// The datetime of the earliest expiring Twitch token, to help limit operations and prevent crashing from using an outdated access token.
        /// </summary>
        public static DateTime TwitchRefreshDate => Settings.Default.TwitchRefreshDate;

        /// <summary>
        /// Adds /me to all outgoing bot messages.
        /// </summary>
        public static bool MsgAddMe => Settings.Default.MsgInsertMe;

        /// <summary>
        /// Enable the bot to emit "Command Not Found" response message when command is not found.
        /// </summary>
        public static bool MsgCommandNotFound => Settings.Default.MsgCommandNotFound;
        /// <summary>
        /// Does not include /me on any message.
        /// </summary>
        public static bool MsgNoMe => Settings.Default.MsgNoMe;
        /// <summary>
        /// Adds /me per each command setting in the database.
        /// </summary>
        public static bool MsgPerComMe => Settings.Default.MsgPerComMe;

        /// <summary>
        /// Specifies whether to not welcome the streamer if they type a message.
        /// </summary>
        public static bool MsgWelcomeStreamer => Settings.Default.MsgWelcomeStreamer;
        /// <summary>
        /// Enables sending the social messages separately to the chat channel.
        /// </summary>
        public static bool MsgSocialSeparate => Settings.Default.MsgSocialSeparate;
        /// <summary>
        /// Enables a custom welcome message depending on the user entering the channel, is active alongside the welcome user setting.
        /// </summary>
        public static bool WelcomeCustomMsg => Settings.Default.WelcomeCustomMsg;
        /// <summary>
        /// Auto shout specific users, per the welcome user setting (when they first join or when they first chat).
        /// </summary>
        public static bool AutoShout => Settings.Default.MsgAutoShout;
        /// <summary>
        /// Setting for the bot to emit the "!so Username" to the given channel chat, such that other bots can pick-up a shout-out for additional notifications.
        /// </summary>
        public static bool MsgSendSOToChat => Settings.Default.MsgSendSOToChat;
        /// <summary>
        /// Enables whether the bot should shout out all users who raid the given channel.
        /// </summary>
        public static bool TwitchRaidShoutOut => Settings.Default.TwitchRaidShoutOut;
        /// <summary>
        /// Enables repeat timer commands.
        /// </summary>
        public static bool RepeatTimerCommands => Settings.Default.RepeatTimerCommands;
        /// <summary>
        /// Whether to dilute the repeat timer commands, they'll run at a later time to not clog up the channel's chat.
        /// </summary>
        public static bool RepeatTimerComSlowdown => Settings.Default.RepeatTimerComSlowdown;
        /// <summary>
        /// Setting for not adjusting repeat timers or limiting messages.
        /// </summary>
        public static bool RepeatNoAdjustment => Settings.Default.RepeatNoAdjustment;
        /// <summary>
        /// Setting for repeat timers to send messages based on thresholds.
        /// </summary>
        public static bool RepeatUseThresholds => Settings.Default.RepeatUseThresholds;
        /// <summary>
        /// Setting to use the users threadhold, whether to send repeat messages.
        /// </summary>
        public static bool RepeatAboveUserCount => Settings.Default.RepeatAboveUserCount;
        /// <summary>
        /// Setting to use the chat count threshold, whether to send repeat messages.
        /// </summary>
        public static bool RepeatAboveChatCount => Settings.Default.RepeatAboveChatCount;
        /// <summary>
        /// Enables performing the repeat timer commands only when live, otherwise the messages would run all the time.
        /// </summary>
        public static bool RepeatWhenLive => Settings.Default.RepeatWhenLive;
        /// <summary>
        /// Enables resetting the repeat times once the stream is live, as a basis for how much time to elapse before the next repeat. For when the bot has been on for days.
        /// </summary>
        public static bool RepeatLiveReset => Settings.Default.RepeatLiveReset;  // reset repeat timers when stream goes live
        /// <summary>
        /// Enables actually sending the message to chat once the times are reset once the given channel is live.
        /// </summary>
        public static bool RepeatLiveResetShow => Settings.Default.RepeatLiveResetShow;   // enable showing message when repeat timers reset for going live
        /// <summary>
        /// The about of minutes to check the number of users for the dilution calculation.
        /// </summary>
        public static int RepeatUserMinutes => Settings.Default.RepeatUserMinutes;
        /// <summary>
        /// The amount of users in the time interval, under this threshold is in dilution calculation.
        /// </summary>
        public static int RepeatUserCount => Settings.Default.RepeatUserCount;
        /// <summary>
        /// The time interval to check the number of chats for the time dultion calculation.
        /// </summary>
        public static int RepeatChatMinutes => Settings.Default.RepeatChatMinutes;
        /// <summary>
        /// The amount of chat in time interval, under this threshold is in dilution calculation.
        /// </summary>
        public static int RepeatChatCount => Settings.Default.RepeatChatCount;


        /// <summary>
        /// Indicates the user join list is active to accept users to the join list.
        /// </summary>
        public static bool UserPartyStart => Settings.Default.UserPartyStart;
        /// <summary>
        /// Stops the user join list, doesn't accept any more join requests.
        /// </summary>
        public static bool UserPartyStop => Settings.Default.UserPartyStop;

        /// <summary>
        /// Specifies if the MultiLive bot autostarts when application opens, standalone app isn't running, and LiveMonitor bot is ProcessFollowQueuestarted.
        /// </summary>
        public static bool TwitchMultiLiveAutoStart => Settings.Default.TwitchMultiLiveAutoStart;

        /// <summary>
        /// Enables or disables posting multiple live messages to social media on the same day, i.e. the stream crashes and restarts and another 'Live' alert is posted.
        /// </summary>
        public static bool PostMultiLive => Settings.Default.PostMultiLive;

        /// <summary>
        /// Whether to display the bot welcome message when connecting to the channel.
        /// </summary>
        public static bool MsgBotConnection => Settings.Default.MsgBotConnection;
        /// <summary>
        /// The social media message to use to notify the given channel is live.
        /// </summary>
        public static string MsgLive => Settings.Default.MsgLive;

        /// <summary>
        /// Specifies actually hiding the "Manage Data" options section in the GUI to prevent accidental clicking.
        /// </summary>
        public static bool EnableManageDataOptions => Settings.Default.EnableManageDataOptions;
        /// <summary>
        /// Specifies to save user data in the database.
        /// </summary>
        public static bool ManageUsers
        {
            get => Settings.Default.ManageUsers; set => Settings.Default.ManageUsers = value;
        }
        /// <summary>
        /// Specifies to save follower data in the database.
        /// </summary>
        public static bool ManageFollowers
        {
            get => Settings.Default.ManageFollowers; set => Settings.Default.ManageFollowers = value;
        }
        /// <summary>
        /// Specifies to save stream statistic data in the database.
        /// </summary>
        public static bool ManageStreamStats
        {
            get => Settings.Default.ManageStreamStats; set => Settings.Default.ManageStreamStats = value;
        }
        /// <summary>
        /// Specifies to save incoming raid data in the database.
        /// </summary>
        public static bool ManageRaidData
        {
            get => Settings.Default.ManageRaidData; set => Settings.Default.ManageRaidData = value;
        }
        /// <summary>
        /// Specifies to save outgoing raid data in the database.
        /// </summary>
        public static bool ManageOutRaidData
        {
            get => Settings.Default.ManageOutRaidData; set => Settings.Default.ManageOutRaidData = value;
        }
        /// <summary>
        /// Specifies to save giveaway data in the database.
        /// </summary>
        public static bool ManageGiveawayUsers
        {
            get => Settings.Default.ManageGiveawayUsers; set => Settings.Default.ManageGiveawayUsers = value;
        }
        /// <summary>
        /// Specifies to user about they need to manage archiving the saved data.
        /// </summary>
        public static bool ManageDataArchiveMsg => Settings.Default.ManageDataArchiveMsg;
        /// <summary>
        /// Enables 'clear data' buttons in the GUI, to prevent accidental clicking.
        /// </summary>
        public static bool ManageClearButtonEnabled => Settings.Default.ManageClearButtonEnabled;

        /// <summary>
        /// Specifies to save OverlayTicker data in the database.
        /// </summary>
        public static bool ManageOverlayTicker => Settings.Default.ManageOverlayTicker;

        /// <summary>
        /// Enables starting the Twitch Chat Bot once the stream goes live. Depends on the live service detecting going live.
        /// </summary>
        public static bool TwitchChatBotConnectOnline => Settings.Default.TwitchChatBotConnectOnline;
        /// <summary>
        /// Enables whether to stop Twitch Chat Bot once the stream goes offline.
        /// </summary>
        public static bool TwitchChatBotDisconnectOffline => Settings.Default.TwitchChatBotDisconnectOffline;

        /// <summary>
        /// Specifies whether to post a channel clip link to the given channel chat.
        /// </summary>
        public static bool TwitchClipPostChat
        {
            get => Settings.Default.TwitchClipPostChat; set => Settings.Default.TwitchClipPostChat = value;
        }

        /// <summary>
        /// Manages the user specified frequency to check for clips.
        /// </summary>
        public static double TwitchFrequencyClipTime => Settings.Default.TwitchFrequencyClipTime;
        /// <summary>
        /// Specifies whether to post a channel clip link to WebHooks, and WebHooks webhooks need a 'clips' link.
        /// </summary>
        public static bool TwitchClipPostDiscord => Settings.Default.TwitchClipPostDiscord;
        /// <summary>
        /// Specifies whether the clip bot connects when the stream is detected online.
        /// </summary>
        public static bool TwitchClipConnectOnline => Settings.Default.TwitchClipConnectOnline;
        /// <summary>
        /// Specifies whether the clip bot disconnects when the stream is detected offline.
        /// </summary>
        public static bool TwitchClipDisconnectOffline => Settings.Default.TwitchClipDisconnectOffline;

        #region Currency
        /// <summary>
        /// Specifies starting currency accruals.
        /// </summary>
        public static bool CurrencyStart => Settings.Default.CurrencyStart;
        /// <summary>
        /// Specifies whether currency accrual should only occur when stream is live (online), for testing purposes.
        /// </summary>
        public static bool CurrencyOnline => Settings.Default.CurrencyOnline;

        /// <summary>
        /// Specifies the value where the House stands.
        /// </summary>
        public static int GameBlackJackHouseStands => Settings.Default.GameBlackJackHouseStands;
        /// <summary>
        /// Specifies the payout for a player dealt 21.
        /// </summary>
        public static int GameBlackJackPayoutDealt21 => Settings.Default.GameBlackJackPayoutDealt21;
        /// <summary>
        /// Specifies the payout for a player reaching 21.
        /// </summary>
        public static int GameBlackJackPayoutReach21 => Settings.Default.GameBlackJackPayoutReach21;
        /// <summary>
        /// Specifies the payout for a player stopping under 21.
        /// </summary>
        public static int GameBlackJackPayoutUnder21 => Settings.Default.GameBlackJackPayoutUnder21;
        /// <summary>
        /// Specifies the payout message when a player wins.
        /// </summary>
        public static string GameBlackJackPayoutMessage => Settings.Default.GameBlackJackPayoutMessage;

        #endregion Currency

        /// <summary>
        /// Specifies how many selections to make for the giveaway.
        /// </summary>
        public static short GiveawayCount
        {
            get => Settings.Default.GiveawayCount; set => Settings.Default.GiveawayCount = value;
        }
        /// <summary>
        /// The message to send the channel chat for a user winning the giveaway.
        /// </summary>
        public static string GiveawayWinMsg
        {
            get => Settings.Default.GiveawayWinMsg; set => Settings.Default.GiveawayWinMsg = value;
        }
        /// <summary>
        /// The message for when the giveaway starts/begins.
        /// </summary>
        public static string GiveawayBegMsg
        {
            get => Settings.Default.GiveawayBegMsg; set => Settings.Default.GiveawayBegMsg = value;
        }
        /// <summary>
        /// The message for when the giveaway ends.
        /// </summary>
        public static string GiveawayEndMsg
        {
            get => Settings.Default.GiveawayEndMsg; set => Settings.Default.GiveawayEndMsg = value;
        }
        /// <summary>
        /// Determines if the user can submit multiple entries.
        /// </summary>
        public static bool GiveawayMultiUser
        {
            get => Settings.Default.GiveawayMultiUser; set => Settings.Default.GiveawayMultiUser = value;
        }
        /// <summary>
        /// The number of giveaway entries a user can submit.
        /// </summary>
        public static short GiveawayMaxEntries
        {
            get => Settings.Default.GiveawayMaxEntries; set => Settings.Default.GiveawayMaxEntries = value;
        }

        /// <summary>
        /// Specifies Twitch Pub Sub scope, to include Channel Points.
        /// </summary>
        public static bool TwitchPubSubChannelPoints => Settings.Default.TwitchPubSubChannelPoints;
        /// <summary>
        /// Enables the PubSub to start when the stream is online and stop when the stream is offline.
        /// </summary>
        public static bool TwitchPubSubOnlineMode => Settings.Default.TwitchPubSubOnlineMode;

        /// <summary>
        /// Specifies the Twitch Channel Name to monitor.
        /// </summary>
        public static string TwitchChannelName
        {
            get => Settings.Default.TwitchChannelName; set => Settings.Default.TwitchChannelName = value;
        }
        /// <summary>
        /// Specifies the Twitch Bot User Name for this application to chat through.
        /// </summary>
        public static string TwitchBotUserName
        {
            get => Settings.Default.TwitchBotUserName; set => Settings.Default.TwitchBotUserName = value;
        }
        /// <summary>
        /// Captures prior channel name to detect if the user is monitoring another channel, hence, requiring new channel user id.
        /// </summary>
        public static string TwitchPriorChannelName
        {
            get => Settings.Default.TwitchPriorChannelName; set => Settings.Default.TwitchPriorChannelName = value;
        }
        /// <summary>
        /// Captures prior bot account name to detect if user changed bot accounts, hence, requiring a new bot user id.
        /// </summary>
        public static string TwitchPriorBotName
        {
            get => Settings.Default.TwitchPriorBotName; set => Settings.Default.TwitchPriorBotName = value;
        }
        /// <summary>
        /// Holds the latest used bot user id, to minimize api calls.
        /// </summary>
        public static string TwitchBotUserId
        {
            get => Settings.Default.TwitchBotUserId; set => Settings.Default.TwitchBotUserId = value;
        }

        /// <summary>
        /// Holds the latest used streamer user id, to minimize API calls.
        /// </summary>
        public static string TwitchStreamerUserId
        {
            get => Settings.Default.TwitchChannelUserId; set => Settings.Default.TwitchChannelUserId = value;
        }
        /// <summary>
        /// Specifies the bot account client ID, to use in authentication calls.
        /// </summary>
        public static string TwitchBotClientId
        {
            get => Settings.Default.TwitchClientID; set => Settings.Default.TwitchClientID = value;
        }
        /// <summary>
        /// Specifies the bot account access token, used in authentication calls.
        /// </summary>
        public static string TwitchBotAccessToken
        {
            get => Settings.Default.TwitchAccessToken; set => Settings.Default.TwitchAccessToken = value;
        }

        /// <summary>
        /// Manages the Twitch Refresh Token.
        /// </summary>
        public static string TwitchRefreshToken
        {
            get => Settings.Default.TwitchRefreshToken;
            set => Settings.Default.TwitchRefreshToken = value;
        }

        /// <summary>
        /// Some Twitch PubSub and Twitch API calls require the streamer account client ID, to perform the function call.
        /// </summary>
        public static string TwitchStreamClientId => Settings.Default.TwitchStreamClientId;
        /// <summary>
        /// Some Twitch PubSub and Twitch API calls require the streamer account access token, to perform the function call.
        /// </summary>
        public static string TwitchStreamOauthToken
        {
            get => Settings.Default.TwitchStreamOauthToken;
            set => Settings.Default.TwitchStreamOauthToken = value;
        }

        /// <summary>
        /// Manages the Twitch Streamer based Refresh Token
        /// </summary>
        public static string TwitchStreamRefreshToken
        {
            get => Settings.Default.TwitchStreamerRefreshToken;
            set => Settings.Default.TwitchStreamerRefreshToken = value;
        }

        /// <summary>
        /// Some Twitch PubSub and Twitch API calls require the streamer account access token expiration date.
        /// </summary>
        public static DateTime TwitchStreamerTokenDate => Settings.Default.TwitchStreamerTokenDate;

        #region Twitch Authorization code flow

        /// <summary>
        /// Retrieve the URL used to retrieve the Auth Code, through the auth code grant flow
        /// </summary>
        public static string TwitchAuthRedirectURL => Settings.Default.TwitchAuthRedirectURL;
        /// <summary>
        /// Refers to the bot client Id, from app registration.
        /// </summary>
        public static string TwitchAuthClientId => Settings.Default.TwitchAuthClientId;
        /// <summary>
        /// Holds the bot account access token obtained in the 'authorization code flow' method to receive an access token.
        /// </summary>
        public static string TwitchAuthBotAccessToken
        {
            get => Settings.Default.TwitchAuthBotAccessToken;
            set => Settings.Default.TwitchAuthBotAccessToken = value;
        }
        /// <summary>
        /// Holds the bot account refresh token obtained in the 'authorization code flow' method to receive an access token.
        /// </summary>
        public static string TwitchAuthBotRefreshToken
        {
            get => Settings.Default.TwitchAuthBotRefreshToken;
            set => Settings.Default.TwitchAuthBotRefreshToken = value;
        }
        /// <summary>
        /// Holds the bot account user authorized code obtained in the 'authorization code flow' method
        /// </summary>
        public static string TwitchAuthBotAuthCode
        {
            get => Settings.Default.TwitchAuthBotAuthCode;
            set => Settings.Default.TwitchAuthBotAuthCode = value;
        }
        /// <summary>
        /// Holds the bot account client secret obtained in the 'authorization code flow' method to receive an access token.
        /// </summary>
        public static string TwitchAuthBotClientSecret => Settings.Default.TwitchAuthBotClientSecret;
        /// <summary>
        /// Refers to the streamer account based client Id, from app registration.
        /// </summary>
        public static string TwitchAuthStreamerClientId => Settings.Default.TwitchAuthStreamerClientId;
        /// <summary>
        /// Holds the streamer account access token obtained in the 'authorization code flow' method to receive an access token.
        /// </summary>
        public static string TwitchAuthStreamerAccessToken
        {
            get => Settings.Default.TwitchAuthStreamerAccessToken;
            set => Settings.Default.TwitchAuthStreamerAccessToken = value;
        }
        /// <summary>
        /// Holds the streamer account refresh token obtained in the 'authorization code flow' method to receive an access token.
        /// </summary>
        public static string TwitchAuthStreamerRefreshToken
        {
            get
            {
                return Settings.Default.TwitchAuthStreamerRefreshToken;
            }
            set
            {
                Settings.Default.TwitchAuthStreamerRefreshToken = value;
            }
        }
        /// <summary>
        /// Holds the streamer account auth code obtained in the 'authorization code flow' method to receive an access token
        /// </summary>
        public static string TwitchAuthStreamerAuthCode
        {
            get => Settings.Default.TwitchAuthStreamerAuthCode;
            set => Settings.Default.TwitchAuthStreamerAuthCode = value;
        }
        /// <summary>
        /// Holds the streamer account client secret obtained in the 'authorization code flow' method to receive an access token.
        /// </summary>
        public static string TwitchAuthStreamerClientSecret => Settings.Default.TwitchAuthStreamerClientSecret;
        /// <summary>
        /// Flag on whether to use the internal web browser for the authentication code process
        /// </summary>
        public static bool TwitchAuthUseInternalBrowser => Settings.Default.TwitchAuthUseInternalBrowser;
        #endregion

        /// <summary>
        /// The user specified time frequency for when to check Twitch for new followers.
        /// </summary>
        public static double TwitchFrequencyFollow => Settings.Default.TwitchFrequencyFollow;
        /// <summary>
        /// The user specified time frequency for when to check Twitch for streams appearing online.
        /// </summary>
        public static double TwitchGoLiveFrequency => Settings.Default.TwitchGoLiveFrequency;

        /// <summary>
        /// Flag to warn users of auto-moderation actions.
        /// </summary>
        public static bool ModerateUsersWarn => Settings.Default.ModerateUsersWarn;
        /// <summary>
        /// Perform a bot moderation action.
        /// </summary>
        public static bool ModerateUsersAction => Settings.Default.ModerateUsersAction;
        /// <summary>
        /// Enable learning messages from the chat, to have a list of safe messages and unsafe messages needing moderation.
        /// </summary>
        public static bool ModerateUserLearnMsgs => Settings.Default.ModerateUserLearnMsgs;
        /// <summary>
        /// Specify number of minutes for when a moderator approval action will expire.
        /// </summary>
        public static int ModeratorApprovalTimeout => Settings.Default.ModeratorApprovalTimeout;

        /// <summary>
        /// Specifies whether the Overlay Server should start and stop when the user stream is online or offline.
        /// </summary>
        public static bool MediaOverlayStartWithStream => Settings.Default.MediaOverlayStartWithStream;

        /// <summary>
        /// Enable the Media Overlay Services.
        /// </summary>
        public static bool MediaOverlayEnabled
        {
            get => Settings.Default.MediaOverlayEnabled; set => Settings.Default.MediaOverlayEnabled = value;
        }
        /// <summary>
        /// Enable the Media Overlay (Twitch) channel points redemption for an overlay action.
        /// </summary>
        public static bool MediaOverlayChannelPoints
        {
            get => Settings.Default.MediaOverlayChannelPoints; set => Settings.Default.MediaOverlayChannelPoints = value;
        }
        /// <summary>
        /// Saves the last selected file path when specifying a file for the media overlay action.
        /// </summary>
        public static string MediaOverlayMRUPathSelect
        {
            get => Settings.Default.MediaOverlayMRUPathSelect; set => Settings.Default.MediaOverlayMRUPathSelect = value;
        }

        /// <summary>
        /// Enables whether shouting out a user shows a random clip from their channel
        /// </summary>
        public static bool MediaOverlayShoutoutClips
        {
            get => Settings.Default.MediaOverlayShoutoutClips; set => Settings.Default.MediaOverlayShoutoutClips = value;
        }
        /// <summary>
        /// Manages the Overlay action media port serving Overlay alert Content through the http server.
        /// </summary>
        public static short MediaOverlayMediaActionPort
        {
            get => Settings.Default.MediaOverlayActionPort; set => Settings.Default.MediaOverlayActionPort = value;
        }

        /// <summary>
        /// Manages the Overlay ticker medition port server Overlay ticker item Content through the http server.
        /// </summary>
        public static short MediaOverlayMediaTickerPort
        {
            get => Settings.Default.MediaOverlayTickerPort; set => Settings.Default.MediaOverlayTickerPort = value;
        }

        /// <summary>
        /// Manages option to log exceptions within the Media Overlay server - was separate when Overlay Server was its own package.
        /// </summary>
        public static bool MediaOverlayLogExceptions => Settings.Default.MediaOverlayLogExceptions;

        /// <summary>
        /// Manages if user wants action alerts to display with the same styling.
        /// </summary>
        public static bool MediaOverlayUseSameStyle => Settings.Default.MediaOverlayUseSameStyle;
        /// <summary>
        /// Manages user preference to auto start the Overlay bot upon application start.
        /// </summary>
        public static bool MediaOverlayAutoStart => Settings.Default.MediaOverlayAutoStart;
        /// <summary>
        /// Manages user preference whether the Overlay http server should start upon the Overlay bot start.
        /// </summary>
        public static bool MediaOverlayAutoServerStart => Settings.Default.MediaOverlayAutoServerStart;

        /// <summary>
        /// Defines using the ticker in single pages for each ticker item
        /// </summary>
        public static bool MediaOverlayTickerSingle => Settings.Default.MediaOverlayTickerSingle;
        /// <summary>
        /// Defines using the ticker with multiple items visible
        /// </summary>
        public static bool MediaOverlayTickerMulti => Settings.Default.MediaOverlayTickerMulti;

        /// <summary>
        /// Defines a ticker where all elements stand in place
        /// </summary>
        public static bool MediaOverlayTickerStatic => Settings.Default.MediaOverlayTickerStatic;
        /// <summary>
        /// Defines rotating each ticker item in the same spot
        /// </summary>
        public static bool MediaOverlayTickerRotate => Settings.Default.MediaOverlayTickerRotate;
        public static int MediaOverlayTickerRotateTime => Settings.Default.MediaOverlayTickerRotateTime;
        /// <summary>
        /// Defines a marquee ticker scroller
        /// </summary>
        public static bool MediaOverlayTickerMarquee => Settings.Default.MediaOverlayTickerMarquee;
        /// <summary>
        /// Manages the user preference for how fast the ticker scrolling occurs.
        /// </summary>
        public static int MediaOverlayTickerMarqueeTime => Settings.Default.MediaOverlayTickerMarqueeTime;

        /// <summary>
        /// Saves the user's preference for what to show in the Overlay ticker
        /// </summary>
        public static string[] MediaOverlayTickerSelected
        {
            get => (Settings.Default.MediaOverlayTickerSelected).Split('_');
            set => Settings.Default.MediaOverlayTickerSelected = string.Join('_', value);
        }

        /// <summary>
        /// Specifies whether user wants the UserData->User Follow tab to adjust its layout when the width changes
        /// </summary>
        public static bool GridTabifyUserFollow => Settings.Default.GridTabifyUserFollow;

        /// <summary>
        /// Specifies the user's 'User Follow tab' width threshold for when 'tabify' activates, this width or smaller.
        /// </summary>
        public static int GridTabifyUserFollowWidth => Settings.Default.GridTabifyUserFollowWidth;

        /// <summary>
        /// Specifies whether the user wants the StreamData->Raids tab to adjust its layout when the width changes
        /// </summary>
        public static bool GridTabifyStreamRaids => Settings.Default.GridTabifyStreamRaids;

        /// <summary>
        /// Specifies the user's 'Raid tab' width threshold for when 'tabify' activates, this width or smaller.
        /// </summary>
        public static int GridTabifyStreamRaidsWidth => Settings.Default.GridTabifyStreamRaidsWidth;

        #region Themes

        // specify properties as "Theme[theme name]", e.g. "ThemeDark". reverse the name for the theme file, e.g. "DarkTheme.xaml"
        // parsers use the properties here to reconstruct the file name to use for selecting the theme

        /// <summary>
        /// This prefix starts the name of all theme properties.
        /// </summary>
        public static string PrefixForThemes => "Theme";

        /// <summary>
        /// Specifies to use the Light Theme within the app
        /// </summary>
        public static bool ThemeLight => Settings.Default.ThemeLight;

        /// <summary>
        /// Specifies to use the Darkl Theme within the app
        /// </summary>
        public static bool ThemeDark => Settings.Default.ThemeDark;

        #endregion Themes

        #region DebugLogFlags

        /// <summary>
        /// Enables the Overlay actions to save to the debug log.
        /// </summary>
        public static bool EnableDebugLogOverlays => Settings.Default.EnableDebugLogOverlays;
        /// <summary>
        /// Enables the DataManager actions to save to the debug log.
        /// </summary>
        public static bool EnableDebugDataManager => Settings.Default.EnableDebugDataManager;
        /// <summary>
        /// Enables the Twitch Chat Bot related actions to save to the debug log.
        /// </summary>
        public static bool EnableDebugTwitchChatBot => Settings.Default.EnableDebugTwitchChatBot;
        /// <summary>
        /// Enables the Twitch Clip Bot related actions to save to the debug log.
        /// </summary>
        public static bool EnableDebugTwitchClipBot => Settings.Default.EnableDebugTwitchClipBot;
        /// <summary>
        /// Enables the Twitch Live Bot related actions to save to the debug log.
        /// </summary>
        public static bool EnableDebugTwitchLiveBot => Settings.Default.EnableDebugTwitchLiveBot;
        /// <summary>
        /// Enables the Twitch Follow Bot related actions to save to the debug log.
        /// </summary>
        public static bool EnableDebugTwitchFollowBot => Settings.Default.EnableDebugTwitchFollowBot;
        /// <summary>
        /// Enables the Twitch PubSub Bot related actions to save to the debug log.
        /// </summary>
        public static bool EnableDebugTwitchPubSubBot => Settings.Default.EnableDebugTwitchPubSubBot;
        /// <summary>
        /// Enables debug logging for Twitch bot User Service methods.
        /// </summary>
        public static bool EnableDebugTwitchUserSvcBot => Settings.Default.EnableDebugTwitchUserSvcBot;
        /// <summary>
        /// Enables the Media-WebHooks (may change to be more generic) Bot- related actions to save to the debug log.
        /// </summary>
        public static bool EnableDebugDiscordBot => Settings.Default.EnableDebugDiscordBot;
        public static bool EnableDebugTwitchTokenBot => Settings.Default.EnableDebugTwitchTokenBot;
        public static bool EnableDebugCommandSystem => Settings.Default.EnableDebugCommandSystem;
        public static bool EnableDebugCommonSystem => Settings.Default.EnableDebugCommonSystem;
        public static bool EnableDebugSystemController => Settings.Default.EnableDebugSystemController;
        public static bool EnableDebugStatSystem => Settings.Default.EnableDebugStatSystem;
        public static bool EnableDebugBotController => Settings.Default.EnableDebugBotController;
        public static bool EnableDebugTwitchMultiLiveBot => Settings.Default.EnableDebugTwitchMultiLiveBot;
        public static bool EnableDebugCurrencySystem => Settings.Default.EnableDebugCurrencySystem;
        public static bool EnableDebugModerationSystem => Settings.Default.EnableDebugModerationSystem;
        public static bool EnableDebugOverlaySystem => Settings.Default.EnableDebugOverlaySystem;
        public static bool EnableDebugBlackjackGame => Settings.Default.EnableDebugBlackjackGame;
        public static bool EnableDebugLocalizedMessages => Settings.Default.EnableDebugLocalizedMessages;
        public static bool EnableDebugThreadManager => Settings.Default.EnableDebugThreadManager;
        public static bool EnableDebugFormatData => Settings.Default.EnableDebugFormatData;
        public static bool EnableDebugOutputMsgParsing => Settings.Default.EnableDebugOutputMsgParsing;
        public static bool EnableDebugGUIProcessWatcher => Settings.Default.EnableDebugGUIProcessWatcher;
        public static bool EnableDebugGUITabSizes => Settings.Default.EnableDebugGUITabSizes;
        public static bool EnableDebugGUIThemes => Settings.Default.EnableDebugGUIThemes;
        public static bool EnableDebugGUITwitchTokenAuth => Settings.Default.EnableDebugGUITwitchTokenAuth;
        public static bool EnableDebugGUIEvents => Settings.Default.EnableDebugGUIEvents;
        public static bool EnableDebugGUIHelpers => Settings.Default.EnableDebugGUIHelpers;
        public static bool EnableDebugGUIDataViews => Settings.Default.EnableDebugGUIDataViews;
        public static bool EnableDebugGUIBotComs => Settings.Default.EnableDebugGUIBotComs;
        public static bool EnableDebugTwitchBots => Settings.Default.EnableDebugTwitchBots;


        #endregion

        #region GitHub links

        /// <summary>
        /// Specifies the GitHub link found for the latest stable package version, for this application
        /// </summary>
        public static string GitHubCheckStable
        {
            get => Settings.Default.GitHubCheckStable;
            set => Settings.Default.GitHubCheckStable = value;
        }
        /// <summary>
        /// Specifies the GitHub link to the stable application version.
        /// </summary>
        public static string GitHubStableLink => Resources.GitHubStableLink;
        /// <summary>
        /// Specifies the GitHub link to the latest application version.
        /// </summary>
        public static string GitHubLatestLink => Resources.GitHubLatestLink;
        /// <summary>
        /// Specifies the GitHub link to the application Wiki
        /// </summary>
        public static string GitHubWikiLink => Resources.GitHubWikiLink;

        #endregion

        /// <summary>
        /// Sets whether the "Join Party" list is ProcessFollowQueuestarted or not, and refreshes the settings.
        /// </summary>
        /// <param name="Start">Whether the party is ProcessFollowQueuestarted.</param>
        public static void SetParty(bool Start = true)
        {
            Settings.Default.UserPartyStart = Start;
            Settings.Default.UserPartyStop = !Start;
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
