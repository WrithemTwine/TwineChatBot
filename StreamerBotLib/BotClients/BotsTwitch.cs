using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.Culture;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Globalization;
using System.Net;
using System.Reflection;
using System.Web;

using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;
using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;

namespace StreamerBotLib.BotClients
{
    public class BotsTwitch : BotsBase
    {
        internal static TwitchTokenBot TwitchTokenBot { get; private set; }
        public static TwitchBotChatClient TwitchBotChatClient { get; private set; }
        public static TwitchBotFollowerSvc TwitchFollower { get; private set; }
        public static TwitchBotLiveMonitorSvc TwitchLiveMonitor { get; private set; }
        public static TwitchBotClipSvc TwitchBotClipSvc { get; private set; }
        public static TwitchBotUserSvc TwitchBotUserSvc { get; private set; }
        public static TwitchBotPubSub TwitchBotPubSub { get; private set; }


        private Thread BulkLoadFollows;
        private Thread BulkLoadClips;

        private const int BulkFollowSkipCount = 1000;

        public static event EventHandler<EventArgs> RaidCompleted;
        public event EventHandler<InvalidAccessTokenEventArgs> InvalidTwitchAccess;

        public BotsTwitch()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Building all of the Twitch bots.");

             DataManager = SystemsController.DataManage;

            TwitchTokenBot = new();
            TwitchBotChatClient = new();
            TwitchFollower = new();
            TwitchLiveMonitor = new(DataManager);
            TwitchBotClipSvc = new();
            TwitchBotUserSvc = new();
            TwitchBotPubSub = new();

            AddBot(TwitchTokenBot);
            // not including "TwitchBotUserSvc" bot, it's an authentication on-demand bot to get the info
            AddBot(TwitchFollower);
            AddBot(TwitchLiveMonitor);
            AddBot(TwitchBotClipSvc);
            AddBot(TwitchBotChatClient);
            AddBot(TwitchBotPubSub);

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Adding event handlers from bot managers.");

            TwitchBotChatClient.OnBotStarted += TwitchBotChatClient_OnBotStarted;
            TwitchBotChatClient.OnBotStopping += TwitchBotChatClient_OnBotStopping;
            TwitchBotChatClient.OnBotStopped += TwitchBotChatClient_OnBotStopped;
            //TwitchBotChatClient.UnRegisterHandlers += TwitchBotChatClient_UnRegisterHandlers;

            TwitchFollower.OnBotStarted += TwitchFollower_OnBotStarted;
            TwitchLiveMonitor.OnBotStarted += TwitchLiveMonitor_OnBotStarted;
            TwitchBotClipSvc.OnBotStarted += TwitchBotClipSvc_OnBotStarted;
            TwitchBotUserSvc.GetChannelGameName += TwitchBotUserSvc_GetChannelGameName;
            TwitchBotUserSvc.StartRaidEventResponse += TwitchBotUserSvc_StartRaidEventResponse;
            TwitchBotUserSvc.CancelRaidEvent += TwitchBotUserSvc_CancelRaidEvent;
            TwitchBotUserSvc.GetStreamsViewerCount += TwitchBotUserSvc_OnGetStreamsViewerCount;
            TwitchBotPubSub.OnBotStarted += TwitchBotPubSub_OnBotStarted;
            TwitchBotPubSub.OnBotStopped += TwitchBotPubSub_OnBotStopped;

            TwitchTokenBot.BotAcctAuthCodeExpired += TwitchTokenBot_BotAcctAuthCodeExpired;
            TwitchTokenBot.StreamerAcctAuthCodeExpired += TwitchTokenBot_StreamerAcctAuthCodeExpired;


            CheckStreamerBotIds();
        }

        /// <summary>
        /// Retrieves a property within the Helix, and creates a new API object.
        /// The use case is: access tokens ready to go at the GUI level, no bots are auto-started 
        /// (would otherwise create a new Helix api object) upon app start, and there's a category 
        /// update in the GUI - creates a null exception.
        /// </summary>
        public static void InitializeHelix()
        {
            _ = TwitchBotUserSvc.HelixAPIBotToken; // performs a null check and creates a new api if necessary
        }

        public override void ManageStreamOnlineOfflineStatus(bool Start)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, $"Now managing starting or stopping bots " +
                "since active livestream={Start}");

            if (Start)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Starting bots now that the livestream " +
                    "is online.");

                if (OptionFlags.TwitchChatBotConnectOnline)
                {
                    TwitchBotChatClient.StartBot();
                }

                if (OptionFlags.TwitchPubSubOnlineMode)
                {
                    TwitchBotPubSub.StartBot();
                }

                if (OptionFlags.TwitchFollowerConnectOnline)
                {
                    TwitchFollower.StartBot();
                }

                if (OptionFlags.TwitchClipConnectOnline)
                {
                    TwitchBotClipSvc.StartBot();
                }
            }
            else
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Stopping bots now that the " +
                    "livestream is offline.");

                if (OptionFlags.TwitchChatBotDisconnectOffline && TwitchBotChatClient.IsStarted)
                {
                    TwitchBotChatClient.StopBot();
                }

                if (OptionFlags.TwitchPubSubOnlineMode && TwitchBotPubSub.IsStarted)
                {
                    TwitchBotPubSub.StopBot();
                }

                if (OptionFlags.TwitchClipDisconnectOffline && TwitchBotClipSvc.IsStarted)
                {
                    TwitchBotClipSvc.StopBot();
                }

                if (OptionFlags.TwitchFollowerDisconnectOffline && TwitchFollower.IsStarted)
                {
                    TwitchFollower.StopBot();
                }
            }

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Finished managing the bots based " +
                "on current livestream status (online or offline).");
        }

        private void TwitchBotUserSvc_GetChannelGameName(object sender, OnGetChannelGameNameEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Updating channel category game name.");

            InvokeBotEvent(this, BotEvents.TwitchCategoryUpdate, e);
        }

        private void RegisterHandlers()
        {
            if (TwitchBotChatClient.IsStarted && !TwitchBotChatClient.HandlersAdded)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Adding event handlers to chat client.");

                TwitchBotChatClient.TwitchChat.OnChatCommandReceived += Client_OnChatCommandReceived;
                TwitchBotChatClient.TwitchChat.OnCommunitySubscription += Client_OnCommunitySubscription;
                TwitchBotChatClient.TwitchChat.OnExistingUsersDetected += Client_OnExistingUsersDetected;
                TwitchBotChatClient.TwitchChat.OnGiftedSubscription += Client_OnGiftedSubscription;
                TwitchBotChatClient.TwitchChat.OnJoinedChannel += Client_OnJoinedChannel;
                TwitchBotChatClient.TwitchChat.OnMessageReceived += Client_OnMessageReceived;
                TwitchBotChatClient.TwitchChat.OnNewSubscriber += Client_OnNewSubscriber;
                TwitchBotChatClient.TwitchChat.OnRaidNotification += Client_OnRaidNotification;
                TwitchBotChatClient.TwitchChat.OnReSubscriber += Client_OnReSubscriber;
                TwitchBotChatClient.TwitchChat.OnUserBanned += Client_OnUserBanned;
                TwitchBotChatClient.TwitchChat.OnUserJoined += Client_OnUserJoined;
                TwitchBotChatClient.TwitchChat.OnUserLeft += Client_OnUserLeft;
                TwitchBotChatClient.TwitchChat.OnUserTimedout += Client_OnUserTimedout;
                TwitchBotChatClient.TwitchChat.OnMessageCleared += Client_OnMessageCleared;

                TwitchBotChatClient.HandlersAdded = true;
            }

            if (TwitchFollower.IsStarted && !TwitchFollower.HandlersAdded)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Adding event handlers to follower service.");

                TwitchFollower.FollowerService.OnNewFollowersDetected += FollowerService_OnNewFollowersDetected;
                TwitchFollower.FollowerService.OnBulkFollowsUpdate += FollowerService_OnBulkFollowsUpdate;

                TwitchFollower.HandlersAdded = true;
            }

            if (TwitchLiveMonitor.IsStarted && !TwitchLiveMonitor.HandlersAdded)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, 
                    $"Adding event handlers to Livestream object, with instance date: {TwitchLiveMonitor.LiveStreamMonitor.InstanceDate}.");

                TwitchLiveMonitor.LiveStreamMonitor.OnStreamOnline += LiveStreamMonitor_OnStreamOnline;
                TwitchLiveMonitor.LiveStreamMonitor.OnStreamUpdate += LiveStreamMonitor_OnStreamUpdate;
                TwitchLiveMonitor.LiveStreamMonitor.OnStreamOffline += LiveStreamMonitor_OnStreamOffline;

                TwitchLiveMonitor.HandlersAdded = true;
            }

            if (TwitchBotClipSvc.IsStarted && !TwitchBotClipSvc.HandlersAdded)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Adding event handlers to clip service.");

                TwitchBotClipSvc.ClipMonitorService.OnNewClipFound += ClipMonitorServiceOnNewClipFound;

                TwitchBotClipSvc.HandlersAdded = true;
            }

            if (TwitchBotPubSub.IsStarted && !TwitchBotPubSub.HandlersAdded)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Adding event handlers to PubSub service.");

                TwitchBotPubSub.TwitchPubSub.OnChannelPointsRewardRedeemed += TwitchPubSub_OnChannelPointsRewardRedeemed;
                TwitchBotPubSub.HandlersAdded = true;
            }

        }

        public static void CheckStreamerBotIds()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Checking and getting user ids for bot and/or streamer account(s).");

            string Botuserid = OptionFlags.TwitchBotUserId, Channeluserid = OptionFlags.TwitchStreamerUserId;

            if (OptionFlags.TwitchPriorBotName != OptionFlags.TwitchBotUserName)
            {
                OptionFlags.TwitchPriorBotName = OptionFlags.TwitchBotUserName;
                Botuserid = DataManager.GetUserId(new(TwitchBotsBase.TwitchBotUserName, Platform.Twitch));
            }
            if (OptionFlags.TwitchPriorChannelName != OptionFlags.TwitchChannelName)
            {
                OptionFlags.TwitchPriorChannelName = OptionFlags.TwitchChannelName;
                Channeluserid = DataManager.GetUserId(new(TwitchBotsBase.TwitchChannelName, Platform.Twitch));
            }

            TwitchBotUserSvc.SetIds(Channeluserid, Botuserid);
        }

        private void TwitchBotChatClient_UnRegisterHandlers(object sender, EventArgs e)
        {
            if (TwitchBotChatClient.HandlersAdded)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Removing event handlers from chat client.");

                TwitchBotChatClient.TwitchChat.OnChatCommandReceived -= Client_OnChatCommandReceived;
                TwitchBotChatClient.TwitchChat.OnCommunitySubscription -= Client_OnCommunitySubscription;
                TwitchBotChatClient.TwitchChat.OnExistingUsersDetected -= Client_OnExistingUsersDetected;
                TwitchBotChatClient.TwitchChat.OnGiftedSubscription -= Client_OnGiftedSubscription;
                TwitchBotChatClient.TwitchChat.OnJoinedChannel -= Client_OnJoinedChannel;
                TwitchBotChatClient.TwitchChat.OnMessageReceived -= Client_OnMessageReceived;
                TwitchBotChatClient.TwitchChat.OnNewSubscriber -= Client_OnNewSubscriber;
                TwitchBotChatClient.TwitchChat.OnRaidNotification -= Client_OnRaidNotification;
                TwitchBotChatClient.TwitchChat.OnReSubscriber -= Client_OnReSubscriber;
                TwitchBotChatClient.TwitchChat.OnUserBanned -= Client_OnUserBanned;
                TwitchBotChatClient.TwitchChat.OnUserJoined -= Client_OnUserJoined;
                TwitchBotChatClient.TwitchChat.OnUserLeft -= Client_OnUserLeft;
                TwitchBotChatClient.TwitchChat.OnUserTimedout -= Client_OnUserTimedout;
                TwitchBotChatClient.TwitchChat.OnMessageCleared -= Client_OnMessageCleared;

                TwitchBotChatClient.HandlersAdded = false;
            }
        }

        #region Twitch Bot Chat Client

        private void TwitchBotChatClient_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();

            InvokeBotEvent(this, BotEvents.TwitchChatBotStarted, null);

            if (OptionFlags.TwitchChatBotConnectOnline || OptionFlags.IsStreamOnline)
            {
                InvokeBotEvent(this, BotEvents.TwitchOnUserJoined, new StreamerOnUserJoinedArgs() { LiveUser = AddUserId(TwitchBotsBase.TwitchBotUserName) });
            }
        }

        private void TwitchBotChatClient_OnBotStopping(object sender, EventArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchChatBotStopping, new());
        }

        private void TwitchBotChatClient_OnBotStopped(object sender, EventArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchChatBotStopped, new());

            TwitchBotChatClient.HandlersAdded = false;
        }

        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchNewSubscriber, e);
        }

        private void Client_OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchReSubscriber, e);
        }

        private void Client_OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchGiftSubscription, e);
        }

        private void Client_OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchCommunitySubscription, e);
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            if (OptionFlags.MsgBotConnection)
            {
                Version version = Assembly.GetEntryAssembly().GetName().Version;
                string s = string.Format(CultureInfo.CurrentCulture,
                    LocalizedMsgSystem.GetTwineBotAuthorInfo(), version.Major, version.Minor, version.Build, version.Revision);

                Send(s);
            }
        }

        private void Client_OnExistingUsersDetected(object sender, OnExistingUsersDetectedArgs e)

        {
            InvokeBotEvent(this, BotEvents.TwitchExistingUsers, new StreamerOnExistingUserDetectedArgs()
            {
                Users = e.Users.ConvertAll(AddUserId)
            });
        }

        private static Models.LiveUser AddUserId(string s)
        {
            Models.LiveUser user = new(s, Platform.Twitch);

            string userId = DataManager.GetUserId(user);
            if (userId == null || userId == string.Empty)
            {
                user.UserId = TwitchBotUserSvc.GetUserId(user.UserName);
            }
            else
            {
                user.UserId = userId;
            }
            return user;
        }

        private void Client_OnUserBanned(object sender, OnUserBannedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchOnUserBanned, e);
        }

        private void Client_OnUserTimedout(object sender, OnUserTimedoutArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchOnUserTimedout, e);
        }

        private void Client_OnUserLeft(object sender, OnUserLeftArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchOnUserLeft, new StreamerOnUserLeftArgs() { LiveUser = AddUserId(e.Username) });
        }

        private void Client_OnUserJoined(object sender, OnUserJoinedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchOnUserJoined, new StreamerOnUserJoinedArgs() { LiveUser = AddUserId(e.Username) });
        }

        private void Client_OnRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            string CategoryName = GetUserCategory(e.RaidNotification.UserId);

            InvokeBotEvent(this, BotEvents.TwitchIncomingRaid, new OnIncomingRaidArgs() { Category = CategoryName, RaidTime = DateTime.Now.ToLocalTime(), ViewerCount = e.RaidNotification.MsgParamViewerCount, DisplayName = e.RaidNotification.DisplayName });
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchMessageReceived, e);
        }

        private void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchChatCommandReceived, e);
        }

        private void Client_OnMessageCleared(object sender, OnMessageClearedArgs e)
        {
        }

        #endregion

        #region Twitch User Bot

        public static string GetUserCategory(string UserId = null, string UserName = null)
        {
            return TwitchBotUserSvc.GetUserGameCategory(UserId: UserId, UserName: UserName);
        }

        public static DateTime GetUserAccountAge(string UserId = null, string UserName = null)
        {
            return TwitchBotUserSvc.GetUserCreatedAt(UserName: UserName, UserId: UserId);
        }

        public static bool VerifyUserExist(string UserName)
        {
            return TwitchBotUserSvc.GetUserId(UserName) != null;
        }

        public static void BanUserRequest(string UserName, BanReasons Reason, int Duration = 0)
        {
            TwitchBotUserSvc.BanUser(UserName, Reason, Duration);
        }

        public static bool ModifyChannelInformation(string Title = null, string CategoryName = null, string CategoryId = null)
        {
            bool result = false;

            if (Title != null)
            {
                result = TwitchBotUserSvc.SetChannelTitle(Title);
            }
            if (CategoryName != null || CategoryId != null)
            {
                result = TwitchBotUserSvc.SetChannelCategory(CategoryName, CategoryId);
            }

            return result;
        }

        public static string GetUserId(string UserName)
        {
            return TwitchBotUserSvc.GetUserId(UserName);
        }

        public static void RaidChannel(string ToUserName)
        {
            TwitchBotUserSvc.RaidChannel(ToUserName);
        }

        public static void CancelRaidChannel()
        {
            TwitchBotUserSvc.CancelRaidChannel();
        }

        private void TwitchBotUserSvc_StartRaidEventResponse(object sender, OnStreamRaidResponseEventArgs e)
        {
            StartRaid(e.ToChannel, e.CreatedAt.ToLocalTime());
        }

        private void TwitchBotUserSvc_CancelRaidEvent(object sender, EventArgs e)
        {
            CancelRaidLoop();
        }

        public static void GetViewerCount()
        {
            TwitchBotUserSvc.GetViewerCount(TwitchBotsBase.TwitchChannelName);
        }
        private void TwitchBotUserSvc_OnGetStreamsViewerCount(object sender, GetStreamsEventArgs e)
        {
            PostInternalCommand(LocalizedMsgSystem.GetVar(DefaultCommand.uptime), [e.ViewerCount.ToString()], $"!{LocalizedMsgSystem.GetVar(MsgVars.uptime)} {e.ViewerCount}");
        }

        internal void PostInternalCommand(string Com, List<string> ComArgs, string ComMessage)
        {
            InvokeBotEvent(this, BotEvents.TwitchBotCommandCall, new SendBotCommandEventArgs()
            {
                CmdMessage = new()
                {
                    CommandArguments = ComArgs,
                    CommandText = Com,
                    DisplayName = TwitchBotsBase.TwitchBotUserName,
                    Channel = TwitchBotsBase.TwitchChannelName,
                    IsBroadcaster = TwitchBotsBase.TwitchBotUserName == TwitchBotsBase.TwitchChannelName,
                    IsHighlighted = false,
                    IsMe = false,
                    IsModerator = TwitchBotsBase.TwitchBotUserName != TwitchBotsBase.TwitchChannelName,
                    IsPartner = false,
                    IsSkippingSubMode = false,
                    IsStaff = false,
                    IsSubscriber = false,
                    IsTurbo = false,
                    IsVip = false,
                    Message = ComMessage
                }
            });
        }

        #endregion

        #region Follower Bot

        private void TwitchFollower_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();

            GetAllFollowers();
        }

        private void FollowerService_OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
            List<ChannelFollower> newFollows = [];

            foreach (ChannelFollower F in e.NewFollowers)
            {
                if (!DataManager.CheckFollower(F.UserName))
                {
                    newFollows.Add(F);
                }
            }

            if (newFollows.Count > 0)
            {
                e.NewFollowers = newFollows;
                InvokeBotEvent(this, BotEvents.TwitchPostNewFollowers, e);
            }
        }

        #endregion

        #region Twitch LiveMonitor

        private void TwitchLiveMonitor_OnBotStarted(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, "Bot started, registering handles.");
            RegisterHandlers();
        }

        public static TwitchBotLiveMonitorSvc LiveMonitorSvc => TwitchLiveMonitor;

        private void LiveStreamMonitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, $"Found {e.Channel} is now online.");

            if (e.Channel != TwitchBotsBase.TwitchChannelName)
            {
                TwitchLiveMonitor.SendMultiLiveMsg(e);
            }
            else
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, "Notifying streamer channel is now online.");

                InvokeBotEvent(this, BotEvents.TwitchStreamOnline, e);

                if (!OptionFlags.TwitchChatBotConnectOnline && TwitchBotChatClient.IsStarted)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, $"Adding {TwitchBotsBase.TwitchBotUserName} to user list.");

                    InvokeBotEvent(this, BotEvents.TwitchOnUserJoined, new OnUserJoinedArgs() { Username = TwitchBotsBase.TwitchBotUserName, Channel = TwitchBotsBase.TwitchChannelName });
                }

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, "Getting a list of all current viewers in the stream to register in the system.");

                //ThreadManager.CreateThreadStart(() =>
                //{
                //    InvokeBotEvent(this, BotEvents.TwitchExistingUsers, new StreamerOnExistingUserDetectedArgs()
                //    {
                //        Users = (from C in TwitchBotUserSvc.GetChatters()
                //                 select AddUserId(C.UserName)).ToList()
                //    });
                //});

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, "Sent the current viewership list.");

            }
        }

        private void LiveStreamMonitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, $"Received, {e.Channel} has a stream update notification.");

            if (e.Channel == TwitchBotsBase.TwitchChannelName)
            {
                InvokeBotEvent(this, BotEvents.TwitchStreamUpdate, e);
            }
        }

        private void LiveStreamMonitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            if (e.Channel == TwitchBotsBase.TwitchChannelName) // live monitor checks different channels, we need this event to focus on the streamer channel
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, $"Received, {e.Channel} is now offline.");

                if (OptionFlags.IsStreamOnline)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, "Data system thinks stream is still online, now posting event stream is offline.");

                    InvokeBotEvent(this, BotEvents.TwitchStreamOffline, e);
                }
            }
        }

        private static void CheckStreamOnlineChatBot()
        {
            while (OptionFlags.IsStreamOnline && !TwitchBotChatClient.IsStarted)
            {
                if (OptionFlags.TwitchChatBotConnectOnline)
                {
                    TwitchBotChatClient.StartBot();
                }
                Thread.Sleep(5000);
            }
        }

        #endregion

        #region Twitch Bot Clip Service

        private void TwitchBotClipSvc_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();

            // start thread to retrieve all clips
            BulkLoadClips = ThreadManager.CreateThread(ProcessClips);
            MultiThreadOps.Add(BulkLoadClips);
            BulkLoadClips.Start();
        }

        public void ClipMonitorServiceOnNewClipFound(object sender, OnNewClipsDetectedArgs e)
        {
            while (StartClips) { } // wait while receiving new clips

            InvokeBotEvent(this, BotEvents.TwitchPostNewClip, e);
        }

        /// <summary>
        /// Get the clips for a specific user channel.
        /// </summary>
        /// <param name="ChannelName">Channel to get the clips.</param>
        /// <param name="ReturnData">The callback method when the clips are found.</param>
        public void GetChannelClips(Action<List<Models.Clip>> ReturnData)
        {
            ThreadManager.CreateThreadStart(() => ProcessChannelClips(ReturnData));
        }

        /// <summary>
        /// Creates a clip, per Twitch API (30 seconds of the prior 90 seconds of broadcasted video), of the current streamer channel.
        /// </summary>
        public static void CreateClip()
        {
            TwitchBotClipSvc.CreateClip();
        }

        #endregion

        #region PubSub
        private void TwitchBotPubSub_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();
        }

        private void TwitchPubSub_OnChannelPointsRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs e)
        {
            Twitch.TwitchLib.Events.PubSub.OnChannelPointsRewardRedeemedArgs local = new() { ChannelId = e.ChannelId, RewardRedeemed = e.RewardRedeemed };
            InvokeBotEvent(this, BotEvents.TwitchChannelPointsRewardRedeemed, local);
        }

        private void TwitchBotPubSub_OnBotStopped(object sender, EventArgs e)
        {
            if (TwitchBotPubSub.HandlersAdded)
            {
                TwitchBotPubSub.TwitchPubSub.OnChannelPointsRewardRedeemed -= TwitchPubSub_OnChannelPointsRewardRedeemed;
            }
        }

        #endregion

        #region Twitch Token Bot

        public static void TwitchActivateAuthCode(string clientId, Action<string> OpenBrowser, Action AuthenticationFinished)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Received request to start the Twitch auth code approval process.");

            ThreadManager.CreateThreadStart(() =>
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Asking Twitch Token Bot to create the " +
                    "authorization URL for the user to approve this application access to their account.");

                TwitchTokenBot.GenerateAuthCodeURL(clientId, OpenBrowser, AuthenticationFinished);
            });
        }

        /// <summary>
        /// Since user can try to authorize both tokens, we're saving the states to determine which one the user authorized.
        /// </summary>
        private string AuthCodeStreamerState = "";
        private Action<string> AuthCodeStreamerAction;
        private string AuthCodeBotState = "";
        private Action<string> AuthCodeBotAction;
        private Action FinishedAuthenticationAction;

        private void TwitchTokenBot_StreamerAcctAuthCodeExpired(object sender, TwitchAuthCodeExpiredEventArgs e)
        {
            if (e?.OpenBrowser != null)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Starting Twitch auth code approval for the streamer " +
                    "channel.");

                lock (AuthCodeStreamerState)
                {
                    AuthCodeStreamerState = e.State;
                }
                AuthCodeStreamerAction = e.OpenBrowser;
                FinishedAuthenticationAction = e.AuthenticationFinished;

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, $"Start the http listener " +
                    $"on {OptionFlags.TwitchAuthRedirectURL} to get ready for when the user authorizes application access to their account.");

                AuthCodeListener();

                // call the provided method to give user the web based app authorization URL
                e.OpenBrowser(e.AuthURL);
            }
            else
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Determined the auth code expired and user isn't " +
                    "ready to authorize the application. Need to stop all of the bots, since the access tokens are no longer valid; auth code is now invalid.");

                foreach (IIOModule bot in BotsList)
                {
                    bot.StopBot();
                }
                InvalidTwitchAccess?.Invoke(this, new(Platform.Twitch, e.BotType));
            }
        }

        private bool HttpStarted = false;

        /// <summary>
        /// HTTP Listener on the auth code redirect URL, listening for responses to the user authenticating bot & streamer access.
        /// </summary>
        private void AuthCodeListener()
        {
            if (!HttpStarted)
            {
                // start an http listener to receive auth code
                ThreadManager.CreateThreadStart(() =>
                {
                    HttpStarted = true;
                    HttpListener httpListener = new();
                    httpListener.Prefixes.Add(OptionFlags.TwitchAuthRedirectURL + (OptionFlags.TwitchAuthRedirectURL.EndsWith('/') ? "" : "/")); // requires ending '/' to URL
                    httpListener.Start();

                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Started http listener for when user " +
                        "accepts and authorizes this app to access their account.");


                    // blocking call to wait for incoming request from Twitch
                    HttpListenerRequest request = httpListener.GetContext().Request;

                    Uri uridata = request.Url;

                    /*
                    from: https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#authorization-code-grant-flow
                    expected return, affirm or deny, when attempting to authorize the application

                    If the user authorized your app by clicking Authorize, the server sends the authorization code 
                    to your redirect URI (see the code query parameter):

                    http://localhost:3000/
                    ?code=gulfwdmys5lsm6qyz4xiz9q32l10
                    &scope=channel%3Amanage%3Apolls+channel%3Aread%3Apolls
                    &state=c3ab8aa609ea11e793ae92361f002671

                    If the user didn’t authorize your app, the server sends the error code and description 
                    to your redirect URI (see the error and error_description query parameters):

                    http://localhost:3000/
                    ?error=access_denied
                    &error_description=The+user+denied+you+access
                    &state=c3ab8aa609ea11e793ae92361f002671

                     */

                    var QueryValues = HttpUtility.ParseQueryString(uridata.Query);
                    httpListener.Close(); // finished receiving requests

                    if (QueryValues["state"] == AuthCodeStreamerState)
                    {
                        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Handling the streamer account authorization response.");

                        if (!QueryValues.AllKeys.Contains("error"))
                        {
                            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Determined user approved the application access.");

                            OptionFlags.TwitchAuthStreamerAuthCode = QueryValues["code"];
                        }
                        else
                        {
                            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Determined user didn't approve the application access.");

                            AuthCodeStreamerAction(Msgs.MsgTwitchAuthFailedAuthentication);
                            OptionFlags.TwitchAuthStreamerAuthCode = null;
                        }
                        lock (AuthCodeStreamerState)
                        {
                            AuthCodeStreamerState = "";
                        }
                    }
                    else if (QueryValues["state"] == AuthCodeBotState)
                    {
                        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Handling the bot account authorization response.");

                        if (!QueryValues.AllKeys.Contains("error"))
                        {
                            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Determined user approved the application access.");

                            OptionFlags.TwitchAuthBotAuthCode = QueryValues["code"];
                        }
                        else
                        {
                            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Determined user didn't approve the application process.");

                            AuthCodeBotAction(Msgs.MsgTwitchAuthFailedAuthentication);
                            OptionFlags.TwitchAuthBotAuthCode = null;
                        }

                        lock (AuthCodeBotState)
                        {
                            AuthCodeBotState = "";
                        }
                    }

                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Checking tokens.");
                    TwitchTokenBot.CheckToken();   // proceed with getting a refresh token and access token
                    
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Captured the auth code. Now attempting the " +
                            "finishing action.");
                    FinishedAuthenticationAction.Invoke();

                    HttpStarted = false;
                });
            }
        }

        private void TwitchTokenBot_BotAcctAuthCodeExpired(object sender, TwitchAuthCodeExpiredEventArgs e)
        {
            if (e.OpenBrowser != null)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Starting Twitch auth code approval for the bot " +
    "account.");

                lock (AuthCodeBotState)
                {
                    AuthCodeBotState = e.State;
                }
                AuthCodeBotAction = e.OpenBrowser;
                FinishedAuthenticationAction = e.AuthenticationFinished;

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, $"Start the http listener " +
    $"on {OptionFlags.TwitchAuthRedirectURL} to get ready for when the user authorizes application access to their account.");

                AuthCodeListener();

                // call the provided method to give user the web based app authorization URL
                e.OpenBrowser(e.AuthURL);
            }
            else
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Determined the auth code expired and user isn't " +
    "ready to authorize the application. Need to stop all of the bots, since the access tokens are no longer valid; auth code is now invalid.");

                foreach (IIOModule bot in BotsList)
                {
                    bot.StopBot();
                }
                InvalidTwitchAccess?.Invoke(this, new(Platform.Twitch, e.BotType));
            }
        }

        public static void ForceTwitchReauthorization()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, $"Received a request to reauthenticate.");

            TwitchTokenBot.ForceReauthorization();
        }

        #endregion

        #region Threaded Ops

        public override void GetAllFollowers()
        {
            if (TwitchFollower.IsStarted)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Request to 'Get All Followers', without " +
                    "overriding to update followers.");

                GetAllFollowers(false);
            }
        }

        public override void GetAllFollowers(bool OverrideUpdateFollowers = false)
        {
            if (OptionFlags.ManageFollowers && (OptionFlags.TwitchAddFollowersStart || OverrideUpdateFollowers) && TwitchFollower.IsStarted)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Processing request to update all followers to " +
                    "the streamer's channel.");

                BulkLoadFollows = ThreadManager.CreateThread(() =>
                {
                    string ChannelName = TwitchBotsBase.TwitchChannelName;

                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Prepare to bulk-add Twitch followers.");

                    InvokeBotEvent(this, BotEvents.TwitchStartBulkFollowers, null);

                    try
                    {
                        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Started the bulk-add " +
                            "process for the followers of the Twitch streamer channel.");

                        _ = TwitchFollower.GetAllFollowersBulkAsync().Result;
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                    }

                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Wrap up bulk-add Twitch followers.");

                    InvokeBotEvent(this, BotEvents.TwitchStopBulkFollowers, null);
                });
                MultiThreadOps.Add(BulkLoadFollows);
                BulkLoadFollows.Start();
            }
        }

        private void FollowerService_OnBulkFollowsUpdate(object sender, OnNewFollowersDetectedArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, $"Received event to begin bulk followers update.");

            InvokeBotEvent(
                this,
                BotEvents.TwitchBulkPostFollowers,
                new OnNewFollowersDetectedArgs()
                {
                    NewFollowers = e.NewFollowers
                });
        }

        private List<Clip> ClipList { get; set; }

        private bool StartClips { get; set; }

        private void ProcessClips()
        {
            StartClips = true;
            ClipList = TwitchBotClipSvc.GetAllClipsAsync().Result;

            InvokeBotEvent(this, BotEvents.TwitchClipSvcOnClipFound, new ClipFoundEventArgs() { ClipList = ClipList });
            StartClips = false;
        }

        private async void ProcessChannelClips(Action<List<Models.Clip>> ActionCallback)
        {
            List<Clip> result = [];
            TwitchBotClipSvc ChannelClips = new();
            if (ChannelClips.StartBot())
            {
                result = await ChannelClips.GetAllClipsAsync();
                ChannelClips.StopBot();
            }

            ActionCallback?.Invoke(BotIOController.BotController.ConvertClips(result));
        }

        private readonly TimeSpan DefaultOutRaid = new(0, 0, 90);
        private DateTime OutRaidStarted;
        private Thread RaidLoop = null;
        private readonly string RaidLock = "lock";

        /// <summary>
        /// Establishes a Twitch raid procedure to hold up to 90 seconds, per Twitch API doc, where the user can still cancel the 
        /// raid and the bots can't respond to stream going offline until after the raid completes.
        /// 
        /// When the user quick raids (clicks a raid now button before the timer completes), this check continues waiting until timer completes.
        /// </summary>
        /// <param name="ToChannelName"></param>
        /// <param name="RaidCreated"></param>
        private void StartRaid(string ToChannelName, DateTime RaidCreated)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Starting internal raid loop procedure to " +
                "send 'stream offline' events across the application to finish an active stream.");
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Twitch no longer notifies through the Twitch " +
                "Chat API that a raid occurred for a channel.");
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "But, Twitch added to the API to permit a bot " +
                "to start a raid, meaning, anyone with command access to the channel can initiate a raid, alongside the streamer.");
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "So, this procedure tracks the following 90 " +
                "seconds, in case the user cancels the raid.");
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Otherwise, after 90 seconds, regardless if " +
                "the user completes the raid early (clicked 'raid now' button) and notify rest of the bot of the raid.");


            lock (RaidLock)
            {
                OutRaidStarted = RaidCreated;
            }

            if (RaidLoop == null && OptionFlags.IsStreamOnline) // create only 1 thread & when stream is online
            {
                RaidLoop = ThreadManager.CreateThread(() =>
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Start to wait on the raid.");

                    // declare locals, so we can use a thread-safe lock
                    DateTime LocalRaidStart;
                    bool LocalRaidStarted;

                    lock (RaidLock)
                    {
                        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Acknowledged the raid start time.");

                        LocalRaidStart = OutRaidStarted;
                        LocalRaidStarted = OptionFlags.TwitchOutRaidStarted;
                    }

                    while (DateTime.Now - LocalRaidStart <= DefaultOutRaid && LocalRaidStarted)
                    { // check for 90 seconds
                        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, $"Checking if {LocalRaidStart}+{DefaultOutRaid.Seconds} seconds is after " +
                            $"the current time. And wait some time to check again if user cancels the raid (via !cancelraid command).");

                        lock (RaidLock)
                        { // update values, in case they changed - use thread lock safety as another thread may change these
                            LocalRaidStart = OutRaidStarted;
                            LocalRaidStarted = OptionFlags.TwitchOutRaidStarted;
                        }
                        Thread.Sleep(5000);
                    }

                    // if the raid wasn't canceled after the loop finished, send raid event to main bot
                    if (LocalRaidStarted)
                    {
                        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Raid succeeded. Proceeding to inform the " +
                            "rest of the application to shutdown any bots for going offline and recording the outgoing raid details.");

                        InvokeBotEvent(this, BotEvents.TwitchOutgoingRaid,
                            new OnStreamRaidResponseEventArgs()
                            {
                                ToChannel = ToChannelName,
                                CreatedAt = OutRaidStarted
                            });

                        RaidCompleted?.Invoke(this, new());
                    }
                    RaidLoop = null;
                });
                RaidLoop.Start();
            }

        }

        /// <summary>
        /// Settings an option, thread safe, to cancel the pending Twitch raid.
        /// </summary>
        private void CancelRaidLoop()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Received a 'cancel raid' request.");

            lock (RaidLock)
            {
                OptionFlags.TwitchOutRaidStarted = false;
            }
        }

        #endregion

    }
}
