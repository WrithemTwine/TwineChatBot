#define USEQUEUELOGGER_NO // setup directive for "USEQUEUELOGGER": use logging, "USEQUEUELOGGER_NO": don't use logging

using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.BotClients.Twitch.EventSubSubscriptionManagers;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub;
using StreamerBotLib.Culture;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Logger;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Net;
using System.Web;
using System.Windows.Threading;

using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.EventSub.Core.Models.Chat;

namespace StreamerBotLib.BotClients
{
    public class BotsTwitch : BotsBase
    {
        #region Properties-Events
        internal static TwitchTokenBot TwitchTokenBot { get; private set; }

        internal static IEventSubMessageIdsLogger EventSubMessageIdsLogger { get; private set; } = new EventSubMessageIdsLogger();

        #region Streamer Account access tokens
        public static TwitchBotClipSvc TwitchBotClipSvc { get; private set; }
        /// <summary>
        /// Monitors the multi-live channels, to promote other channels
        /// </summary>
        public static TwitchBotLiveMonitorSvc TwitchBotLiveMonitorSvc { get; private set; }
        public static TwitchHelixBot TwitchHelixBot { get; private set; }

        public static TwitchEventSub TwitchEventSubBot { get; private set; }
        public static TwitchEventSub TwitchEventSubStreamer { get; private set; }

        internal static TwitchStreamerEventSubBotScopes TwitchStreamerEventSubBotScopes { get; set; } = new();
        internal static TwitchStreamerEventSubBotNoScopes TwitchStreamerEventSubBotNoScopes { get; set; } = new();

        #endregion

        #region Bot Account access tokens

        internal static TwitchBotEventSubChatClient TwitchBotEventSubChatClient { get; set; } = new();
        public static TwitchBotSendChatClient TwitchBotSendChatClient { get; set; }

        #endregion

        private Thread BulkLoadClips;

        public static event EventHandler<EventArgs> RaidCompleted;
        public static event EventHandler<StreamUpdatePropertiesEventArgs> StreamOnline;
        public static event EventHandler<StreamUpdatePropertiesEventArgs> StreamUpdated;
        public static event EventHandler<StreamUpdatePropertiesEventArgs> StreamOffline;

        public event EventHandler<InvalidAccessTokenEventArgs> InvalidTwitchAccess;

        public static event EventHandler<TwitchAuthCodeExpiredEventArgs> BotAuthCodeExpired;
        public static event EventHandler<TwitchAuthCodeExpiredEventArgs> StreamerAuthCodeExpired;

        /// <summary>
        /// This event relates to app starting up and notifies when the Helix apis 
        /// are constructed with existing tokens-or authcode access tokens are refreshed upon startup.
        /// </summary>
        public event EventHandler OnTwitchTokensInitialized;

        private Stream _CurrStream;
        public Stream CurrStream
        {
            get
            {
                _CurrStream ??= GetStreamDetail(UserId: OptionFlags.TwitchStreamerUserId).Streams[0];
                return _CurrStream;
            }
        }


#if USEQUEUELOGGER
        private ConcurrentQueue<string> StatusMessages;
        private bool LoggingStarted;
#endif

        /// <summary>
        /// When the user sets up the GUI to use either User tokens or AuthCode auto tokens,
        /// we need to setup the objects using the tokens.
        /// The token bot now manages the tokens, refreshing the auth mode tokens.
        /// </summary>
        public static async Task InitializeHelix()
        {
            await TwitchTokenBot.StartBot(); // performs a null check and creates a new api if necessary
        }

        /// <summary>
        /// Have the Token Bot stop processing token calls when the GUI detects expired tokens - UserToken mode
        /// User needs to update the tokens.
        /// </summary>
        public static async Task NotifyInvalidTwitchTokens()
        {
            await TwitchTokenBot.StopBot();
        }

        #endregion

        public BotsTwitch()
        {
            LogWriter.DebugLog(".ctor_BotsTwitch", DebugLogTypes.TwitchBots, "Building all of the Twitch bots.");

            StreamLoggerProvider.OnWriteLine += StreamLoggerProvider_OnWriteLine;
#if USEQUEUELOGGER
            StatusMessages = new();
#endif

            DataManager = SystemsController.DataManage;
            TwitchTokenBot = new();
            TwitchBotClipSvc = new(TwitchTokenBot);
            TwitchBotLiveMonitorSvc = new(TwitchTokenBot);
            TwitchHelixBot = new(TwitchTokenBot);

            TwitchEventSubBot = new(TwitchTokenBot, Bots.TwitchEventSubBot);
            TwitchEventSubStreamer = new(TwitchTokenBot, Bots.TwitchEventSubStreamer);

            TwitchBotSendChatClient = new(TwitchTokenBot);
            ActiveUserThread = false;

            AddBot(TwitchBotClipSvc);
            AddBot(TwitchBotLiveMonitorSvc);
            AddBot(TwitchHelixBot);
            AddBot(TwitchEventSubBot);
            AddBot(TwitchEventSubStreamer);
            AddBot(TwitchBotSendChatClient);

            LogWriter.DebugLog(".ctor_BotsTwitch", DebugLogTypes.TwitchBots, "Adding event handlers from bot managers.");

            AddHandlers();

#if USEQUEUELOGGER
            StartLogging();
#endif
        }

        private void StreamLoggerProvider_OnWriteLine(object sender, OnWriteLineEventArgs e)
        {
#if USEQUEUELOGGER
            StatusMessages.Enqueue(e.Message);
#else
            ThreadManager.CreateThreadStart("StreamLoggerProvider_OnWriteLine", () =>
            {
                LogWriter.WriteLog(e.Message);
            });
#endif
        }

#if USEQUEUELOGGER
        private void StartLogging()
        {
            if (!LoggingStarted)
            {
                LoggingStarted = true;
                ThreadManager.CreateThreadStart("StartLogging", () =>
                {
                    while (OptionFlags.ActiveToken)
                    {
                        while (StatusMessages.TryDequeue(out string Msg))
                        {
                            LogWriter.WriteLog(Msg);
                        }
                        Thread.Sleep(200);
                    }
                });
            }
        }
#endif
        private void AddHandlers()
        {
            TwitchBotClipSvc.OnBotStarted += TwitchBotClipSvc_OnBotStarted;
            TwitchBotClipSvc.OnBotStopped += TwitchBotClipSvc_OnBotStopped;

            TwitchHelixBot.GetChannelGameName += TwitchHelixBot_GetChannelGameName;
            //TwitchHelixBot.StartRaidEventResponse += TwitchBotUserSvc_StartRaidEventResponse;
            //TwitchHelixBot.CancelRaidEvent += TwitchBotUserSvc_CancelRaidEvent;
            TwitchHelixBot.GetStreamsViewerCount += TwitchBotUserSvc_OnGetStreamsViewerCount;
            TwitchHelixBot.OnBulkFollowsUpdate += TwitchHelixBot_OnBulkFollowsUpdate;
            TwitchHelixBot.BulkFollowsCompleted += TwitchHelixBot_BulkFollowsCompleted;

            TwitchBotLiveMonitorSvc.OnBotStarted += TwitchLiveMonitor_OnBotStarted;
            TwitchBotLiveMonitorSvc.OnBotStopped += TwitchBotLiveMonitorSvc_OnBotStopped;

            TwitchEventSubBot.OnBotStarted += TwitchEventSubBot_OnBotStarted;
            TwitchEventSubBot.OnBotStopped += TwitchEventSubBot_OnBotStopped;
            TwitchEventSubBot.OnInitialBotStartupSubHandlers += TwitchEventSubBot_OnInitialBotStartupSubHandlers;

            TwitchEventSubStreamer.OnBotStarted += TwitchEventSubStreamer_OnBotStarted;
            TwitchEventSubStreamer.OnBotStopped += TwitchEventSubStreamer_OnBotStopped;
            TwitchEventSubStreamer.OnInitialBotStartupSubHandlers += TwitchEventSubStreamer_OnInitialBotStartupSubHandlers;

            TwitchBotEventSubChatClient.OnChannelChatMessageReceived += TwitchBotEventSubChatClient_OnChannelChatMessageReceived;

            TwitchStreamerEventSubBotScopes.NewChannelCheer += TwitchStreamerEventSubBot_NewChannelCheer;
            TwitchStreamerEventSubBotScopes.NewChannelFollow += TwitchStreamerEventSubBot_NewChannelFollow;
            TwitchStreamerEventSubBotScopes.NewChannelCustomRewardRedemption += TwitchStreamerEventSubBot_NewChannelCustomRewardRedemption;
            TwitchStreamerEventSubBotScopes.NewChannelSubscribe += TwitchStreamerEventSubBot_NewChannelSubscribe;
            TwitchStreamerEventSubBotScopes.NewChannelSubscriptionGift += TwitchStreamerEventSubBot_NewChannelSubscriptionGift;
            TwitchStreamerEventSubBotScopes.NewChannelSubscriptionMessage += TwitchStreamerEventSubBot_NewChannelSubscriptionMessage;
            TwitchStreamerEventSubBotScopes.OnChannelChatMessageReceived += TwitchBotEventSubChatClient_OnChannelChatMessageReceived;
            TwitchStreamerEventSubBotScopes.OnChannelChatMessageStarted += TwitchStreamerEventSubBotScopes_OnChannelChatMessageStarted;
            TwitchStreamerEventSubBotScopes.OnChannelChatMessageStopping += TwitchStreamerEventSubBotScopes_OnChannelChatMessageStopping;
            TwitchStreamerEventSubBotScopes.OnChannelChatMessageStopped += TwitchStreamerEventSubBotScopes_OnChannelChatMessageStopped;

            TwitchStreamerEventSubBotNoScopes.NewStreamOffline += TwitchStreamerEventSubBot_NewStreamOffline;
            TwitchStreamerEventSubBotNoScopes.NewChannelUpdate += TwitchStreamerEventSubBot_NewChannelUpdate;
            TwitchStreamerEventSubBotNoScopes.NewChannelRaid += TwitchStreamerEventSubBot_NewChannelRaid;
            TwitchStreamerEventSubBotNoScopes.OutChannelRaid += TwitchStreamerEventSubBotNoScopes_OutChannelRaid;
            TwitchStreamerEventSubBotNoScopes.NewStreamOnline += TwitchStreamerEventSubBot_NewStreamOnline;

            TwitchTokenBot.AccessTokensInitialized += TwitchTokenBot_AccessTokensInitialized;
            TwitchTokenBot.BotAcctAuthCodeExpired += TwitchTokenBot_BotAcctAuthCodeExpired;
            TwitchTokenBot.StreamerAcctAuthCodeExpired += TwitchTokenBot_StreamerAcctAuthCodeExpired;
            TwitchTokenBot.StreamerNoScopesAuthCodeExpired += TwitchTokenBot_StreamerNoScopesAuthCodeExpired;
        }

        private void TwitchTokenBot_AccessTokensInitialized(object sender, EventArgs e)
        {
            OnTwitchTokensInitialized?.Invoke(this, new());
        }

        private void RegisterHandlers()
        {
            if (TwitchBotClipSvc.IsActive == true && !TwitchBotClipSvc.HandlersAdded)
            {
                LogWriter.DebugLog("RegisterHandlers", DebugLogTypes.TwitchBots, "Adding event handlers to clip service.");

                TwitchBotClipSvc.ClipMonitorService.OnNewClipFound += ClipMonitorServiceOnNewClipFound;

                TwitchBotClipSvc.HandlersAdded = true;
            }

            if (TwitchBotLiveMonitorSvc.IsActive == true && !TwitchBotLiveMonitorSvc.HandlersAdded)
            {
                LogWriter.DebugLog("RegisterHandlers", DebugLogTypes.TwitchBots,
                    $"Adding event handlers to Livestream object.");

                TwitchBotLiveMonitorSvc.LiveStreamMonitor.OnStreamOnline += LiveStreamMonitor_OnStreamOnline;

                TwitchBotLiveMonitorSvc.HandlersAdded = true;
            }
        }

        #region Started-Stopped EventSub Bots
        private void TwitchEventSubBot_OnInitialBotStartupSubHandlers(object sender, EventArgs e)
        {
            List<ITwitchBotEventSubSubscriptions> managers = [TwitchBotEventSubChatClient];

            if (!OptionFlags.TwitchStreamerUseToken)
            {
                managers.AddRange([TwitchStreamerEventSubBotNoScopes, TwitchStreamerEventSubBotScopes]);
            }

            foreach (var m in managers)
            {
                TwitchEventSubBot.AddSubscriptionHandler(m);
            }
        }
        private void TwitchEventSubStreamer_OnInitialBotStartupSubHandlers(object sender, EventArgs e)
        {
            List<ITwitchBotEventSubSubscriptions> managers = [TwitchStreamerEventSubBotNoScopes, TwitchStreamerEventSubBotScopes];

            foreach (var m in managers)
            {
                TwitchEventSubStreamer.AddSubscriptionHandler(m);
            }
        }
        private void TwitchEventSubStreamer_OnBotStarted(object sender, EventArgs e)
        {
            _CurrStream = null;

            Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
            {
                // the bot might start when stream is already started-not registered online, check for online and start additional subscriptions
                if (!OptionFlags.IsStreamOnline)
                {
                    ThreadManager.CreateThreadStart("TwitchEventSubStreamer_OnBotStarted", () =>
                    {
                        var response = GetStreamDetail(UserId: OptionFlags.TwitchStreamerUserId);

                        if (response != null && response.Streams.Length > 0)
                        {
                            LogWriter.DebugLog($"TwitchEventSubStreamer_OnBotStarted-StartMoreServices", DebugLogTypes.TwitchBots, "Found existing online stream for streamer channel.");
                            OptionFlags.IsStreamOnline = true;

                            InvokeBotEvent(this, BotEvents.TwitchResumeStreamOnline, new ResumeStreamOnlineEventArgs(response.Streams[0]));
                            ActiveUsers();
                            TwitchEventSubStreamer.AddStreamOnlineSubscriptions();
                            ManageStreamOnlineOfflineStatus(true);
                            StreamOnline?.Invoke(this, new() { CategoryName = CurrStream.GameName });
                        }
                    });
                }

            }));

            if (OptionFlags.TwitchAddFollowersStart)
            {
                ThreadManager.CreateThreadStart($"TwitchEventSubStreamer_OnBotStarted-GetFollowers", () =>
                {
                    GetAllFollowers();
                });
            }


        }

        private void TwitchEventSubStreamer_OnBotStopped(object sender, EventArgs e)
        {
            LogWriter.DebugLog("TwitchEventSubStreamer_OnBotStopped", DebugLogTypes.TwitchBots, "EventSub bot is now stopped.");

            CheckActiveBots();
        }

        private void TwitchEventSubBot_OnBotStarted(object sender, EventArgs e)
        {
            LogWriter.DebugLog("TwitchEventSubBot_OnBotStarted", DebugLogTypes.TwitchBots, "EventSub bot is now started.");
            InvokeBotEvent(this, BotEvents.TwitchBotEventSubStarted, null);
        }

        private void TwitchEventSubBot_OnBotStopped(object sender, EventArgs e)
        {
            LogWriter.DebugLog("TwitchEventSubBot_OnBotStopped", DebugLogTypes.TwitchBots, "EventSub bot is now stopped.");
            InvokeBotEvent(this, BotEvents.TwitchBotEventSubStopped, null);

            CheckActiveBots();
        }

        //private void TwitchBotEventSubChatClient_OnBotStarted(object sender, EventArgs e)
        //{
        //    LogWriter.DebugLog("TwitchBotEventSubChatClient_OnBotStarted", DebugLogTypes.TwitchBots, "Bot EventSub bot started to read channel chat messages.");

        //}
        //private void TwitchBotEventSubChatClient_OnBotStopped(object sender, EventArgs e)
        //{
        //    LogWriter.DebugLog("TwitchBotEventSubChatClient_OnBotStopped", DebugLogTypes.TwitchBots, "Bot EventSub bot stopped and won't read channel chat messages.");

        //}
        //private void TwitchStreamerEventSubBotNoScopes_OnBotStarted(object sender, EventArgs e)
        //{
        //    _CurrStream = null;

        //    Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
        //    {
        //        // the bot might start when stream is already started-not registered online, check for online and start additional subscriptions
        //        if (!OptionFlags.IsStreamOnline)
        //        {
        //            ThreadManager.CreateThreadStart("TwitchStreamerEventSubBotNoScopes_OnBotStarted", () =>
        //            {
        //                var response = GetStreamDetail(UserId: OptionFlags.TwitchStreamerUserId);

        //                if (response != null && response.Streams.Length > 0)
        //                {
        //                    LogWriter.DebugLog($"TwitchStreamerEventSubBotNoScopes_OnBotStarted-StartMoreServices", DebugLogTypes.TwitchBots, "Found existing online stream for streamer channel.");
        //                    OptionFlags.IsStreamOnline = true;

        //                    InvokeBotEvent(this, BotEvents.TwitchResumeStreamOnline, new ResumeStreamOnlineEventArgs(response.Streams[0]));
        //                    ActiveUsers();
        //                    ManageStreamOnlineOfflineStatus(true);
        //                    StreamOnline?.Invoke(this, new() { CategoryName = CurrStream.GameName });
        //                }
        //            });
        //        }
        //    }));
        //}
        //private void TwitchStreamerEventSubBot_OnBotStarted(object sender, EventArgs e)
        //{
        //    LogWriter.DebugLog("TwitchStreamerEventSubBot_OnBotStarted", DebugLogTypes.TwitchBots, "EventSub bot started.");

        //    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (OptionFlags.TwitchAddFollowersStart)
        //        {
        //            ThreadManager.CreateThreadStart($"TwitchStreamerEventSubBot_OnBotStarted-GetFollowers", () =>
        //            {
        //                GetAllFollowers();
        //            });
        //        }
        //    }));
        //}
        //private void TwitchStreamerEventSubBot_OnBotStopped(object sender, EventArgs e)
        //{
        //    LogWriter.DebugLog("TwitchStreamerEventSubBot_OnBotStopped", DebugLogTypes.TwitchBots, "Chat bot is now stopped.");

        //    CheckActiveBots();
        //}

        #endregion

        #region Bot EventSub Bot - listen to chat messages

        private void TwitchBotEventSubChatClient_OnChannelChatMessageReceived(object sender, ChannelChatMessageEventArgs e)
        {
            LogWriter.DebugLog("TwitchBotEventSubChatClient_OnChannelChatMessageReceived", DebugLogTypes.TwitchBots, "Bot EventSub bot received a new chat message. Determining if command.");

            if (e.ChannelChatMessage.ChatterUserId != OptionFlags.TwitchBotUserId)
            {
                bool command = false;

                foreach (var _ in from ChatMessageFragment fragment in e.ChannelChatMessage.Message.Fragments
                                  where fragment.Text.StartsWith('!')
                                  select new { })
                {
                    command = true;
                    break;
                }

                if (command)
                {
                    InvokeBotEvent(this, BotEvents.TwitchChatCommandReceived, e);
                }
                else
                {
                    InvokeBotEvent(this, BotEvents.TwitchMessageReceived, e);
                }
            }
        }

        #endregion

        #region Streamer No Scopes EventSub Bot

        private void TwitchStreamerEventSubBot_NewStreamOnline(object sender, NewStreamOnlineEventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewStreamOnline", DebugLogTypes.TwitchBots, "Notifying streamer channel is now online.");
            ManageStreamOnlineOfflineStatus(true);

            TwitchEventSubStreamer.AddStreamOnlineSubscriptions();

            StreamOnline?.Invoke(this, new() { CategoryName = CurrStream.GameName });

            InvokeBotEvent(this, BotEvents.TwitchStreamOnline, e);

            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewStreamOnline", DebugLogTypes.TwitchBots, "Getting a list of all current viewers in the stream to register in the system.");

            ActiveUsers();

            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewStreamOnline", DebugLogTypes.TwitchBots, "Sent the current viewership list.");
        }
        private void TwitchStreamerEventSubBot_NewChannelUpdate(object sender, NewChannelUpdateEventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewChannelUpdate", DebugLogTypes.TwitchBots, $"Registered a stream update, {e.ChannelUpdate.BroadcasterUserName}.");
            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewChannelUpdate", DebugLogTypes.TwitchBots, $"Received, {e.ChannelUpdate.BroadcasterUserName} has a stream update notification.");

            StreamUpdated?.Invoke(this, new(categoryName: e.ChannelUpdate.CategoryName));

            InvokeBotEvent(this, BotEvents.TwitchStreamUpdate, e);
        }
        private void TwitchStreamerEventSubBot_NewStreamOffline(object sender, NewStreamOfflineEventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewStreamOffline", DebugLogTypes.TwitchBots, $"Registered a stream is offline, {e.StreamOffline.BroadcasterUserName}.");

            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewStreamOffline", DebugLogTypes.TwitchBots, "Now posting to data system the stream is offline.");
            ManageStreamOnlineOfflineStatus(false);
            InvokeBotEvent(this, BotEvents.TwitchStreamOffline, e);

            StreamOffline?.Invoke(this, new());
        }
        private void TwitchStreamerEventSubBot_NewChannelRaid(object sender, NewChannelRaidEventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewChannelRaid", DebugLogTypes.TwitchBots, "EventSub bot received an incoming raid notification.");

            var Category = GetUserCategory(UserId: e.ChannelRaid.FromBroadcasterUserId);

            InvokeBotEvent(this, BotEvents.TwitchIncomingRaid,
                new OnIncomingRaidArgs()
                {
                    Category = Category,
                    RaidTime = DateTime.Now.ToLocalTime(),
                    ViewerCount = e.ChannelRaid.Viewers,
                    LiveUser = new(e.ChannelRaid.FromBroadcasterUserName, Platform.Twitch, e.ChannelRaid.FromBroadcasterUserId)
                });
        }

        private void TwitchStreamerEventSubBotNoScopes_OutChannelRaid(object sender, NewChannelRaidEventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBotNoScopes_OutChannelRaid", DebugLogTypes.TwitchBots, $"EventSub bot received an outgoing raid notification to channel {e.ChannelRaid.ToBroadcasterUserName}.");

            InvokeBotEvent(this, BotEvents.TwitchOutgoingRaid,
                            new OnStreamRaidResponseEventArgs()
                            {
                                ToChannel = e.ChannelRaid.ToBroadcasterUserName,
                                CreatedAt = e.RaidTime
                            });
        }

        #endregion

        #region Streamer EventSub Bot

        private void TwitchStreamerEventSubBot_NewChannelSubscriptionGift(object sender, NewChannelSubscriptionGiftEventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewChannelSubscriptionGift", DebugLogTypes.TwitchBots, "EventSub bot received a gifted subscription message.");
            InvokeBotEvent(this, BotEvents.TwitchCommunitySubscription, e);
        }
        private void TwitchStreamerEventSubBot_NewChannelSubscribe(object sender, NewChannelSubscribeEventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewChannelSubscribe", DebugLogTypes.TwitchBots, "EventSub bot received a new subscriber message.");
            InvokeBotEvent(this, BotEvents.TwitchNewSubscriber, e);
        }
        private void TwitchStreamerEventSubBot_NewChannelSubscriptionMessage(object sender, NewChannelSubscriptionMessageEventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewChannelSubscriptionMessage", DebugLogTypes.TwitchBots, "EventSub bot received a re-subscriber message.");
            InvokeBotEvent(this, BotEvents.TwitchReSubscriber, e);
        }
        private void TwitchStreamerEventSubBot_NewChannelCustomRewardRedemption(object sender, NewChannelCustomRewardRedemptionEventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewChannelCustomRewardRedemption", DebugLogTypes.TwitchBots, "EventSub bot received a channel point-reward redemption, posting into system.");

            InvokeBotEvent(this, BotEvents.TwitchChannelPointsRewardRedeemed, e);
        }
        private void TwitchStreamerEventSubBot_NewChannelFollow(object sender, NewChannelFollowEventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewChannelFollow", DebugLogTypes.TwitchBots, "Detected new followers.");

            InvokeBotEvent(this, BotEvents.TwitchPostNewFollowers, e);
        }
        private void TwitchStreamerEventSubBot_NewChannelCheer(object sender, NewChannelCheerEventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBot_NewChannelCheer", DebugLogTypes.TwitchBots, $"Received {e.ChannelCheer.Bits} bits/cheer.");

            InvokeBotEvent(this, BotEvents.TwitchChannelCheer, e);
        }
        public override void ManageStreamOnlineOfflineStatus(bool Start)
        {
            LogWriter.DebugLog("ManageStreamOnlineOfflineStatus", DebugLogTypes.TwitchBots, $"Now managing starting or stopping bots " +
                "since active livestream={Start}");

            if (Start)
            {
                LogWriter.DebugLog("ManageStreamOnlineOfflineStatus", DebugLogTypes.TwitchBots, "Starting bots now that the livestream " +
                    "is online.");

                if (OptionFlags.TwitchClipConnectOnline)
                {
                    TwitchBotClipSvc.StartBot();
                }
            }
            else
            {
                LogWriter.DebugLog("ManageStreamOnlineOfflineStatus", DebugLogTypes.TwitchBots, "Stopping bots now that the " +
                    "livestream is offline.");

                if (OptionFlags.TwitchClipDisconnectOffline)
                {
                    TwitchBotClipSvc.StopBot();
                }

            }

            LogWriter.DebugLog("ManageStreamOnlineOfflineStatus", DebugLogTypes.TwitchBots, "Finished managing the bots based " +
                "on current livestream status (online or offline).");
        }

        private void TwitchStreamerEventSubBotScopes_OnChannelChatMessageStarted(object sender, EventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBotScopes_OnChannelChatMessageStarted", DebugLogTypes.TwitchBots, "Bot EventSub bot started to read channel chat messages.");

            InvokeBotEvent(this, BotEvents.TwitchBotEventSubStarted, null);
        }
        private void TwitchStreamerEventSubBotScopes_OnChannelChatMessageStopping(object sender, EventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBotScopes_OnChannelChatMessageStopping", DebugLogTypes.TwitchBots, "Bot EventSub bot stopped and won't read channel chat messages.");

            InvokeBotEvent(this, BotEvents.TwitchBotEventSubStopping, null);
        }
        private void TwitchStreamerEventSubBotScopes_OnChannelChatMessageStopped(object sender, EventArgs e)
        {
            LogWriter.DebugLog("TwitchStreamerEventSubBotScopes_OnChannelChatMessageStopped", DebugLogTypes.TwitchBots, "Bot EventSub bot stopped and won't read channel chat messages.");

            InvokeBotEvent(this, BotEvents.TwitchBotEventSubStopped, null);
        }

        #endregion

        #region Twitch LiveMonitor - Multichannels

        private void TwitchLiveMonitor_OnBotStarted(object sender, EventArgs e)
        {
            LogWriter.DebugLog("TwitchLiveMonitor_OnBotStarted", DebugLogTypes.TwitchBots, "Live bot started, registering handles.");
            LogWriter.DebugLog("TwitchLiveMonitor_OnBotStarted", DebugLogTypes.TwitchBots, "Bot started, registering handles.");

            RegisterHandlers();
        }

        private void LiveStreamMonitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            LogWriter.DebugLog("LiveStreamMonitor_OnStreamOnline", DebugLogTypes.TwitchBots, $"Registered a stream is online, {e.Channel}.");
            LogWriter.DebugLog("LiveStreamMonitor_OnStreamOnline", DebugLogTypes.TwitchBots, $"Found {e.Channel} is now online.");
            InvokeBotEvent(this, BotEvents.TwitchMultiStreamOnline, e);
        }

        private void TwitchBotLiveMonitorSvc_OnBotStopped(object sender, EventArgs e)
        {
            CheckActiveBots();
        }

        #endregion

        #region Twitch Helix Calls
        //private static Models.LiveUser AddUserId(string s)
        //{
        //    LogWriter.DebugLog("AddUserId", DebugLogTypes.TwitchBots, "Adding user Id to the existing user name.");

        //    Models.LiveUser user = new(s, Platform.Twitch);

        //    string userId = DataManager.GetUserId(user);
        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        user.UserId = TwitchHelixBot.GetUserId(user.UserName);
        //    }
        //    else
        //    {
        //        user.UserId = userId;
        //    }
        //    return user;
        //}

        public static CategoryData GetUserCategory(string UserId = null, string UserName = null)
        {
            LogWriter.DebugLog("GetUserCategory", DebugLogTypes.TwitchBots, "Performing request to receive a user category.");

            return TwitchHelixBot.GetUserGameCategory(UserId: UserId, UserName: UserName);
        }

        public static DateTime GetUserAccountAge(string UserId = null, string UserName = null)
        {
            LogWriter.DebugLog("GetUserAccountAge", DebugLogTypes.TwitchBots, "Performing a request to find an account age.");

            return TwitchHelixBot.GetUserCreatedAt(UserName: UserName, UserId: UserId);
        }

        public static bool VerifyUserExist(string UserName)
        {
            LogWriter.DebugLog("VerifyUserExist", DebugLogTypes.TwitchBots, "Performing a user verification request.");

            return TwitchHelixBot.GetUserId(UserName) != null;
        }

        public void SendShoutOut(LiveUser user)
        {
            if (!ShoutOutTaskActive)
            {
                ShoutOutTaskActive = true;
                ThreadManager.CreateThreadStart("SendShoutOut", ()=>EvaluateShoutOutUsers());
            }

            lock (ShoutOutUsers)
            {
                ShoutOutLiveUser shoutOutLiveUser = new(user);
                if (!ShoutOutUsers.UniqueAdd(shoutOutLiveUser))
                {
                    var shoutuser = ShoutOutUsers.Find(s => s.Equals(shoutOutLiveUser) && (s.LastShoutOut != null && s.LastShoutOut.Value.AddMinutes(15) < DateTime.Now));
                    if (shoutuser != null)
                    {
                        shoutuser.NextShoutOut = shoutuser.LastShoutOut.Value.AddHours(1);
                    }
                }
            }
        }

        public static void BanUserRequest(string UserName, BanReasons Reason, int Duration = 0)
        {
            LogWriter.DebugLog("BanUserRequest", DebugLogTypes.TwitchBots, "Performing a request to ban a user.");

            TwitchHelixBot.BanUser(UserName, Reason, Duration);
        }

        public static bool ModifyChannelInformation(string Title = null, string CategoryName = null, string CategoryId = null)
        {
            LogWriter.DebugLog("ModifyChannelInformation", DebugLogTypes.TwitchBots, "Performing request to modify channel information.");

            bool result = false;

            if (Title != null)
            {
                result = TwitchHelixBot.SetChannelTitle(Title);
            }
            if (CategoryName != null || CategoryId != null)
            {
                result = TwitchHelixBot.SetChannelCategory(CategoryName, CategoryId);
            }

            return result;
        }

        public static string GetUserId(string UserName)
        {
            LogWriter.DebugLog("GetUserId", DebugLogTypes.TwitchBots, "Performing a request to get a user Id.");

            return TwitchHelixBot.GetUserId(UserName);
        }

        public static void RaidChannel(string ToUserName)
        {
            LogWriter.DebugLog("RaidChannel", DebugLogTypes.TwitchBots, "Performing a request to raid a channel.");

            TwitchHelixBot.RaidChannel(ToUserName);
        }

        public static void CancelRaidChannel()
        {
            LogWriter.DebugLog("CancelRaidChannel", DebugLogTypes.TwitchBots, "Performing a request to cancel a raid.");

            TwitchHelixBot.CancelRaidChannel();
        }

        //private void TwitchBotUserSvc_StartRaidEventResponse(object sender, OnStreamRaidResponseEventArgs e)
        //{
        //    LogWriter.DebugLog("TwitchBotUserSvc_StartRaidEventResponse", DebugLogTypes.TwitchBots, "Registering the raid command, to track the raid as bot doesn't receive a now-completed raid message.");

        //    // StartRaid(e.ToChannel, e.CreatedAt.ToLocalTime());
        //}

        //private void TwitchBotUserSvc_CancelRaidEvent(object sender, EventArgs e)
        //{
        //    LogWriter.DebugLog("TwitchBotUserSvc_CancelRaidEvent", DebugLogTypes.TwitchBots, "Registering the cancel raid command, to stop the raid tracking code.");

        //    CancelRaidLoop();
        //}

        public static void GetViewerCount()
        {
            LogWriter.DebugLog("GetViewerCount", DebugLogTypes.TwitchBots, "Performing a request to get the current channel viewer count.");

            TwitchHelixBot.GetViewerCount(OptionFlags.TwitchChannelName);
        }

        private void TwitchBotUserSvc_OnGetStreamsViewerCount(object sender, GetStreamsEventArgs e)
        {
            LogWriter.DebugLog("TwitchBotUserSvc_OnGetStreamsViewerCount", DebugLogTypes.TwitchBots, "With viewer count only part of the uptime command, the bot sends out the viewer count request, then sends in the command with the viewer count added to it.");

            PostInternalCommand(LocalizedMsgSystem.GetVar(DefaultCommand.uptime), [e.ViewerCount.ToString()], $"!{LocalizedMsgSystem.GetVar(MsgVars.uptime)} {e.ViewerCount}");
        }

        /// <summary>
        /// Retrieves the current Twitch stream detail.
        /// </summary>
        /// <param name="UserId">The channel user Id to get the stream information.</param>
        /// <param name="UserName">The channel user name to get the stream information.</param>
        /// <returns>The details of the current streams for the provided UserId or UserName.</returns>
        public static GetStreamsResponse GetStreamDetail(string UserId = null, string UserName = null)
        {
            return TwitchHelixBot.GetStreamDetail(UserId, UserName);
        }

        internal void PostInternalCommand(string Com, List<string> ComArgs, string ComMessage)
        {
            LogWriter.DebugLog("PostInternalCommand", DebugLogTypes.TwitchBots, "Performs an internal command action.");

            InvokeBotEvent(this, BotEvents.TwitchBotCommandCall, new SendBotCommandEventArgs()
            {
                CmdMessage = new()
                {
                    CommandArguments = ComArgs,
                    CommandText = Com,
                    DisplayName = OptionFlags.TwitchBotUserName,
                    Channel = OptionFlags.TwitchChannelName,
                    IsBroadcaster = OptionFlags.TwitchBotUserName == OptionFlags.TwitchChannelName,
                    IsHighlighted = false,
                    IsMe = false,
                    IsModerator = OptionFlags.TwitchBotUserName != OptionFlags.TwitchChannelName,
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

        private void TwitchHelixBot_GetChannelGameName(object sender, OnGetChannelGameNameEventArgs e)
        {
            LogWriter.DebugLog("TwitchHelixBot_GetChannelGameName", DebugLogTypes.TwitchBots, "Updating channel category game name.");

            InvokeBotEvent(this, BotEvents.TwitchCategoryUpdate, e);
        }

        #endregion

        #region Twitch Token Bot

        /// <summary>
        /// When bots start or stop, check the current active to ensure the token gets updated as necessary - minimize update calls to Twitch
        /// </summary>
        private void CheckActiveBots()
        {
            // clipservice - streamer token
            // LiveMonitor - streamer token
            // HelixBot - uses StreamerHelix token; including 'GetAllFollowers', when active
            // EventSubBot - bot token
            // EventSubBot - both streamer token & streamer no scopes token
            // sendchatclient - bot token

            bool streamertoken = false;
            bool streamernoscopestoken = false;
            bool bottoken = false;

            foreach (var bot in BotsList)
            {
                if (bot.BotClientName == Bots.TwitchClipBot)
                {
                    streamertoken |= ((TwitchBotClipSvc)bot).IsActive == true;
                }
                else if (bot.BotClientName == Bots.TwitchMultiBot)
                {
                    streamertoken |= ((TwitchBotLiveMonitorSvc)bot).IsActive == true;
                }
                else if (bot.BotClientName == Bots.TwitchEventSubBot)
                { // includes TiwtchBotSendChatClient - need to receive chat to send chat
                    bottoken |= ((TwitchEventSub)bot).IsActive == true;
                }
                else if (bot.BotClientName == Bots.TwitchEventSubStreamer)
                {
                    bool curr = ((TwitchEventSub)bot).IsActive == true;
                    streamertoken |= curr;
                    streamernoscopestoken |= curr;
                }
            }

            TwitchTokenBot.UpdateActiveTokens(BotType.BotAccount, bottoken);
            TwitchTokenBot.UpdateActiveTokens(BotType.StreamerAccount, streamertoken);
            TwitchTokenBot.UpdateActiveTokens(BotType.StreamerNoScopes, streamernoscopestoken);

        }

        public static void TwitchActivateAuthCode(string clientId, bool NoScopes, Action<string> OpenBrowser, Action AuthenticationFinished)
        {
            LogWriter.DebugLog("TwitchActivateAuthCode", DebugLogTypes.TwitchBots, "Received request to start the Twitch auth code approval process.");

            ThreadManager.CreateThreadStart("TwitchActivateAuthCode", () =>
            {
                LogWriter.DebugLog("TwitchActivateAuthCode", DebugLogTypes.TwitchBots, "Asking Twitch Token Bot to create the " +
                    "authorization URL for the user to approve this application access to their account.");

                TwitchTokenBot.GenerateAuthCodeURL(clientId, NoScopes, OpenBrowser, AuthenticationFinished);
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

        private string AuthCodeStreamerNoScopeState = "";
        private Action<string> AuthCodeStreamerNoScopeAction;

        private bool HttpStarted = false;

        /// <summary>
        /// HTTP Listener on the auth code redirect URL, listening for responses to the user authenticating bot & streamer access.
        /// </summary>
        private void AuthCodeListener()
        {
            if (!HttpStarted)
            {
                // start an http listener to receive auth code
                ThreadManager.CreateThreadStart("AuthCodeListener", () =>
                {
                    HttpStarted = true;
                    HttpListener httpListener = new();
                    httpListener.Prefixes.Add(OptionFlags.TwitchAuthRedirectURL + (OptionFlags.TwitchAuthRedirectURL.EndsWith('/') ? "" : "/")); // requires ending '/' to URL
                    httpListener.Start();

                    LogWriter.DebugLog("AuthCodeListener", DebugLogTypes.TwitchBots, "Started http listener for when user " +
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

                    if (QueryValues["state"] == AuthCodeStreamerNoScopeState)
                    {
                        LogWriter.DebugLog("AuthCodeListener", DebugLogTypes.TwitchBots, "Handling the streamer account authorization response.");

                        if (!QueryValues.AllKeys.Contains("error"))
                        {
                            LogWriter.DebugLog("AuthCodeListener", DebugLogTypes.TwitchBots, "Determined user approved the application access.");

                            OptionFlags.TwitchAuthStreamerNoScopesAuthCode = QueryValues["code"];
                        }
                        else
                        {
                            LogWriter.DebugLog("AuthCodeListener", DebugLogTypes.TwitchBots, "Determined user didn't approve the application access.");

                            AuthCodeStreamerNoScopeAction(Msgs.MsgTwitchAuthFailedAuthentication);
                            OptionFlags.TwitchAuthStreamerNoScopesAuthCode = null;
                        }
                        lock (AuthCodeStreamerNoScopeState)
                        {
                            AuthCodeStreamerNoScopeState = "";
                        }
                    }
                    else if (QueryValues["state"] == AuthCodeStreamerState)
                    {
                        LogWriter.DebugLog("AuthCodeListener", DebugLogTypes.TwitchBots, "Handling the streamer account authorization response.");

                        if (!QueryValues.AllKeys.Contains("error"))
                        {
                            LogWriter.DebugLog("AuthCodeListener", DebugLogTypes.TwitchBots, "Determined user approved the application access.");

                            OptionFlags.TwitchAuthStreamerAuthCode = QueryValues["code"];
                        }
                        else
                        {
                            LogWriter.DebugLog("AuthCodeListener", DebugLogTypes.TwitchBots, "Determined user didn't approve the application access.");

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
                        LogWriter.DebugLog("AuthCodeListener", DebugLogTypes.TwitchBots, "Handling the bot account authorization response.");

                        if (!QueryValues.AllKeys.Contains("error"))
                        {
                            LogWriter.DebugLog("AuthCodeListener", DebugLogTypes.TwitchBots, "Determined user approved the application access.");

                            OptionFlags.TwitchAuthBotAuthCode = QueryValues["code"];
                        }
                        else
                        {
                            LogWriter.DebugLog("AuthCodeListener", DebugLogTypes.TwitchBots, "Determined user didn't approve the application process.");

                            AuthCodeBotAction(Msgs.MsgTwitchAuthFailedAuthentication);
                            OptionFlags.TwitchAuthBotAuthCode = null;
                        }

                        lock (AuthCodeBotState)
                        {
                            AuthCodeBotState = "";
                        }
                    }

                    LogWriter.DebugLog("AuthCodeListener", DebugLogTypes.TwitchTokenBot, "Checking tokens.");
                    TwitchTokenBot.CheckToken();
                    LogWriter.DebugLog("AuthCodeListener", DebugLogTypes.TwitchBots, "Captured the auth code. Now performing the " +
                        "finishing action.");

                    FinishedAuthenticationAction.Invoke();

                    HttpStarted = false;
                });
            }
        }

        private void TwitchTokenBot_BotAcctAuthCodeExpired(object sender, TwitchAuthCodeExpiredEventArgs e)
        {
            if (e?.OpenBrowser != null)
            {
                LogWriter.DebugLog("TwitchTokenBot_BotAcctAuthCodeExpired", DebugLogTypes.TwitchBots, "Starting Twitch auth code approval for the bot " +
    "account.");

                lock (AuthCodeBotState)
                {
                    AuthCodeBotState = e.State;
                }
                AuthCodeBotAction = e.OpenBrowser;
                FinishedAuthenticationAction = e.AuthenticationFinished;

                LogWriter.DebugLog("TwitchTokenBot_BotAcctAuthCodeExpired", DebugLogTypes.TwitchBots, $"Start the http listener " +
    $"on {OptionFlags.TwitchAuthRedirectURL} to get ready for when the user authorizes application access to their account.");

                AuthCodeListener();

                // call the provided method to give user the web based app authorization URL
                e.OpenBrowser(e.AuthURL);
            }
            else
            {
                LogWriter.DebugLog("TwitchTokenBot_BotAcctAuthCodeExpired", DebugLogTypes.TwitchBots, "Determined the auth code expired and user isn't " +
    "ready to authorize the application. Need to stop all of the bots, since the access tokens are no longer valid; auth code is now invalid.");

                foreach (IIOModule bot in BotsList)
                {
                    bot.StopBot();
                }
                InvalidTwitchAccess?.Invoke(this, new(Platform.Twitch, e.BotType));
            }
        }

        private void TwitchTokenBot_StreamerNoScopesAuthCodeExpired(object sender, TwitchAuthCodeExpiredEventArgs e)
        {
            if (e?.OpenBrowser != null)
            {
                LogWriter.DebugLog("TwitchTokenBot_StreamerNoScopesAuthCodeExpired", DebugLogTypes.TwitchBots, "Starting Twitch auth code approval for the no scopes streamer " +
                    "credential.");

                lock (AuthCodeStreamerState)
                {
                    AuthCodeStreamerNoScopeState = e.State;
                }
                AuthCodeStreamerNoScopeAction = e.OpenBrowser;
                FinishedAuthenticationAction = e.AuthenticationFinished;

                LogWriter.DebugLog("TwitchTokenBot_StreamerNoScopesAuthCodeExpired", DebugLogTypes.TwitchBots, $"Start the http listener " +
                   $"on {OptionFlags.TwitchAuthRedirectURL} to get ready for when the user authorizes application access to their account.");

                AuthCodeListener();

                // call the provided method to give user the web based app authorization URL
                e.OpenBrowser(e.AuthURL);
            }
            else
            {
                LogWriter.DebugLog("TwitchTokenBot_StreamerNoScopesAuthCodeExpired", DebugLogTypes.TwitchBots, "Determined the auth code expired and user isn't " +
                    "ready to authorize the application. Need to stop all of the bots, since the access tokens are no longer valid; auth code is now invalid.");

                foreach (IIOModule bot in BotsList)
                {
                    bot.StopBot();
                }
                InvalidTwitchAccess?.Invoke(this, new(Platform.Twitch, e.BotType));
            }
        }

        private void TwitchTokenBot_StreamerAcctAuthCodeExpired(object sender, TwitchAuthCodeExpiredEventArgs e)
        {
            if (e?.OpenBrowser != null)
            {
                LogWriter.DebugLog("TwitchTokenBot_StreamerAcctAuthCodeExpired", DebugLogTypes.TwitchBots, "Starting Twitch auth code approval for the streamer " +
                    "credential.");

                lock (AuthCodeStreamerState)
                {
                    AuthCodeStreamerState = e.State;
                }
                AuthCodeStreamerAction = e.OpenBrowser;
                FinishedAuthenticationAction = e.AuthenticationFinished;

                LogWriter.DebugLog("TwitchTokenBot_StreamerAcctAuthCodeExpired", DebugLogTypes.TwitchBots, $"Start the http listener " +
                    $"on {OptionFlags.TwitchAuthRedirectURL} to get ready for when the user authorizes application access to their account.");

                AuthCodeListener();

                // call the provided method to give user the web based app authorization URL
                e.OpenBrowser(e.AuthURL);
            }
            else
            {
                LogWriter.DebugLog("TwitchTokenBot_StreamerAcctAuthCodeExpired", DebugLogTypes.TwitchBots, "Determined the auth code expired and user isn't " +
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
            LogWriter.DebugLog("ForceTwitchReauthorization", DebugLogTypes.TwitchBots, $"Received a request to reauthenticate.");

            TwitchTokenBot.ForceReauthorization();
        }

        #endregion

        #region Token Clip Service
        private void TwitchBotClipSvc_OnBotStarted(object sender, EventArgs e)
        {
            LogWriter.DebugLog("TwitchBotClipSvc_OnBotStarted", DebugLogTypes.TwitchBots, "Found clip bot started, now adding handlers.");
            RegisterHandlers();

            // start thread to retrieve all clips
            BulkLoadClips = ThreadManager.CreateThread("TwitchBotClipSvc_OnBotStarted", ProcessClipsAsync);
            MultiThreadOps.Add(BulkLoadClips);
            BulkLoadClips.Start();
        }

        private void TwitchBotClipSvc_OnBotStopped(object sender, EventArgs e)
        {
            CheckActiveBots();
        }

        public void ClipMonitorServiceOnNewClipFound(object sender, OnNewClipsDetectedArgs e)
        {
            LogWriter.DebugLog("ClipMonitorServiceOnNewClipFound", DebugLogTypes.TwitchBots, "Detected a new clip to post.");

            if (e != null)
            {
                InvokeBotEvent(this, BotEvents.TwitchPostNewClip, e);
            }
        }

        /// <summary>
        /// Get the clips for a specific user channel.
        /// </summary>
        /// <param name="ChannelName">Channel to get the clips.</param>
        /// <param name="ReturnData">The callback method when the clips are found.</param>
        public void GetChannelClips(Action<List<Models.Clip>> ReturnData)
        {
            LogWriter.DebugLog("GetChannelClips", DebugLogTypes.TwitchBots, "Performing a request to get channel clips.");
            ThreadManager.CreateThreadStart("GetChannelClips", () => ProcessChannelClipsAsync(ReturnData));
        }

        /// <summary>
        /// Creates a clip, per Twitch API (30 seconds of the prior 90 seconds of broadcasted video), of the current streamer channel.
        /// </summary>
        public static void CreateClip()
        {
            LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchBots, "Performing a request to create a clip.");

            TwitchBotClipSvc.CreateClip();
        }

        #endregion

        #region Threaded Ops

        public override void GetAllFollowers()
        {
            if (OptionFlags.ManageFollowers)// && OptionFlags.TwitchAddFollowersStart)
            {
                LogWriter.DebugLog("GetAllFollowers", DebugLogTypes.TwitchBots, "Processing request to update all followers to " +
                    "the streamer's channel.");

                LogWriter.DebugLog("GetAllFollowers", DebugLogTypes.TwitchBots, "Prepare to bulk-add Twitch followers.");

                InvokeBotEvent(this, BotEvents.TwitchStartBulkFollowers, null);

                try
                {
                    LogWriter.DebugLog("GetAllFollowers", DebugLogTypes.TwitchBots, "Started the bulk-add " +
                        "process for the followers of the Twitch streamer channel.");

                    TwitchHelixBot.GetAllFollowers();
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "GetAllFollowers");
                }

                LogWriter.DebugLog("GetAllFollowers", DebugLogTypes.TwitchBots, "Wrap up bulk-add Twitch followers.");
            }
        }

        private void TwitchHelixBot_OnBulkFollowsUpdate(object sender, OnNewFollowersDetectedArgs e)
        {
            LogWriter.DebugLog("TwitchHelixBot_OnBulkFollowsUpdate", DebugLogTypes.TwitchBots, $"Received event to begin bulk followers update.");

            InvokeBotEvent(
                this,
                BotEvents.TwitchBulkPostFollowers,
                new OnNewFollowersDetectedArgs()
                {
                    NewFollowers = e.NewFollowers
                });
        }

        private void TwitchHelixBot_BulkFollowsCompleted(object sender, EventArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchStopBulkFollowers, null);
        }

        private bool ActiveUserThread;
        private void ActiveUsers()
        {
            if (!ActiveUserThread)
            {
                ActiveUserThread = true;
                ThreadManager.CreateThreadStart("ActiveUsers", () =>
                {
                    LogWriter.DebugLog("ActiveUsers", DebugLogTypes.TwitchBots, "Starting to monitor active users in the stream.");
                    const int SecondsBetweenUserCheck = 90;

                    DateTime LastChecked = DateTime.Now;

                    while (OptionFlags.ActiveToken && OptionFlags.IsStreamOnline)
                    {
                        if (DateTime.Now >= LastChecked)
                        {
                            LogWriter.DebugLog("ActiveUsers", DebugLogTypes.TwitchBots, "Time to check active users in the current stream.");
                            LastChecked = LastChecked.AddSeconds(SecondsBetweenUserCheck);
                            InvokeBotEvent(this, BotEvents.TwitchCurrentUsers, new StreamerOnExistingUserDetectedArgs()
                            {
                                Users = (from C in TwitchHelixBot.GetChatters()
                                         let LU = new LiveUser(C.UserName, Platform.Twitch, C.UserId)
                                         select LU).ToList() ?? []
                            });
                        }
                        Thread.Sleep(1000); // wait and check; query for new chatters every {SecondsBetweenUserCheck} timeframe -short wait to permit thread to shut down if bot closes
                    }
                    ActiveUserThread = false;
                });
            }
        }

        private List<TwitchLib.Api.Helix.Models.Clips.GetClips.Clip> ClipList { get; set; }

        private bool StartClips { get; set; }

        private async void ProcessClipsAsync()
        {
            if (!StartClips)
            {
                LogWriter.DebugLog("ProcessClipsAsync", DebugLogTypes.TwitchBots, "Retrieving all clips for the channel.");

                StartClips = true;
                ClipList = await TwitchBotClipSvc.GetAllClipsAsync();

                if (ClipList != null)
                {
                    InvokeBotEvent(this, BotEvents.TwitchClipSvcOnClipFound, new ClipFoundEventArgs() { ClipList = ClipList });
                }
                StartClips = false;
            }
        }

        private async void ProcessChannelClipsAsync(Action<List<Models.Clip>> ActionCallback)
        {
            LogWriter.DebugLog("ProcessChannelClipsAsync", DebugLogTypes.TwitchBots, "Retrieving all clips for the channel.");

            List<TwitchLib.Api.Helix.Models.Clips.GetClips.Clip> result = [];
            TwitchBotClipSvc ChannelClips = new(TwitchTokenBot);
            await ChannelClips.StartBot();
            if (ChannelClips.IsActive == true)
            {
                result = await ChannelClips.GetAllClipsAsync();
                await ChannelClips.StopBot();
            }

            ActionCallback?.Invoke(BotIOController.BotController.ConvertClips(result));
        }

        private bool ShoutOutTaskActive = false;
        private List<ShoutOutLiveUser> ShoutOutUsers = [];

        private Task EvaluateShoutOutUsers()
        {
            // NewUserEntry: Different users can only be shoutout once every 2 minutes
            // -LastShoutOut = null, NextShoutOut = null => first shoutout occurs asap
            // 
            // ExistingUserEntry: Same user can only be shoutout after at least every 60 minutes
            // -LastShoutOut = value, NextShoutOut = null => no shoutout scheduled
            // -LastShoutOUt = value, NextShoutOut = value => computed next shoutout to perform

            return Task.Run(async () =>
            {
                try
                {
                    DateTime lastShoutOut = DateTime.MinValue;

                    while (OptionFlags.ActiveToken)
                    {
                        ShoutOutLiveUser nextShoutOut = null;
                        lock (ShoutOutUsers)
                        {
                            foreach (var S in ShoutOutUsers)
                            {
                                if (S.LastShoutOut == null && S.NextShoutOut == null)
                                {
                                    nextShoutOut ??= S;
                                    break;
                                }
                                else if (S.NextShoutOut != null)
                                {
                                    nextShoutOut ??= S;
                                    break;
                                }
                            }
                        }

                        DateTime Curr = DateTime.Now;

                        if (nextShoutOut.NextShoutOut == null && lastShoutOut.AddMinutes(2) <= Curr)
                        { // new shoutout user, allowed per Twitch API every 2 minutes
                            TwitchHelixBot.SendShoutOut(nextShoutOut.User.UserId, nextShoutOut.User.UserName);

                            lock (ShoutOutUsers)
                            {
                                nextShoutOut.LastShoutOut = Curr;
                            }

                            lastShoutOut = Curr;
                        }
                        else if (lastShoutOut.AddHours(1) <= Curr && nextShoutOut.NextShoutOut < Curr)
                        { // existing shoutout user, allowed per Twitch API every 60 minutes
                            TwitchHelixBot.SendShoutOut(nextShoutOut.User.UserId, nextShoutOut.User.UserName);

                            lock (ShoutOutUsers)
                            {
                                nextShoutOut.LastShoutOut = Curr;
                                nextShoutOut.NextShoutOut = null;
                            }

                            lastShoutOut = Curr;
                        }
                        await Task.Delay(5000);
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "EvaluateShoutOutUsers");
                    ShoutOutTaskActive = false;
                }
            });
        }


        //private readonly TimeSpan DefaultOutRaid = new(0, 0, 90);
        //private DateTime OutRaidStarted;
        //private Thread RaidLoop = null;
        //private readonly string RaidLock = "lock";

        ///// <summary>
        ///// Establishes a Twitch raid procedure to hold up to 90 seconds, per Twitch API doc, where the user can still cancel the 
        ///// raid and the bots can't respond to stream going offline until after the raid completes.
        ///// 
        ///// When the user quick raids (clicks a raid now button before the timer completes), this check continues waiting until timer completes.
        ///// </summary>
        ///// <param name="ToChannelName"></param>
        ///// <param name="RaidCreated"></param>
        //private void StartRaid(string ToChannelName, DateTime RaidCreated)
        //{
        //    LogWriter.DebugLog("StartRaid", DebugLogTypes.TwitchBots, "Starting internal raid loop procedure to " +
        //        "send 'stream offline' events across the application to finish an active stream.");
        //    LogWriter.DebugLog("StartRaid", DebugLogTypes.TwitchBots, "Twitch no longer notifies through the Twitch " +
        //        "Chat API that a raid occurred for a channel.");
        //    LogWriter.DebugLog("StartRaid", DebugLogTypes.TwitchBots, "But, Twitch added to the API to permit a bot " +
        //        "to start a raid, meaning, anyone with command access to the channel can initiate a raid, alongside the streamer.");
        //    LogWriter.DebugLog("StartRaid", DebugLogTypes.TwitchBots, "So, this procedure tracks the following 90 " +
        //        "seconds, in case the user cancels the raid.");
        //    LogWriter.DebugLog("StartRaid", DebugLogTypes.TwitchBots, "Otherwise, after 90 seconds, regardless if " +
        //        "the user completes the raid early (clicked 'raid now' button) and notify rest of the bot of the raid.");


        //    lock (RaidLock)
        //    {
        //        OutRaidStarted = RaidCreated;
        //    }

        //    if (RaidLoop == null && OptionFlags.IsStreamOnline) // create only 1 thread & when stream is online
        //    {
        //        RaidLoop = ThreadManager.CreateThread("StartRaid", () =>
        //        {
        //            LogWriter.DebugLog("StartRaid", DebugLogTypes.TwitchBots, "Start to wait on the raid.");

        //            // declare locals, so we can use a thread-safe lock
        //            DateTime LocalRaidStart;
        //            bool LocalRaidStarted;

        //            lock (RaidLock)
        //            {
        //                LogWriter.DebugLog("StartRaid", DebugLogTypes.TwitchBots, "Acknowledged the raid start time.");

        //                LocalRaidStart = OutRaidStarted;
        //                LocalRaidStarted = OptionFlags.TwitchOutRaidStarted;
        //            }

        //            while (DateTime.Now - LocalRaidStart <= DefaultOutRaid && LocalRaidStarted)
        //            { // check for 90 seconds
        //                LogWriter.DebugLog("StartRaid", DebugLogTypes.TwitchBots, $"Checking if {LocalRaidStart}+{DefaultOutRaid.Seconds} seconds is after " +
        //                    $"the current time. And wait some time to check again if user cancels the raid (via !cancelraid command).");

        //                lock (RaidLock)
        //                { // update values, in case they changed - use thread lock safety as another thread may change these
        //                    LocalRaidStart = OutRaidStarted;
        //                    LocalRaidStarted = OptionFlags.TwitchOutRaidStarted;
        //                }
        //                Thread.Sleep(5000);
        //            }

        //            // if the raid wasn't canceled after the loop finished, send raid event to main bot
        //            if (LocalRaidStarted)
        //            {
        //                LogWriter.DebugLog("StartRaid", DebugLogTypes.TwitchBots, "Raid succeeded. Proceeding to inform the " +
        //                    "rest of the application to shutdown any bots for going offline and recording the outgoing raid details.");

        //                InvokeBotEvent(this, BotEvents.TwitchOutgoingRaid,
        //                    new OnStreamRaidResponseEventArgs()
        //                    {
        //                        ToChannel = ToChannelName,
        //                        CreatedAt = OutRaidStarted
        //                    });

        //                RaidCompleted?.Invoke(this, new());
        //            }
        //            RaidLoop = null;
        //        });
        //        RaidLoop.Start();
        //    }

        //}

        ///// <summary>
        ///// Settings an option, thread safe, to cancel the pending Twitch raid.
        ///// </summary>
        //private void CancelRaidLoop()
        //{
        //    LogWriter.DebugLog("CancelRaidLoop", DebugLogTypes.TwitchBots, "Received a 'cancel raid' request.");

        //    lock (RaidLock)
        //    {
        //        OptionFlags.TwitchOutRaidStarted = false;
        //    }
        //}

        #endregion

    }
}
