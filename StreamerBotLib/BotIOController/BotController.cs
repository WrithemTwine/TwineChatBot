using StreamerBotLib.BotClients;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub;
using StreamerBotLib.Models.BotManagement;
using StreamerBotLib.Models.Data;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Models.Schedule;
using StreamerBotLib.Models.Users;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.ObjectModel;
using System.Data;

using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

// TODO: Add Bot contacts users to invoke conversation; carry-on conversation with existing

namespace StreamerBotLib.BotIOController
{
    /// <summary>
    /// The BotController is the central hub for the bots to interact with the system. It receives events from the bots, and then invokes the appropriate methods in the system to handle those events. It also receives requests from the system to send messages to the bots, and then sends those messages to the appropriate bots. The BotController also manages the lifecycle of the bots, such as initializing them, starting them, and stopping them. The BotController also manages the database interactions for the bots, such as clearing data, updating data, and retrieving data. The BotController also manages the Twitch API interactions for the bots, such as getting user information, getting stream information, and modifying channel information.
    /// </summary>
    public partial class BotController
    {

        public event EventHandler<PostChannelMessageEventArgs> OutputSentToBots;
        public event EventHandler<InvalidAccessTokenEventArgs> InvalidAuthorizationToken;
        public event EventHandler TokensInitialized;

        public event EventHandler OnStreamOnline;
        public event EventHandler<FindChannelCategoryEventArgs> OnStreamCategoryChanged;
        public event EventHandler OnStreamOffline;

        internal event EventHandler OnBulkFollowerStarted;

        private readonly Dictionary<Platform, bool> PlatformOnlineStatus = new(from Platform P in Enum.GetValues<Platform>()
                                                                               select new KeyValuePair<Platform, bool>(P, false));

        // public SystemsController Systems { get; private set; }
        public static DataBot DataBot { get; private set; }

        public List<Bots> StartedChatBots { get; private set; } = [];
        private bool ChatBotStopping;

        private GiveawayTypes GiveawayItemType = GiveawayTypes.None;
        private string GiveawayItemName = "";
        private bool GiveawayStarted = false;

        internal static Collection<IBotTypes> BotsList { get; private set; } = [];
        private BotsTwitch TwitchBots { get; set; }
        public static BotOverlayServer OverlayServerBot { get; set; } = new();

        private const int SendMsgDelay = 750;
        // 600ms between messages, permits about 100 messages max in 60 seconds == 1 minute
        // 759ms between messages, permits about 80 messages max in 60 seconds == 1 minute
        private Queue<Task> Operations { get; set; } = [];   // an ordered list, enqueue into one end, dequeue from other end
        private Thread SendThread;  // the thread for sending messages back to the monitored channels

        #region Construction
        /// <summary>
        /// The BotController is the central hub for the bots to interact with the system. It receives events from the bots, and then invokes the appropriate methods in the system to handle those events. It also receives requests from the system to send messages to the bots, and then sends those messages to the appropriate bots. The BotController also manages the lifecycle of the bots, such as initializing them, starting them, and stopping them. The BotController also manages the database interactions for the bots, such as clearing data, updating data, and retrieving data. The BotController also manages the Twitch API interactions for the bots, such as getting user information, getting stream information, and modifying channel information.
        /// </summary>
        /// <param name="OnLoadCompletedHandler">The handler for when the load is completed.</param>
        public BotController(EventHandler<EventArgs> OnLoadCompletedHandler)
        {
            SetupDataBot(OnLoadCompletedHandler);

            SetupTwitchBots();

            OutputSentToBots += DataBot.GetPostChannelMessageHandler();

            ThreadManager.CreateThreadStart(".ctor_BotController", () =>
            {
                TwitchBots.InitializeGetIds(
                  (liveUser) =>
                  {
                      string result = null;
                      using (var waitHandle = new ManualResetEventSlim())
                      {
                          DataBot.GetUserId(liveUser, id =>
                          {
                              result = id;
                              waitHandle.Set();
                          });
                          waitHandle.Wait();
                      }
                      return result;
                  }
                  );

                DataBot.InitializeLiveMonitorUpdateChannels(
                    TwitchBots.InitializeLiveMonitor(
                       (platform) =>
                       {
                           IEnumerable<string> result = null;
                           using (var waitHandle = new ManualResetEventSlim())
                           {
                               DataBot.GetMonitorChannels(platform, ids =>
                               {
                                   result = ids;
                                   waitHandle.Set();
                               });
                               waitHandle.Wait();
                           }
                           return result ?? [];
                       }
                       )
                    );
            });

            SetNewOverlayEventHandler();

            BotsList.Add(TwitchBots);
            BotsList.Add(OverlayServerBot);


            SetBotOperations();
        }

        public void SetupDataBot(EventHandler<EventArgs> OnLoadCompletedHandler)
        {
            DataBot = new();
            DataBot.SetLoadCompletedHandler(OnLoadCompletedHandler);
            DataBot.Initialize();

            DataBot.InitializeBotControllerHandlers(
                PostChannelMessage: Systems_PostChannelMessage,
                BanUserRequest: Systems_BanUserRequest,
                TwitchShoutOutUser: Systems_TwitchShoutOutUser);

            DataBot.SetGetClipsHandler(HandleGetUserClips);
        }

        public void SetupTwitchBots()
        {
            TwitchBots = new();
            TwitchBots.BotEvent += HandleBotEvent;
            BotsTwitch.StaticBotEvent += HandleStaticBotEvent;

            TwitchBots.NotifyAdSoon += TwitchBots_NotifyAdSoon;
            TwitchBots.NotifyAdStarted += TwitchBots_NotifyAdStarted;
            TwitchBots.NotifyAdEnded += TwitchBots_NotifyAdEnded;
            TwitchBots.InvalidTwitchAccess += TwitchBots_InvalidTwitchAccess;
            TwitchBots.OnTwitchTokensInitialized += TwitchBots_OnTwitchTokensInitialized;
        }


        /// <summary>
        /// Connect the Overlay System event notification to the Overlay Server bot to process any new Overlay actions detected.
        /// </summary>
        private void SetNewOverlayEventHandler()
        {
            DataBot.SetNewOverlayEventHandler(
                OverlayServerBot.NewOverlayEventHandler,
                OverlayServerBot.UpdatedTickerEventHandler
                );
        }

        private void SetBotOperations()
        {
            BotOperationStatus.Clear();
            BotOperationStatus.Add(new BotOperationTwitch(TwitchBots, Platform.Twitch));
        }

        #endregion

        public IEnumerable<string> PostIds(IEnumerable<string> Ids)
        {
            return Ids;
        }

        /// <summary>
        /// Set the event handler for when the overlay server receives a new event to post to the overlay, which then posts the event to the system to handle and send to the overlay.
        /// </summary>
        /// <param name="eventHandler">The event handler for data collection updates.</param>
        public void HandleOnDataCollectionUpdated(EventHandler<OnDataCollectionUpdatedEventArgs> eventHandler)
        {
            LogWriter.DebugLog("HandleOnDataCollectionUpdated", DebugLogTypes.BotController, "Received a request to handle data collection updates.");
            DataBot.InitializeDataManagerCollectionUpdateEvent(eventHandler);
        }

        private void TwitchBots_OnTwitchTokensInitialized(object sender, EventArgs e)
        {
            TokensInitialized?.Invoke(this, new());
            GetUserCategory();
        }

        /// <summary>
        /// Notify when authorized bots fail and access/refresh tokens are now invalid and can't be renewed, or the user provided tokens have expired and are no longer valid for processing Twitch API calls. This is a static method that can be called from anywhere in the application to notify the system of invalid Twitch tokens, which then invokes the InvalidAuthorizationToken event to handle the invalid tokens and prompt the user to re-authorize the application with Twitch.
        /// </summary>
        public static async void NotifyInvalidTwitchTokens()
        {
            await BotsTwitch.NotifyInvalidTwitchTokens();
        }

        /// <summary>
        /// Notify when authorized bots fail and access/refresh tokens are now invalid and can't be renewed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchBots_InvalidTwitchAccess(object sender, InvalidAccessTokenEventArgs e)
        {
            LogWriter.DebugLog("TwitchBots_InvalidTwitchAccess", DebugLogTypes.BotController, "");

            InvalidAuthorizationToken?.Invoke(this, e);
        }

        /// <summary>
        /// Receives a bundled event from the bots, which is unpackaged and now runs on the GUI thread dispatcher.
        /// </summary>
        /// <param name="sender">Unused.</param>
        /// <param name="e">The parameters to include the method name to invoke, and the event arguments for the invoked method.</param>
        private void HandleBotEvent(object sender, BotEventArgs e)
        {
            //AppDispatcher.BeginInvoke(() =>
            ThreadManager.CreateThreadStart("HandleBotEvent", () =>
            {
                LogWriter.DebugLog("HandleBotEvent", DebugLogTypes.BotController, $"Event, {e.MethodName}, received from bots to post into system.");

                try
                {
                    //_ = typeof(BotController).InvokeMember(
                    //        name: e.MethodName,
                    //        invokeAttr: BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.OptionalParamBinding,
                    //        binder: null,
                    //        target: this,
                    //        args: e.e == null ? null : [e.e],
                    //        culture: null);

                    switch (e.MethodName)
                    {
                        case BotEvents.TwitchBotEventSubStarted:
                            TwitchBotEventSubStarted(e.e);
                            break;
                        case BotEvents.TwitchBotEventSubStopping:
                            TwitchBotEventSubStopping(e.e);
                            break;
                        case BotEvents.TwitchBotEventSubStopped:
                            TwitchBotEventSubStopped(e.e);
                            break;
                        case BotEvents.TwitchBeingHosted:
                            break;
                        case BotEvents.TwitchBulkPostFollowers:
                            TwitchBulkPostFollowers((OnNewFollowersDetectedArgs)e.e);
                            break;
                        case BotEvents.TwitchStartBulkFollowers:
                            TwitchStartBulkFollowers();
                            break;
                        case BotEvents.TwitchStopBulkFollowers:
                            TwitchStopBulkFollowers();
                            break;
                        case BotEvents.TwitchCommunitySubscription:
                            TwitchCommunitySubscription((NewChannelSubscriptionGiftEventArgs)e.e);
                            break;
                        case BotEvents.TwitchGiftSubscription:
                            TwitchGiftSubscription((NewChannelSubscribeEventArgs)e.e);
                            break;
                        case BotEvents.TwitchNewSubscriber:
                            TwitchNewSubscriber((NewChannelSubscribeEventArgs)e.e);
                            break;
                        case BotEvents.TwitchPostNewClip:
                            TwitchPostNewClip((OnNewClipsDetectedArgs)e.e);
                            break;
                        case BotEvents.TwitchClipSvcOnClipFound:
                            TwitchClipSvcOnClipFound((ClipFoundEventArgs)e.e);
                            break;
                        case BotEvents.TwitchPostNewFollowers:
                            TwitchPostNewFollowers((NewChannelFollowEventArgs)e.e);
                            break;
                        case BotEvents.TwitchReSubscriber:
                            TwitchReSubscriber((NewChannelSubscriptionMessageEventArgs)e.e);
                            break;
                        case BotEvents.TwitchStreamOffline:
                            TwitchStreamOffline((NewStreamOfflineEventArgs)e.e);
                            break;
                        case BotEvents.TwitchMultiStreamOnline:
                            TwitchMultiStreamOnline((OnStreamOnlineArgs)e.e);
                            break;
                        case BotEvents.TwitchMultiGetChannels:
                            break;
                        case BotEvents.TwitchStreamOnline:
                            TwitchStreamOnline((NewStreamOnlineEventArgs)e.e);
                            break;
                        case BotEvents.TwitchResumeStreamOnline:
                            TwitchResumeStreamOnline((ResumeStreamOnlineEventArgs)e.e);
                            break;
                        case BotEvents.TwitchStreamUpdate:
                            TwitchStreamUpdate((NewChannelUpdateEventArgs)e.e);
                            break;
                        case BotEvents.TwitchCategoryUpdate:
                            TwitchCategoryUpdate((FindChannelCategoryEventArgs)e.e);
                            break;
                        case BotEvents.TwitchFoundViewerCategory:
                            TwitchFoundViewerCategory((FindChannelCategoryEventArgs)e.e);
                            break;
                        case BotEvents.TwitchNowHosting:
                            break;
                        case BotEvents.TwitchOnUserLeft:
                            TwitchOnUserLeft((StreamerOnUserLeftArgs)e.e);
                            break;
                        //case BotEvents.TwitchOnUserTimedout:
                        //    TwitchOnUserTimedout((OnUserTimedoutArgs)e.e);
                        //    break;
                        //case BotEvents.TwitchOnUserBanned:
                        //    TwitchOnUserBanned((OnUserBannedArgs)e.e);
                        //    break;
                        case BotEvents.TwitchRitualNewChatter:
                            break;
                        case BotEvents.TwitchMessageReceived:
                            TwitchMessageReceived((ChannelChatMessageEventArgs)e.e);
                            break;
                        case BotEvents.TwitchIncomingRaid:
                            TwitchIncomingRaid((OnIncomingRaidArgs)e.e);
                            break;
                        case BotEvents.TwitchChatCommandReceived:
                            TwitchChatCommandReceived((ChannelChatMessageEventArgs)e.e);
                            break;
                        case BotEvents.TwitchChannelPointsRewardRedeemed:
                            TwitchChannelPointsRewardRedeemed((NewChannelCustomRewardRedemptionEventArgs)e.e);
                            break;
                        case BotEvents.TwitchOutgoingRaid:
                            TwitchOutgoingRaid((OnStreamRaidResponseEventArgs)e.e);
                            break;
                        case BotEvents.TwitchBotCommandCall:
                            TwitchBotCommandCall((SendBotCommandEventArgs)e.e);
                            break;
                        case BotEvents.TwitchCurrentUsers:
                            TwitchCurrentUsers((StreamerOnExistingUserDetectedArgs)e.e);
                            break;
                        case BotEvents.HandleBotEventEmpty:
                            break;
                    }

                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "HandleBotEvent");
                }
            });
        }

        private void HandleStaticBotEvent(object sender, BotEventArgs e)
        {
            ThreadManager.CreateThreadStart("HandleStaticBotEvent", () =>
            {
                LogWriter.DebugLog("HandleStaticBotEvent", DebugLogTypes.BotController, $"Event, {e.MethodName}, received from bots to post into system.");

                try
                {
                    //_ = typeof(BotController).InvokeMember(
                    //        name: e.MethodName,
                    //        invokeAttr: BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.OptionalParamBinding,
                    //        binder: null,
                    //        target: this,
                    //        args: e.e == null ? null : [e.e],
                    //        culture: null);

                    switch (e.MethodName)
                    {
                        case BotEvents.TwitchBotEventSubStarted:
                            TwitchBotEventSubStarted(e.e);
                            break;
                        case BotEvents.TwitchBotEventSubStopping:
                            TwitchBotEventSubStopping(e.e);
                            break;
                        case BotEvents.TwitchBotEventSubStopped:
                            TwitchBotEventSubStopped(e.e);
                            break;
                        case BotEvents.TwitchBeingHosted:
                            break;
                        case BotEvents.TwitchBulkPostFollowers:
                            TwitchBulkPostFollowers((OnNewFollowersDetectedArgs)e.e);
                            break;
                        case BotEvents.TwitchStartBulkFollowers:
                            TwitchStartBulkFollowers();
                            break;
                        case BotEvents.TwitchStopBulkFollowers:
                            TwitchStopBulkFollowers();
                            break;
                        case BotEvents.TwitchCommunitySubscription:
                            TwitchCommunitySubscription((NewChannelSubscriptionGiftEventArgs)e.e);
                            break;
                        case BotEvents.TwitchGiftSubscription:
                            TwitchGiftSubscription((NewChannelSubscribeEventArgs)e.e);
                            break;
                        case BotEvents.TwitchNewSubscriber:
                            TwitchNewSubscriber((NewChannelSubscribeEventArgs)e.e);
                            break;
                        case BotEvents.TwitchPostNewClip:
                            TwitchPostNewClip((OnNewClipsDetectedArgs)e.e);
                            break;
                        case BotEvents.TwitchClipSvcOnClipFound:
                            TwitchClipSvcOnClipFound((ClipFoundEventArgs)e.e);
                            break;
                        case BotEvents.TwitchPostNewFollowers:
                            TwitchPostNewFollowers((NewChannelFollowEventArgs)e.e);
                            break;
                        case BotEvents.TwitchReSubscriber:
                            TwitchReSubscriber((NewChannelSubscriptionMessageEventArgs)e.e);
                            break;
                        case BotEvents.TwitchStreamOffline:
                            TwitchStreamOffline((NewStreamOfflineEventArgs)e.e);
                            break;
                        case BotEvents.TwitchMultiStreamOnline:
                            TwitchMultiStreamOnline((OnStreamOnlineArgs)e.e);
                            break;
                        case BotEvents.TwitchMultiGetChannels:
                            break;
                        case BotEvents.TwitchStreamOnline:
                            TwitchStreamOnline((NewStreamOnlineEventArgs)e.e);
                            break;
                        case BotEvents.TwitchResumeStreamOnline:
                            TwitchResumeStreamOnline((ResumeStreamOnlineEventArgs)e.e);
                            break;
                        case BotEvents.TwitchStreamUpdate:
                            TwitchStreamUpdate((NewChannelUpdateEventArgs)e.e);
                            break;
                        case BotEvents.TwitchCategoryUpdate:
                            TwitchCategoryUpdate((FindChannelCategoryEventArgs)e.e);
                            break;
                        case BotEvents.TwitchFoundViewerCategory:
                            TwitchFoundViewerCategory((FindChannelCategoryEventArgs)e.e);
                            break;
                        case BotEvents.TwitchNowHosting:
                            break;
                        case BotEvents.TwitchOnUserLeft:
                            TwitchOnUserLeft((StreamerOnUserLeftArgs)e.e);
                            break;
                        //case BotEvents.TwitchOnUserTimedout:
                        //    TwitchOnUserTimedout((OnUserTimedoutArgs)e.e);
                        //    break;
                        //case BotEvents.TwitchOnUserBanned:
                        //    TwitchOnUserBanned((OnUserBannedArgs)e.e);
                        //    break;
                        case BotEvents.TwitchRitualNewChatter:
                            break;
                        case BotEvents.TwitchMessageReceived:
                            TwitchMessageReceived((ChannelChatMessageEventArgs)e.e);
                            break;
                        case BotEvents.TwitchIncomingRaid:
                            TwitchIncomingRaid((OnIncomingRaidArgs)e.e);
                            break;
                        case BotEvents.TwitchChatCommandReceived:
                            TwitchChatCommandReceived((ChannelChatMessageEventArgs)e.e);
                            break;
                        case BotEvents.TwitchChannelPointsRewardRedeemed:
                            TwitchChannelPointsRewardRedeemed((NewChannelCustomRewardRedemptionEventArgs)e.e);
                            break;
                        case BotEvents.TwitchOutgoingRaid:
                            TwitchOutgoingRaid((OnStreamRaidResponseEventArgs)e.e);
                            break;
                        case BotEvents.TwitchBotCommandCall:
                            TwitchBotCommandCall((SendBotCommandEventArgs)e.e);
                            break;
                        case BotEvents.TwitchCurrentUsers:
                            TwitchCurrentUsers((StreamerOnExistingUserDetectedArgs)e.e);
                            break;
                        case BotEvents.HandleBotEventEmpty:
                            break;
                    }

                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "HandleBotEvent");
                }
            });

        }

        /// <summary>
        /// Captures send events from the systems object to send to every bot with a send method. Some bots don't have 'send' implemented, so the message only sends for bots implementing send.
        /// </summary>
        /// <param name="sender">Unused - object invoking the event.</param>
        /// <param name="e">Contains the message to send to the bots.</param>
        private void Systems_PostChannelMessage(object sender, PostChannelMessageEventArgs e)
        {
            LogWriter.DebugLog("Systems_PostChannelMessage", DebugLogTypes.BotController, $"Received message to post to chat: {e.Msg}");

            Send(e.Platform, e.Msg, e.Announcement, e.RepeatMsg);
        }

        /// <summary>
        /// Send a response message to all bots incorporated into this app. The messages send through a thread managing a message delay to not flood the channel with immediate messages, channels often have limited received messages per minute.
        /// </summary>
        /// <param name="s">The string to send.</param>
        public void Send(Platform platform, string s, bool Announcement = false, int Repeat = 0)
        {
            OutputSentToBots?.Invoke(this, new() { Msg = s });

            foreach (IBotTypes bot in BotsList)
            {
                lock (Operations)
                {
                    if (bot.Platform == platform || platform == Platform.Default)
                    {
                        for (int x = 0; x <= Repeat; x++)
                        {
                            Operations.Enqueue(new Task(() =>
                            {
                                bot.Send(s, Announcement);
                            }));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cycles through the 'Operations' queue and runs each task in order.
        /// </summary>
        private void BeginProcMsgs()
        {
            // TODO: set option to stop messages immediately, and wait until started again to send them
            // until the ProcessOps is false to stop operations, only run until the operations queue is empty
            while ((OptionFlags.ActiveToken || Operations.Count > 0) && StartedChatBots.Count > 0)
            {
                while (ChatBotStopping) { } // spin while a bot is stopping, to prevent sending any messages
                Task temp = null;
                lock (Operations)
                {
                    if (Operations.Count > 0)
                    {
                        temp = Operations.Dequeue(); // get a task from the queue
                    }
                }

                if (temp != null)
                {
                    temp.Start();   // begin, wait, and dispose the task; let it process in sequence before the next message
                    temp.Wait();
                    temp.Dispose();
                }

                Thread.Sleep(SendMsgDelay);
            }
        }

        /// <summary>
        /// Wait for all messages to send to bots. Invoke a StopBots() method for each bot, and prepare to stop the application.
        /// </summary>
        public void ExitBots()
        {
            try
            {
                LogWriter.DebugLog("ExitBots", DebugLogTypes.BotController, "User wants to exit the bot.");
                LogWriter.DebugLog("ExitBots", DebugLogTypes.BotController, "Sending a Twitch Stream Offline message.");

                TwitchStreamOffline(null);

                LogWriter.DebugLog("ExitBots", DebugLogTypes.BotController, "Waiting for any queued messages to finish sending to the channel.");

                SendThread?.Join(); // wait until all the messages are sent to ask bots to close

                LogWriter.DebugLog("ExitBots", DebugLogTypes.BotController, "Stopping bots.");

                foreach (IBotTypes bot in BotsList)
                {
                    bot.StopBots();
                }

                LogWriter.DebugLog("ExitBots", DebugLogTypes.BotController, "Sending an exit to the data system.");
                DataBot.Exit();
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "ExitBots");
            }
        }

        /// <summary>
        /// This method checks the user settings and will delete any DB data if the user unchecks the setting. 
        /// Other methods to manage users & followers will adapt to if the user adjusted the setting
        /// </summary>
        public void ManageDatabase()
        {
            DataBot.ManageDatabase();
            // TODO: add fixes if user re-enables 'managing { users || followers || stats }' to restart functions without restarting the bot

            // if ManageFollowers is False, then remove followers!, upstream code stops the follow bot
            //if (OptionFlags.ManageFollowers)
            //{
            //    foreach (IBotTypes bot in BotsList)
            //    {
            //        bot.GetAllFollowers();
            //    }
            //}
            // when management resumes, code upstream enables the startbot process 
        }

        #region I-O to Database

        /// <summary>
        /// Send a 'Clear Watch Time' to the system database.
        /// </summary>
        public void ClearWatchTime()
        {
            LogWriter.DebugLog("ClearWatchTime", DebugLogTypes.BotController, $"Received \"Clear Watch Time\" request.");

            DataBot.ClearWatchTime();
        }

        /// <summary>
        /// Send a 'clear all currency values' to the system database.
        /// </summary>
        public void ClearAllCurrenciesValues()
        {
            LogWriter.DebugLog("ClearAllCurrenciesValues", DebugLogTypes.BotController, "Received \"Clear All Currencies Values\" request.");

            DataBot.ClearAllCurrenciesValues();
        }

        /// <summary>
        /// Send a 'clear all users non followers' to the system database.
        /// </summary>
        public void ClearUsersNonFollowers()
        {
            LogWriter.DebugLog("ClearUsersNonFollowers", DebugLogTypes.BotController, "Received \"Clear Users Non Followers\" request.");

            DataBot.ClearUsersNonFollowers();
        }

        /// <summary>
        /// Send a "Set System Events Enabled" toggle request to the system database.
        /// </summary>
        /// <param name="Enabled">True or False to set System Events in bulk.</param>
        public void SetSystemEventsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetSystemEventsEnabled", DebugLogTypes.BotController, $"Received a \"Set System Events Enabled\" " +
                $"request to set all events to {Enabled}.");

            DataBot.SetSystemEventsEnabled(Enabled);
        }

        /// <summary>
        /// Send a "Set BuiltIn Commands Enabled" toggle request to the system database.
        /// </summary>
        /// <param name="Enabled">True or False to set built-in commands in bulk.</param>
        public void SetBuiltInCommandsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetBuiltInCommandsEnabled", DebugLogTypes.BotController, $"Received a \"Set Built-in " +
                $"Commands Enabled\" request set all events to {Enabled}.");

            DataBot.SetBuiltInCommandsEnabled(Enabled);
        }

        /// <summary>
        /// Send a "Set User Defined Commands Enabled" toggle request to the system database.
        /// </summary>
        /// <param name="Enabled">True or False to set user defined commands in bulk.</param>
        public void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetUserDefinedCommandsEnabled", DebugLogTypes.BotController, $"Received a \"Set User Defined " +
                $"Commands Enabled\" request set all events to {Enabled}.");

            DataBot.SetUserDefinedCommandsEnabled(Enabled);
        }

        /// <summary>
        /// Send a "Delete Data Rows" request to the system database, which includes the data rows to delete and the table name to delete from.
        /// </summary>
        /// <param name="dataRows">The data rows to delete.</param>
        /// <param name="TableName">The name of the table to delete from.</param>
        public void DeleteDataRows(IEnumerable<object> dataRows, string TableName)
        {
            DataBot.DeleteDataRows(dataRows, TableName);
        }

        /// <summary>
        /// Send a "Set WebHooks Webhooks Enabled" toggle request to the system database.
        /// </summary>
        /// <param name="Enabled">True or False to set WebHooks Webhooks in bulk.</param>
        public void SetDiscordWebhooksEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetDiscordWebhooksEnabled", DebugLogTypes.BotController, $"Received a \"Set WebHooks Webhooks Enabled\" " +
                $"request to set all events to {Enabled}.");

            DataBot.SetDiscordWebhooksEnabled(Enabled);
        }

        /// <summary>
        /// Send an "Update IsEnabled Rows" request to the system database, which includes the table name to update the IsEnabled value for all rows in that table based on the current settings for those rows.
        /// </summary>
        /// <param name="TableName">The name of the table to update.</param>
        public void UpdatedIsEnabledRows(string TableName)
        {
            DataBot.GUISaveDataGridEdits(false, TableName);
        }

        /// <summary>
        /// Send a "Save Data Grid Edits" request to the system database, which includes the table name to update the data for all rows in that table based on the current values in the GUI datagrid for that table. The CommandUpdate parameter specifies whether this update is coming from a command execution or from a GUI edit, which can be used to trigger different processes in the database update method if needed.
        /// </summary>
        /// <param name="CommandUpdate">Indicates whether the update is coming from a command execution or a GUI edit.</param>
        /// <param name="TableName">The name of the table to update.</param>
        public void GUISaveDataGridEdits(bool CommandUpdate, string TableName)
        {
            DataBot.GUISaveDataGridEdits(CommandUpdate, TableName);
        }

        /// <summary>
        /// Send an "Update Repeat Commands" request to the system database, which updates the repeat commands based on the current settings for the repeat commands in the database. This is used to update the repeat commands in the system after changes are made to the repeat command settings, such as enabling or disabling repeat commands, or changing the repeat command messages or intervals.
        /// </summary>
        public void UpdateRepeatCommands()
        {
            DataBot.UpdateRepeatCommands();
        }

        /// <summary>
        /// Insert a new AutoShoutUser entry into the database.
        /// </summary>
        /// <param name="UserName">The username to add into the database for the autoshout table.</param>
        public void AddNewAutoShoutUser(string Userid, Platform platform)
        {
            LogWriter.DebugLog("AddNewAutoShoutUser", DebugLogTypes.BotController, $"Received an \"Add New Auto Shout User\" " +
                $"request to add= {Userid} =to the database.");

            DataBot.AddNewAutoShoutUser(Userid, platform);
        }

        /// <summary>
        /// Add new monitor channels to the database, which are then used by the live monitor bot to monitor the specified channels for going live and posting stream updates. This method is used to add new channels into the monitoring list in the database, which then gets retrieved by the live monitor bot to know which channels to monitor for stream updates and going live events.
        /// </summary>
        /// <param name="monitorChannels">The list of monitor channels to add.</param>
        public void AddNewMonitorChannel(List<LiveUser> monitorChannels)
        {
            DataBot.AddNewMonitorChannel(monitorChannels);
        }

        /// <summary>
        /// Send a "Reset Category Stream Count" request to the system database, which resets the stream count for each category back to 0. This is used to reset the stream count for categories, which can be useful for tracking how many times a category has been streamed during a certain period, such as a month or a year, and then resetting the count at the end of that period to start fresh for the next period.
        /// </summary>
        public void ResetCategoryStreamCount()
        {
            DataBot.ResetCategoryStreamCount();
        }

        #endregion

        #region Query Bots

        /// <summary>
        /// Part of the Twitch-Auth-Code Token operation method.
        /// Call to clear out the Twitch Authorization Code(s) to permit the user to re-authorize the application.
        /// </summary>
        public static void ForceTwitchAuthReauthorization(params Bots[] bots)
        {
            LogWriter.DebugLog("ForceTwitchAuthReauthorization", DebugLogTypes.BotController, "Received request to invalidate Twitch Authorization Codes so user can re-authorize application.");
            LogWriter.DebugLog("ForceTwitchAuthReauthorization", DebugLogTypes.BotController, "It's okay, there's a button in the GUI for the user to click and perform this operation.");

            BotsTwitch.ForceTwitchReauthorization(bots);
        }

        /// <summary>
        /// Retrieve the Bot Account User Name.
        /// The <paramref name="Source"/> is a Platform enum to distinguish the different bot groups added into this application-meaning, currently supports
        /// the Twitch streaming platform, but the architecture permits adding a bot for a different platform to connect with the same database.
        /// </summary>
        /// <param name="Source">Specify which bot platform to retrieve the account name.</param>
        /// <returns>The username for the bot account.</returns>
        public static string GetBotName(Platform Source)
        {
            LogWriter.DebugLog("GetBotName", DebugLogTypes.BotController, "Received a request for the Bot username.");

            return Source switch
            {
                Platform.Twitch => OptionFlags.TwitchBotUserName,
                _ => "None",
            };
        }

        /// <summary>
        /// Requests the user stream category for the current streamer setup within the Twitch bot.
        /// </summary>
        public static void GetUserCategory()
        {
            GetUserCategory(OptionFlags.TwitchChannelName, OptionFlags.TwitchStreamerUserId, Platform.Twitch);
        }

        /// <summary>
        /// A request to query the bot specified in <paramref name="bots"/> platform to find the current stream category for the provided channel.
        /// </summary>
        /// <param name="ChannelName">The name of the channel to query.</param>
        /// <param name="UserId">The user Id value to query.</param>
        /// <param name="bots">The platform to query-currently only for Twitch, but may include other bots in the future.</param>
        /// <returns>The category retrieved from the bot query about a certain channel/user Id.</returns>
        public static string GetUserCategory(string ChannelName, string UserId, Platform bots)
        {
            if (bots == Platform.Twitch)
            {
                LogWriter.DebugLog("GetUserCategory", DebugLogTypes.BotController, $"Received request to provide the " +
                    $"streaming category for the channel named: {ChannelName}, with {UserId} userId.");

                CategoryData categoryData = BotsTwitch.GetUserCategory(UserId: UserId, UserName: ChannelName);

                return categoryData.CategoryName;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// A request to query the bot specified in <paramref name="bots"/> platform to find the account age for the provided username.
        /// </summary>
        /// <param name="UserName">The username for which to retrieve the account age.</param>
        /// <param name="bots">The platform to query.</param>
        /// <returns>The account age for the specified user.</returns>
        public static DateTime GetUserAccountAge(string UserName, Platform bots)
        {
            if (bots == Platform.Twitch)
            {
                LogWriter.DebugLog("GetUserAccountAge", DebugLogTypes.BotController, $"Received request " +
                    $"to ask Twitch for {UserName}'s account age.");

                return BotsTwitch.GetUserAccountAge(UserName: UserName);
            }
            else
            {
                return DateTime.MaxValue;
            }
        }

        /// <summary>
        /// A request to query the bot specified in <paramref name="bots"/> platform to verify if a user exists with the provided channel name. This is used to validate if a channel name provided in the settings corresponds to an actual user on the platform, which can help catch typos or incorrect channel names before trying to perform operations with that channel name. Currently implemented for Twitch, but can be expanded for other platforms as needed.
        /// </summary>
        /// <param name="ChannelName">The name of the channel to verify.</param>
        /// <param name="bots">The platform to query.</param>
        /// <returns>True if the user exists, false otherwise.</returns>
        public static bool VerifyUserExist(string ChannelName, Platform bots)
        {
            if (bots == Platform.Twitch)
            {
                LogWriter.DebugLog("VerifyUserExist", DebugLogTypes.BotController, $"Received request " +
                    $"to ask Twitch to verify if the user {ChannelName} exists.");

                return BotsTwitch.VerifyUserExist(ChannelName);
            }
            else
            {
                return false;
            }
            //return bots switch
            //{
            //    Bots.TwitchChatBot or Bots.TwitchUserBot => ,
            //    Bots.Default => throw new NotImplementedException(),
            //    Bots.TwitchLiveBot => throw new NotImplementedException(),
            //    Bots.TwitchFollowBot => throw new NotImplementedException(),
            //    Bots.TwitchClipBot => throw new NotImplementedException(),
            //    Bots.TwitchMultiBot => throw new NotImplementedException(),
            //    Bots.TwitchPubSub => throw new NotImplementedException(),
            //    _ => throw new NotImplementedException()
            //};
        }

        private void Systems_TwitchShoutOutUser(object sender, TwitchShoutOutUsersEventArgs e)
        {
            if (e.User.Platform == Platform.Twitch)
            {
                TwitchBots.SendShoutOut(e.User);
            }
        }

        /// <summary>
        /// A request to modify the Twitch channel information, which can include the stream title and the stream category. This is used to update the Twitch channel information based on changes made in the system, such as changing the stream title or category from the GUI or from a command, and then sending that update to Twitch to change the channel information accordingly. The Title, CategoryName, and CategoryId parameters are optional, so you can choose to update just one of those values or all of them at once depending on what information you want to change for the Twitch channel.
        /// </summary>
        /// <param name="bots">The platform to query.</param>
        /// <param name="Title">The new stream title.</param>
        /// <param name="CategoryName">The new stream category name.</param>
        /// <param name="CategoryId">The new stream category ID.</param>
        /// <returns></returns>
        public static bool ModifyChannelInformation(Platform bots, string Title = null, string CategoryName = null, string CategoryId = null)
        {
            bool result = false;

            if (bots == Platform.Twitch)
            {
                LogWriter.DebugLog("ModifyChannelInformation", DebugLogTypes.BotController, $"Received request to " +
                    $"change the Twitch channel information to Title: {Title}, Category: {CategoryName}.");
                LogWriter.DebugLog("ModifyChannelInformation", DebugLogTypes.BotController, "One of these values can be null, because there are " +
                    "separate !settitle and !setcategory commands, where these separate values can come through this class method.");

                result = BotsTwitch.ModifyChannelInformation(Title, CategoryName, CategoryId);
            }

            return result;
        }

        /// <summary>
        /// A request to ask Twitch to raid another channel, which includes the channel name to raid and the platform to perform the raid on. This is used to send a raid from the streamer's channel to another channel, which can help grow the other channel and provide a fun interaction for viewers. Currently implemented for Twitch, but can be expanded for other platforms that support raiding or similar features as needed.
        /// </summary>
        /// <param name="ToChannelName">The name of the channel to raid.</param>
        /// <param name="bots">The platform to perform the raid on.</param>
        public static void RaidChannel(string ToChannelName, Platform bots)
        {
            if (bots == Platform.Twitch)
            {
                LogWriter.DebugLog("RaidChannel", DebugLogTypes.BotController, $"Received a request " +
                    $"to raid the: {ToChannelName}, Twitch channel.");

                BotsTwitch.RaidChannel(ToChannelName);
            }
        }

        /// <summary>
        /// A request to ask Twitch to cancel any pending outgoing raid, which is used to cancel a raid that was sent but has not yet been accepted by the target channel. This can be useful if the streamer changes their mind about raiding or if they realize they made a mistake in the target channel name and want to cancel the raid before it goes through. Currently implemented for Twitch, but can be expanded for other platforms that support raiding or similar features as needed.
        /// </summary>
        /// <param name="bots">The platform to perform the cancellation on.</param>
        public static void CancelRaidChannel(Platform bots)
        {
            if (bots == Platform.Twitch)
            {
                LogWriter.DebugLog("CancelRaidChannel", DebugLogTypes.BotController, $"Received a request to " +
                    $"cancel the pending Twitch channel raid.");

                BotsTwitch.CancelRaidChannel();
            }
        }

        /// <summary>
        /// A request to ask Twitch for the current viewer count of the stream, which can be used to display the viewer count in the GUI or to use it for other purposes such as triggering certain events when the viewer count reaches a certain threshold. This method checks if the stream is currently online before trying to get the viewer count, since Twitch does not provide a viewer count for offline streams. Currently implemented for Twitch, but can be expanded for other platforms that support retrieving viewer counts as needed.
        /// </summary>
        /// <param name="bots">The platform to retrieve the viewer count from.</param>
        public static void GetViewerCount(Platform bots)
        {
            if (OptionFlags.IsStreamOnline)
            {
                LogWriter.DebugLog("GetViewerCount", DebugLogTypes.BotController, "Found the stream is online.");

                ThreadManager.CreateThreadStart("GetViewerCount", () =>
                {
                    if (bots == Platform.Twitch || bots == Platform.Default)
                    {
                        LogWriter.DebugLog("GetViewerCount", DebugLogTypes.BotController, "Received a request to get the " +
                            "current viewer count for the Twitch streamer channel.");

                        BotsTwitch.GetViewerCount();
                    }
                });
            }
        }

        /// <summary>
        /// Interface method to request Twitch to provide an access/refresh token with the newly obtained authentication code.
        /// </summary>
        /// <param name="clientId">The client Id for the authentication code we need to activate.</param>
        /// <param name="NoScopes">True specifies the current authorization is for the no-scopes access token credential.</param>
        /// <param name="OpenBrowser">A callback method to open the browser for authentication.</param>
        /// <param name="AuthenticationFinished">A callback method once the bot concludes using the auth code to get an access/refresh token.</param>
        public static void TwitchTokenAuthCodeAuthorize(string clientId, bool NoScopes, Action<string> OpenBrowser, Action AuthenticationFinished)
        {
            LogWriter.DebugLog("TwitchTokenAuthCodeAuthorize", DebugLogTypes.BotController, "Received request to activate " +
                "a Twitch authorization code for a specific client Id, which returns an initial access token and a refresh token to begin accessing Twitch.");

            BotsTwitch.TwitchActivateAuthCode(clientId, NoScopes, OpenBrowser, AuthenticationFinished);
        }

        /// <summary>
        /// Interface method to ask the bot(s) to create a clip; which asks Twitch to create a clip.
        /// </summary>
        public static void CreateClip()
        {
            LogWriter.DebugLog("CreateClip", DebugLogTypes.BotController, "Recieved a request to create a Twitch clip.");

            BotsTwitch.CreateClip();
        }

        /// <summary>
        /// Interface method to ask TwitchFollowBot to query Twitch for the user Id of a specified username, which then posts the user Id back into the system through the TwitchPostMultiChannelUserId event method. This is used to get the user Id for a Twitch username, which is often needed for various Twitch API calls that require the user Id instead of the username. The TwitchFollowBot handles this query and posts the result back to the system, which can then use the user Id for other operations as needed.
        /// </summary>
        /// <param name="UserName">The username to query for.</param>
        /// <returns>The user Id for the specified username.</returns>
        public static string GetMultiChannelUserId(string UserName)
        {
            return BotsTwitch.GetUserId(UserName);
        }

        public void SendSchedule(ScheduleConfig data)
        {
            ThreadManager.CreateThreadStart("SendSchedule", () =>
            {
                DataBot.GetGameCategory(data.CategoryName, (category) =>
                {
                    LogWriter.DebugLog("SendSchedule", DebugLogTypes.BotController, $"Received a request to send a schedule to the bot for platform: {data.Platform}.");
                    switch (data.Platform)
                    {
                        case SchedulePlatform.Twitch:
                            TwitchBots.SendSchedule(data, category);
                            break;
                    }
                });
            });
        }

        public static object CreateStreamMarker(string Description, Platform platform)
        {
            switch (platform)
            {
                case Platform.Twitch:
                    return BotsTwitch.CreateStreamMarker(Description);

            }
            return null;
        }

        #endregion


        #region Debug

        /// <summary>
        /// Primarily for debug purposes: Gets the list of game categories from the data bot, which may be used for various purposes such as populating category selection options in the stream overlay, managing category-related functionality in the data bot, or for debugging and testing purposes to ensure that the Twitch bot is correctly retrieving and providing category information. This method is specifically for retrieving the game categories from the data bot and may involve asynchronous operations to wait for the data to be returned before proceeding with any further actions that depend on the category information.
        /// </summary>
        /// <returns>A list of game categories.</returns>
        public IEnumerable<CategoryData> GetGameCategories()
        {
            IEnumerable<CategoryData> categories = null;
            using (var waitHandle = new ManualResetEventSlim())
            {
                DataBot.GetGameCategories((result) =>
                {
                    categories = result;
                    waitHandle.Set();
                });
                waitHandle.Wait();
            }
            return categories;
        }

#if DEBUG

        /// <summary>
        /// Primarily for debug purposes: Adds test users to the data bot, which may be used for testing and debugging functionalities related to user management, such as simulating user join/leave events, testing chat command processing with different user profiles, or for any other scenarios where having test users in the data bot is beneficial for development and debugging. This method is specifically for adding predefined test users to the data bot and may be called during development or testing phases to set up the necessary user data for various test cases.
        /// </summary>
        public void TestAddUsers()
        {
            DataBot.TestAddUsers();
        }

        /// <summary>
        /// Primarily for debug purposes: Adds new multi-live data to the data bot, which may be used for testing and debugging functionalities related to multi-live stream management, such as simulating multiple live streams, testing category management for different streams, or for any other scenarios where having test multi-live data in the data bot is beneficial for development and debugging. This method is specifically for adding predefined multi-live data to the data bot and may be called during development or testing phases to set up the necessary multi-live stream data for various test cases.
        /// </summary>
        public void DebugAddNewMultiLiveData()
        {
            DataBot.DebugAddNewMultiLiveData();
        }
#endif
        #endregion
    }
}
