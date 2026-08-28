using StreamerBotLib.BotClients;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;
using StreamerBotLib.Systems.Overlay.Enums;

using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;

using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

// TODO: Add Bot contacts users to invoke conversation; carry-on conversation with existing

namespace StreamerBotLib.BotIOController
{
    /// <summary>
    /// The BotController is the central hub for the bots to interact with the system. It receives events from the bots, and then invokes the appropriate methods in the system to handle those events. It also receives requests from the system to send messages to the bots, and then sends those messages to the appropriate bots. The BotController also manages the lifecycle of the bots, such as initializing them, starting them, and stopping them. The BotController also manages the database interactions for the bots, such as clearing data, updating data, and retrieving data. The BotController also manages the Twitch API interactions for the bots, such as getting user information, getting stream information, and modifying channel information.
    /// </summary>
    public class BotController
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

        /// <summary>
        /// The BotController is the central hub for the bots to interact with the system. It receives events from the bots, and then invokes the appropriate methods in the system to handle those events. It also receives requests from the system to send messages to the bots, and then sends those messages to the appropriate bots. The BotController also manages the lifecycle of the bots, such as initializing them, starting them, and stopping them. The BotController also manages the database interactions for the bots, such as clearing data, updating data, and retrieving data. The BotController also manages the Twitch API interactions for the bots, such as getting user information, getting stream information, and modifying channel information.
        /// </summary>
        /// <param name="OnLoadCompletedHandler">The handler for when the load is completed.</param>
        public BotController(EventHandler<EventArgs> OnLoadCompletedHandler)
        {
            DataBot = new();
            DataBot.SetLoadCompletedHandler(OnLoadCompletedHandler);
            DataBot.Initialize();

            DataBot.InitializeBotControllerHandlers(
                PostChannelMessage: Systems_PostChannelMessage,
                BanUserRequest: Systems_BanUserRequest,
                TwitchShoutOutUser: Systems_TwitchShoutOutUser);

            DataBot.SetGetClipsHandler(HandleGetUserClips);

            TwitchBots = new();
            TwitchBots.BotEvent += HandleBotEvent;

            TwitchBots.NotifyAdSoon += TwitchBots_NotifyAdSoon;
            TwitchBots.NotifyAdStarted += TwitchBots_NotifyAdStarted;
            TwitchBots.NotifyAdEnded += TwitchBots_NotifyAdEnded;

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

            TwitchBots.InvalidTwitchAccess += TwitchBots_InvalidTwitchAccess;
            TwitchBots.OnTwitchTokensInitialized += TwitchBots_OnTwitchTokensInitialized;

        }

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
        /// Initializes a Helix api.
        /// </summary>
        public static async Task TwitchInitializeHelix()
        {
            await BotsTwitch.InitializeHelix();
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
        /// Request the database to provide the overlay actions for each overlay type, which then sends the data back through the provided callback method to handle the data once retrieved. This is used to get the current overlay actions from the database to display in the GUI or to use in the overlay server for posting events to the overlay based on the configured actions for each event type.
        /// </summary>
        /// <param name="callback">The callback method to handle the retrieved overlay actions.</param>
        public void GetOverlayActions(Action<Dictionary<string, List<string>>> callback)
        {
            DataBot.GetOverlayActions(callback);
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

        #endregion

        #region Twitch Bot Events

        /// <summary>
        /// Interface method to ask TwitchFollowBot to query Twitch for the followers of the channel, which then posts the followers back into the system through the TwitchPostNewFollowers event method.
        /// </summary>
        public void TwitchStartUpdateAllFollowers()
        {
            TwitchBots.GetAllFollowers();
        }

        internal void TwitchPostNewFollowers(NewChannelFollowEventArgs Follower)
        {
            HandleBotEventNewFollowers(ConvertFollowers(Follower.Channel, Platform.Twitch));
        }

        /// <summary>
        /// Method to indicate the end of a bulk followers update operation, which is used to trigger any processes waiting for the bulk update to finish, such as comparing the new followers list to the old followers list to find new followers and lost followers.
        /// </summary>
        public void TwitchStopBulkFollowers()
        {
            HandleBotEventStopBulkFollowers();
        }

        /// <summary>
        /// Convert from Twitch Follower objects to generic "Models.Follow" objects.
        /// </summary>
        /// <param name="follows">The Twitch follows list to convert.</param>
        /// <returns>The follower list converted to the generic "Models.Follow" list.</returns>
        private static Models.Follow ConvertFollowers(ChannelFollow follows, Platform Source)
        {
            return new Models.Follow(

                follows.FollowedAt.DateTime.ToLocalTime(),
                follows.UserId,
                follows.UserName,
                Source,
                null // gets re-assigned in SystemController where Category is tracked
            );
        }

        /// <summary>
        /// Convert from Twitch Follower objects to generic "Models.Follow" objects.
        /// </summary>
        /// <param name="follows">The Twitch follows list to convert.</param>
        /// <returns>The follower list converted to the generic "Models.Follow" list.</returns>
        private static List<Models.Follow> ConvertFollowers(List<ChannelFollower> follows, Platform Source)
        {
            return follows.ConvertAll((f) =>
            {
                return new Models.Follow(

                    DateTime.Parse(f.FollowedAt).ToLocalTime(),
                    f.UserId,
                    f.UserName,
                    Source,
                    null // gets re-assigned in SystemController where Category is tracked
                );
            });
        }

        /// <summary>
        /// Interface method to indicate the Twitch EventSub bot has started, which can be used to trigger any processes waiting for the bot to start before performing certain operations that require the bot to be running, such as subscribing to Twitch events or starting to monitor Twitch channels for updates.
        /// </summary>
        /// <param name="args">The event arguments for the bot start event.</param>
        public void TwitchBotEventSubStarted(EventArgs args = null)
        {
            HandleChatBotStarted(Bots.TwitchEventSubBot, args);
        }

        /// <summary>
        /// Interface method to indicate the Twitch EventSub bot is stopping, which can be used to trigger any processes that need to wait for the bot to stop before performing certain operations, such as cleaning up resources or preparing to exit the application.
        /// </summary>
        /// <param name="args">The event arguments for the bot stopping event.</param>
        public void TwitchBotEventSubStopping(EventArgs args = null)
        {
            HandleChatBotStopping(Bots.TwitchEventSubBot, args);
        }

        /// <summary>
        /// Interface method to indicate the Twitch EventSub bot has stopped, which can be used to trigger any processes that need to wait for the bot to stop before performing certain operations, such as cleaning up resources or preparing to exit the application.
        /// </summary>
        /// <param name="args">The event arguments for the bot stopped event.</param>
        public void TwitchBotEventSubStopped(EventArgs args = null)
        {
            HandleChatBotStopped(Bots.TwitchEventSubBot, args);
        }

        /// <summary>
        /// Interface method to indicate the Twitch Follow bot has started and began bulk follower updates, which can be used to trigger any processes waiting for the bot to start before performing certain operations that require the bot to be running, such as subscribing to Twitch events or starting to monitor Twitch channels for updates.
        /// </summary>
        public void TwitchStartBulkFollowers()
        {
            HandleBotEventStartBulkFollowers();
        }

        /// <summary>
        /// Interface method to post a bulk list of followers from Twitch to the system, which is used to update the system with the current list of followers from Twitch after a bulk update operation. This can be used to compare the new followers list to the old followers list to find new followers and lost followers, and then trigger any events or notifications based on those changes in the followers list.
        /// </summary>
        /// <param name="Follower">The event arguments for the new followers detected event.</param>
        public void TwitchBulkPostFollowers(OnNewFollowersDetectedArgs Follower)
        {
            HandleBotEventBulkPostFollowers(ConvertFollowers(Follower.NewFollowers, Platform.Twitch));
        }

        /// <summary>
        /// Interface method to post new clips from Twitch to the system, which is used to update the system with new clips that have been created on Twitch. This can be used to trigger any events or notifications based on new clips being created, such as posting a message in the channel about the new clip or sending a notification to a webhook about the new clip. The ClipFoundEventArgs includes a list of clips and a flag indicating whether all clips were retrieved, which can be used to determine how to process the new clips in the system.
        /// </summary>
        /// <param name="clips">The event arguments for the clip found event.</param>
        public void TwitchClipSvcOnClipFound(ClipFoundEventArgs clips)
        {
            HandleBotEventPostNewClip(clips.AllClips, ConvertClips(clips.ClipList));
        }

        /// <summary>
        /// Convert from Twitch Clip objects to generic "Models.Clip" objects.
        /// </summary>
        /// <param name="clips">The Twitch Clip objects to convert.</param>
        /// <returns>A list of generic "Models.Clip" objects.</returns>
        public static List<Models.Clip> ConvertClips(List<TwitchLib.Api.Helix.Models.Clips.GetClips.Clip> clips)
        {
            return clips.ConvertAll((SrcClip) =>
            {
                return new Models.Clip()
                {
                    ClipId = SrcClip.Id,
                    CreatedAt = DateTime.Parse(SrcClip.CreatedAt).ToLocalTime(),
                    Duration = SrcClip.Duration,
                    GameId = SrcClip.GameId,
                    Language = SrcClip.Language,
                    Title = SrcClip.Title,
                    Url = SrcClip.Url,
                    EmbedUrl = SrcClip.EmbedUrl,
                    FromUserId = SrcClip.CreatorId,
                    FromUserName = SrcClip.CreatorName
                };
            });
        }

        /// <summary>
        /// Interface method to post new clips from Twitch to the system, which is used to update the system with new clips that have been created on Twitch. This can be used to trigger any events or notifications based on new clips being created, such as posting a message in the channel about the new clip or sending a notification to a webhook about the new clip. The OnNewClipsDetectedArgs includes a list of clips and a flag indicating whether all clips were retrieved, which can be used to determine how to process the new clips in the system.
        /// </summary>
        /// <param name="clips">The event arguments for the new clips detected event.</param>
        public void TwitchPostNewClip(OnNewClipsDetectedArgs clips)
        {
            HandleBotEventPostNewClip(clips.AllClips, ConvertClips(clips.Clips));
        }

        /// <summary>
        /// Send notification messages based on a monitored channel stream went live.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchMultiStreamOnline(OnStreamOnlineArgs e)
        {
            HandleMultiLiveOnStreamOnline(new(e.Stream.UserName, Platform.Twitch, e.Stream.UserId), e.Stream.Title,
                e.Stream.StartedAt.ToLocalTime(), e.Stream.GameName);
        }

        /// <summary>
        /// Send notification messages based on the Twitch stream went live, which includes the stream title, the stream start time, and the stream category. This is used to trigger any events or notifications based on the stream going live, such as posting a message in the channel about the stream going live or sending a notification to a webhook about the stream going live. The NewStreamOnlineEventArgs includes the stream information such as the broadcaster username, title, start time, and game/category information, which can be used to provide detailed information about the stream in the notifications.    
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchStreamOnline(NewStreamOnlineEventArgs e)
        {
            Stream CurrStream = TwitchBots.CurrStream;

            if (CurrStream != null)
            {
                HandleOnStreamOnline(
                    e.StreamOnline.BroadcasterUserName,
                    CurrStream.Title,
                    CurrStream.StartedAt.ToLocalTime(),
                    new(CurrStream.GameId, CurrStream.GameName)
                    );
            } // else; should not happen, but if it does, what should we do here?
        }

        /// <summary>
        /// Send notification messages based on the Twitch stream went live, which includes the stream title, the stream start time, and the stream category. This is used to trigger any events or notifications based on the stream going live, such as posting a message in the channel about the stream going live or sending a notification to a webhook about the stream going live. The ResumeStreamOnlineEventArgs includes the stream information such as the broadcaster username, title, start time, and game/category information, which can be used to provide detailed information about the stream in the notifications. This event is specifically for when the bot detects that the stream is already online when it starts up or when it resumes monitoring after a disconnect, so it can send notifications about the stream that is currently online without waiting for a new stream online event to trigger.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchResumeStreamOnline(ResumeStreamOnlineEventArgs e)
        {
            HandleOnStreamOnline(
                e.Stream.UserName,
                e.Stream.Title,
                e.Stream.StartedAt.ToLocalTime(),
                new(e.Stream.GameId, e.Stream.GameName)
                );
        }

        /// <summary>
        /// Send notification messages based on a monitored channel stream updated their stream category.
        /// </summary>
        /// <param name="e">The event arguments for the stream update event.</param>
        internal void TwitchStreamUpdate(NewChannelUpdateEventArgs e)
        {
            HandleOnStreamUpdate(new(e.ChannelUpdate.CategoryId, e.ChannelUpdate.CategoryName));
        }

        /// <summary>
        /// Send notification messages based on a Twitch stream updated their stream category, which includes the new category name and category Id. This is used to trigger any events or notifications based on the stream updating its category, such as posting a message in the channel about the new category or sending a notification to a webhook about the category change. The FindChannelCategoryEventArgs includes the new category information such as the category name and category Id, which can be used to provide detailed information about the new category in the notifications. This event is specifically for when the bot detects a category update for the stream, so it can send notifications about the new category when it changes.
        /// </summary>
        /// <param name="e">The event arguments for the category update event.</param>
        public void TwitchCategoryUpdate(FindChannelCategoryEventArgs e)
        {
            HandleOnStreamUpdate(new(e.GameId, e.GameName));
        }

        /// <summary>
        /// Send notification message from finding a viewer category.
        /// </summary>
        /// <param name="e">The event arguments for the found viewer category event.</param>
        public void TwitchFoundViewerCategory(FindChannelCategoryEventArgs e)
        {
            HandleFoundViewerCategory(new(e.GameId, e.GameName));
        }

        /// <summary>
        /// Send notification messages based on a Twitch stream went offline, which is used to trigger any events or notifications based on the stream going offline, such as posting a message in the channel about the stream going offline or sending a notification to a webhook about the stream going offline. The NewStreamOfflineEventArgs includes the stream information such as the broadcaster username and user Id, which can be used to provide detailed information about the stream that went offline in the notifications. This event is specifically for when the bot detects that the stream has gone offline, so it can send notifications about the stream going offline when it happens.
        /// </summary>
        /// <param name="e">The event arguments for the stream offline event.</param>
        internal void TwitchStreamOffline(NewStreamOfflineEventArgs e)
        {
            HandleOnStreamOffline(Platform.Twitch);
        }

        /// <summary>
        /// Send notification message for a new subscriber.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchNewSubscriber(NewChannelSubscribeEventArgs e)
        {
            HandleNewSubscriber(
                new LiveUser(e.ChannelSubscribe.UserName, Platform.Twitch, e.ChannelSubscribe.UserId),
                "1",
                e.ChannelSubscribe.Tier.Replace("0", ""),
                e.ChannelSubscribe.Tier.Replace("0", ""));
        }

        /// <summary>
        /// Send notification message for a resubscriber.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchReSubscriber(NewChannelSubscriptionMessageEventArgs e)
        {
            HandleReSubscriber(
                new(e.ChannelSubscriptionMessage.UserName, Platform.Twitch, e.ChannelSubscriptionMessage.UserId),
                e.ChannelSubscriptionMessage.DurationMonths,
                e.ChannelSubscriptionMessage.CumulativeMonths.ToString(),
                e.ChannelSubscriptionMessage.Tier.Replace("0", ""),
                e.ChannelSubscriptionMessage.Tier.Replace("0", ""),
                e.ChannelSubscriptionMessage.StreakMonths != null,
                e.ChannelSubscriptionMessage.StreakMonths.ToString());
        }

        /// <summary>
        /// Send notification message for a gifted subscription, which includes the username of the person who gifted the subscription (or anonymous if the gift was anonymous), the number of months gifted, and the subscription tier. This is used to trigger any events or notifications based on a gifted subscription, such as posting a message in the channel about the gifted subscription or sending a notification to a webhook about the gifted subscription. The NewChannelSubscribeEventArgs includes the information about the gifted subscription such as the gifter username, the number of months gifted, and the subscription tier, which can be used to provide detailed information about the gifted subscription in the notifications. This event is specifically for when the bot detects a new gifted subscription, so it can send notifications about the gifted subscription when it happens.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchGiftSubscription(NewChannelSubscribeEventArgs e)
        {
            HandleGiftSubscription(
                new(null, Platform.Twitch, null),
                "1",
                e.ChannelSubscribe.UserName,
                e.ChannelSubscribe.Tier.Replace("0", ""),
                e.ChannelSubscribe.Tier.Replace("0", ""));
        }

        /// <summary>
        /// Send notification message for a community gifted subscription, which includes the username of the person who gifted the subscription (or anonymous if the gift was anonymous), the total number of subscriptions gifted in the community gift, and the subscription tier. This is used to trigger any events or notifications based on a community gifted subscription, such as posting a message in the channel about the community gifted subscription or sending a notification to a webhook about the community gifted subscription. The NewChannelSubscriptionGiftEventArgs includes the information about the community gifted subscription such as the gifter username, the total number of subscriptions gifted, and the subscription tier, which can be used to provide detailed information about the community gifted subscription in the notifications. This event is specifically for when the bot detects a new community gifted subscription, so it can send notifications about the community gifted subscription when it happens.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchCommunitySubscription(NewChannelSubscriptionGiftEventArgs e)
        {
            HandleCommunitySubscription(
                e.ChannelSubscriptionGift.IsAnonymous ? new(null, Platform.Twitch, null) : new(e.ChannelSubscriptionGift.UserName, Platform.Twitch, e.ChannelSubscriptionGift.UserId),
                e.ChannelSubscriptionGift.Total,
                e.ChannelSubscriptionGift.Tier.Replace("0", ""));
        }

        /// <summary>
        /// Send notification message for the current users in the Twitch channel when the bot starts up and detects existing users in the channel. This is used to trigger any events or notifications based on the existing users in the channel when the bot starts up, such as posting a message in the channel about the current viewers or sending a notification to a webhook about the current viewers. The StreamerOnExistingUserDetectedArgs includes a list of the current users in the channel, which can be used to provide detailed information about the current viewers in the notifications. This event is specifically for when the bot starts up and detects existing users in the channel, so it can send notifications about the current viewers at that time.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        public void TwitchCurrentUsers(StreamerOnExistingUserDetectedArgs e)
        {
            HandleUserJoined(e.Users);
        }

        //public void TwitchOnUserJoined(StreamerOnUserJoinedArgs e)
        //{
        //    HandleUserJoined([e.LiveUser]);
        //}

        /// <summary>
        /// Send notification message for a user leaving the Twitch channel, which includes the username and user Id of the user who left. This is used to trigger any events or notifications based on a user leaving the channel, such as posting a message in the channel about the user leaving or sending a notification to a webhook about the user leaving. The StreamerOnUserLeftArgs includes the information about the user who left such as their username and user Id, which can be used to provide detailed information about the user who left in the notifications. This event is specifically for when the bot detects a user leaving the channel, so it can send notifications about the user leaving when it happens.
        /// </summary>
        /// <param name="e">The event arguments for the user left event.</param>
        public void TwitchOnUserLeft(StreamerOnUserLeftArgs e)
        {
            HandleUserLeft(e.LiveUser);
        }

        //public void TwitchOnUserTimedout(OnUserTimedoutArgs e = null)
        //{
        //    HandleUserTimedOut(e);
        //}

        //public void TwitchOnUserBanned(OnUserBannedArgs e = null)
        //{
        //    HandleUserBanned(new(e.UserBan.TargetUserId, Platform.Twitch, e.UserBan.Username));
        //}

        /// <summary>
        /// Send notification message for a Twitch chat message received, which includes the username and user Id of the user who sent the message, the channel the message was sent in, whether the user is a broadcaster, moderator, subscriber, etc., the content of the message, and any bits included in the message. This is used to trigger any events or notifications based on a chat message being received, such as posting a message in the channel about the received message or sending a notification to a webhook about the received message. The ChannelChatMessageEventArgs includes all the information about the chat message and the user who sent it, which can be used to provide detailed information about the chat message in the notifications. This event is specifically for when the bot detects a new chat message in the channel, so it can send notifications about the received chat message when it happens.
        /// </summary>
        /// <param name="e">The event arguments for the chat message received event.</param>
        public void TwitchMessageReceived(ChannelChatMessageEventArgs e)
        {
            LogWriter.DebugLog("TwitchMessageReceived", DebugLogTypes.BotController, $"Received message {e.ChannelChatMessage.Message.Text} from {e.ChannelChatMessage.ChatterUserName} in {e.ChannelChatMessage.BroadcasterUserName} channel.");

            HandleMessageReceived(
                new()
                {
                    UserId = e.ChannelChatMessage.ChatterUserId,
                    DisplayName = e.ChannelChatMessage.ChatterUserName,
                    Channel = e.ChannelChatMessage.BroadcasterUserName,
                    IsBroadcaster = e.ChannelChatMessage.IsBroadcaster,
                    IsHighlighted = false,
                    IsMe = false,
                    IsModerator = e.ChannelChatMessage.IsModerator,
                    IsPartner = false,
                    IsSkippingSubMode = false,
                    IsStaff = e.ChannelChatMessage.IsStaff,
                    IsSubscriber = e.ChannelChatMessage.IsSubscriber,
                    IsTurbo = false,
                    IsVip = e.ChannelChatMessage.IsVip,
                    Message = e.ChannelChatMessage.Message.Text,
                    Bits = e.ChannelChatMessage.Cheer?.Bits ?? 0
                }
                , Platform.Twitch);
        }

        /// <summary>
        /// When a Twitch raid is incoming, the Twitch bot captures the data about the raid and sends it to the system to handle the incoming raid data, which may include posting messages to the channel about the raid, and posting notifications to any webhooks about the raid.
        /// </summary>
        /// <param name="e">The raid event data</param>
        public void TwitchIncomingRaid(OnIncomingRaidArgs e)
        {
            HandleIncomingRaidData(e.LiveUser, e.RaidTime, e.ViewerCount, e.Category);
        }
        
        /// <summary>
        /// When a Twitch raid is outgoing, the Twitch bot captures the data about the raid and sends it to the system to handle the outgoing raid data, which may include posting messages to the channel about the raid, and posting notifications to any webhooks about the raid.
        /// </summary>
        /// <param name="e">The raid event data</param>
        public void TwitchOutgoingRaid(OnStreamRaidResponseEventArgs e)
        {
            LogWriter.DebugLog("TwitchOutgoingRaid", DebugLogTypes.BotController, "");

            HandleOutgoingRaidData(e.ToChannel, e.CreatedAt, Platform.Twitch);
        }

        /// <summary>
        /// When a Twitch chat message is received, the Twitch bot captures the data about the message and sends it to the system to handle the incoming message data, which may include checking if the message is a command, and if so, executing the command and posting any responses to the channel, and posting notifications to any webhooks about the command.
        /// </summary>
        /// <param name="e">The chat message event data</param>
        public void TwitchChatCommandReceived(ChannelChatMessageEventArgs e)
        {
            string commandtext = "";
            List<string> cmdarglist = [];
            bool foundcommand = false;

            foreach (string f in e.ChannelChatMessage.Message.Text.Split(' '))
            {
                if (f.StartsWith('!') && !foundcommand)
                {
                    foundcommand = true;
                    commandtext = f[1..].ToLower();
                }
                else if (foundcommand)
                {
                    cmdarglist.Add(f);
                }
            }

            LogWriter.DebugLog("TwitchMessageReceived", DebugLogTypes.BotController, $"Received message {e.ChannelChatMessage.Message.Text} from {e.ChannelChatMessage.ChatterUserName} in {e.ChannelChatMessage.BroadcasterUserName} channel.");

            HandleChatCommandReceived(new()
            {
                CommandArguments = cmdarglist,
                CommandText = commandtext,
                UserId = e.ChannelChatMessage.ChatterUserId,
                DisplayName = e.ChannelChatMessage.ChatterUserName,
                Channel = e.ChannelChatMessage.BroadcasterUserName,
                IsBroadcaster = e.ChannelChatMessage.IsBroadcaster,
                IsHighlighted = false,
                IsMe = false,
                IsModerator = e.ChannelChatMessage.IsModerator,
                IsPartner = false,
                IsSkippingSubMode = false,
                IsStaff = e.ChannelChatMessage.IsStaff,
                IsSubscriber = e.ChannelChatMessage.IsSubscriber,
                IsTurbo = false,
                IsVip = e.ChannelChatMessage.IsVip,
                Message = e.ChannelChatMessage.Message.Text
            }, Platform.Twitch);
        }

        /// <summary>
        /// When a Twitch chat command is received, the Twitch bot captures the data about the command and sends it to the system to handle the incoming command data, which may include executing the command and posting any responses to the channel, and posting notifications to any webhooks about the command.
        /// </summary>
        /// <param name="e">The chat command event data</param>
        public void TwitchBotCommandCall(SendBotCommandEventArgs e)
        {
            HandleChatCommandReceived(e.CmdMessage, Platform.Twitch);
        }

        /// <summary>
        /// When a Twitch channel point reward is redeemed, the Twitch bot captures the data about the redemption and sends it to the system to handle the incoming redemption data, which may include checking if the reward title matches any configured rewards in the system, and if so, executing any actions associated with that reward and posting any responses to the channel, and posting notifications to any webhooks about the reward redemption.
        /// </summary>
        /// <param name="e">The channel point reward redemption event data</param>
        internal void TwitchChannelPointsRewardRedeemed(NewChannelCustomRewardRedemptionEventArgs e)
        {
            // currently only need the invoking user DisplayName and the reward title, for determining the reward is used for the giveaway.
            // much more data exists in the resulting data output

            LogWriter.DebugLog("TwitchChannelPointsRewardRedeemed", DebugLogTypes.TwitchBots, $"Received Twitch Channel Point Reward {e.ChannelPointsCustomRewardRedemption.Reward.Title}. Now processing.");

            HandleCustomReward(
                new(e.ChannelPointsCustomRewardRedemption.UserName, Platform.Twitch, e.ChannelPointsCustomRewardRedemption.UserId),
                e.ChannelPointsCustomRewardRedemption.Reward.Title,
                e.ChannelPointsCustomRewardRedemption.UserInput
                );
        }

        /// <summary>
        /// When a Twitch channel cheer is received, the Twitch bot captures the data about the cheer and sends it to the system to handle the incoming cheer data, which may include checking if the cheer message contains any keywords that match with any configured rewards in the system, and if so, executing any actions associated with that reward and posting any responses to the channel, and posting notifications to any webhooks about the cheer.
        /// </summary>
        /// <param name="channelCheer">The channel cheer event data</param>
        internal void TwitchChannelCheered(NewChannelCheerEventArgs channelCheer)
        {
            HandleChannelCheer(new(channelCheer.ChannelCheer.UserName, Platform.Twitch, channelCheer.ChannelCheer.UserId), channelCheer.ChannelCheer.Bits);
        }

        #endregion

        #region Handle Bot Events

        #region Followers

        /// <summary>
        /// When new followers are detected from Twitch, the Twitch bot captures the data about the new followers and sends it to the system to handle the incoming followers data, which may include updating the followers list in the system, posting messages to the channel about the new followers, and posting notifications to any webhooks about the new followers. This method is specifically for handling new followers detected from Twitch, and is called with the follower data such as the follower's username, user Id, and follow time.
        /// </summary>
        /// <param name="follow">The new follower data.</param>
        public void HandleBotEventNewFollowers(Models.Follow follow)
        {
            DataBot.AddNewFollowers([follow]);
        }

        /// <summary>
        /// When a bulk followers update operation starts, this method is called to indicate the start of the bulk update, which can be used to trigger any processes that need to wait for the bulk update to start before performing certain operations, such as preparing to receive the bulk followers data or resetting any temporary data structures used for processing the bulk update. This method is specifically for handling the start of a bulk followers update operation, and is called before the system starts receiving the bulk followers data from Twitch.
        /// </summary>
        public void HandleBotEventStartBulkFollowers()
        {
            OnBulkFollowerStarted?.Invoke(this, new());
            DataBot.StartBulkFollowers();
        }

        /// <summary>
        /// When a bulk followers update operation posts a list of followers, this method is called to handle the incoming bulk followers data, which may include updating the followers list in the system with the new bulk data, posting messages to the channel about any changes in followers based on the new bulk data, and posting notifications to any webhooks about the changes in followers. This method is specifically for handling the incoming bulk followers data from Twitch, and is called with a list of follower data such as the follower's username, user Id, and follow time for each follower in the bulk update. The system can then compare this new bulk followers list to the old followers list to find new followers and lost followers, and trigger any events or notifications based on those changes in the followers list.
        /// </summary>
        /// <param name="follows">The list of followers to update.</param>
        public void HandleBotEventBulkPostFollowers(List<Models.Follow> follows)
        {
            DataBot.UpdateFollowers(follows);
        }

        /// <summary>
        /// When a bulk followers update operation stops, this method is called to indicate the end of the bulk update, which can be used to trigger any processes that need to wait for the bulk update to stop before performing certain operations, such as finalizing the processing of the bulk followers data or cleaning up any temporary data structures used for processing the bulk update. This method is specifically for handling the end of a bulk followers update operation, and is called after the system has finished receiving and processing the bulk followers data from Twitch.
        /// </summary>
        public void HandleBotEventStopBulkFollowers()
        {
            DataBot.StopBulkFollowers();
        }

        #endregion

        #region Clips

        /// <summary>
        /// When new clips are detected from Twitch, the Twitch bot captures the data about the new clips and sends it to the system to handle the incoming clips data, which may include updating the clips list in the system, posting messages to the channel about the new clips, and posting notifications to any webhooks about the new clips. This method is specifically for handling new clips detected from Twitch, and is called with the clip data such as the clip's title, URL, creator username, and creation time.
        /// </summary>
        /// <param name="AllClips">Whether the current call is for all clips.</param>
        /// <param name="clips">The list of clips to update.</param>
        public void HandleBotEventPostNewClip(bool AllClips, List<Models.Clip> clips)
        {
            DataBot.ClipHelper(AllClips, clips);
        }

        /// <summary>
        /// When a request is made to get the clips for a Twitch channel, this method is called to handle the request for getting the channel clips, which may include retrieving the clips data from Twitch for the specified channel, and then posting the retrieved clips data back to the system through the provided callback function. This method is specifically for handling requests to get Twitch channel clips, and is called with the channel name and a callback function that should be called with the list of clips once they are retrieved from Twitch. The system can then use this clips data to update any relevant information in the system, post messages about the clips in the channel, or send notifications to any webhooks about the new clips.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments containing the channel name and callback function.</param>
        public void HandleGetUserClips(object sender, GetChannelClipsEventArgs e)
        {
            if (e.Platform == Platform.Twitch)
            {
                TwitchBots.GetUserChannelClips(e.ChannelName, e.CallBackResult);
            }
        }

        #endregion

        #region LiveStream

        private void HandleMultiLiveOnStreamOnline(LiveUser User, string Title, DateTime StartedAt, string Category)
        {
            DateTime CurrTime = StartedAt.ToLocalTime();

            // true posted new event, false did not post

            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                DataBot.PostMultiStreamDate(User, CurrTime, (PostedLive) => PostMultiLiveStreamDateCallBack(User, Title, Category, CurrTime, PostedLive));

                if (User.Platform == Platform.Twitch)
                {
                    if (OptionFlags.TwitchMultiLiveBrowseChannel)
                    {
                        string URL = Resources.TwitchHomepage + User.UserName;

                        Process startBrowser = new();
                        startBrowser.StartInfo.UseShellExecute = true;
                        startBrowser.StartInfo.FileName = $"\"{URL}\"";
                        _ = startBrowser.Start();
                    }
                }
            });
        }

        private static void PostMultiLiveStreamDateCallBack(LiveUser User, string Title, string Category, DateTime CurrTime, bool PostedLive)
        {
            if (PostedLive)
            {
                DataBot.CheckMultiLiveStreamDate(User.UserId, User.Platform, CurrTime, (MultiLive) => MultiStreamDateCallback(User, Title, Category, MultiLive));
            }
        }

        private static void MultiStreamDateCallback(LiveUser User, string Title, string Category, bool MultiLive)
        {
            if ((OptionFlags.PostMultiLive && MultiLive) || !MultiLive)
            {
                // get message, set a default if otherwise deleted/unavailable
                string msg = OptionFlags.MsgLive ?? "@everyone, #user is now live streaming #category - #title! Come join and say hi at: #url";

                // keys for exchanging codes for representative names
                Dictionary<string, string> dictionary = new()
                        {
                            { "#user", User.UserName },
                            { "#category", Category },
                            { "#title", Title },
                            { "#url", User.UserName }
                        };

                DataBot.PostMultiLiveLog(VariableParser.ParseReplace(msg, dictionary));
                DataBot.GetMultiWebHooks((Webhooks) => PostMultiWebHooksCallback(User, msg, dictionary, Webhooks));
            }
        }

        private static void PostMultiWebHooksCallback(LiveUser User, string msg, Dictionary<string, string> dictionary, IEnumerable<Tuple<WebhooksSource, Uri>> Webhooks)
        {
            foreach (Tuple<WebhooksSource, Uri> u in Webhooks)
            {
                if (u.Item1 == WebhooksSource.Discord)
                {
                    DiscordWebhook.SendMessage(u.Item2,
                        VariableParser.ParseReplace(msg, dictionary),
                        VariableParser.BuildPlatformUrl(User.UserName, User.Platform));
                }
            }
        }

        /// <summary>
        /// When a Twitch stream goes live, the Twitch bot captures the data about the stream and sends it to the system to handle the incoming stream data, which may include posting messages to the channel about the stream, and posting notifications to any webhooks about the stream. This method is specifically for handling multi-channel streaming, which means this method is called when any monitored channel goes live, and the system checks if it's a multi-channel stream and posts messages accordingly.
        /// </summary>
        /// <param name="e">The multi-channel stream summarize event data</param>
        public void MultiChannelSummarize(MultiLiveSummarizeEventArgs e)
        {
            DataBot.MultiSummarize(e);
        }

        private void ManageOnlineStream(Platform platform)
        {
            PlatformOnlineStatus[platform] = true;

            OnStreamOnline?.Invoke(this, new());
        }

        private void ManageOfflineStream(Platform platform)
        {
            PlatformOnlineStatus[platform] = false;

            if (!PlatformOnlineStatus.ContainsValue(true))
            {
                OnStreamOffline?.Invoke(this, new());
            }
        }

        /// <summary>
        /// When a stream goes online, this method is called to handle the stream online event, which includes managing the stream online status for the platform, managing the bots' stream status based on the new stream online status, posting messages to the channel about the stream going live, and posting notifications to any webhooks about the stream going live. This method is specifically for handling a single channel stream going online, and is called with the stream data such as channel name, title, start time, and category.
        /// </summary>
        /// <param name="ChannelName">The name of the channel going live</param>
        /// <param name="Title">The title of the stream</param>
        /// <param name="StartedAt">The start time of the stream</param>
        /// <param name="Category">The category of the stream</param>
        /// <param name="platform">The platform of the stream</param>
        /// <param name="Debug">A flag indicating whether to run in debug mode</param>
        public void HandleOnStreamOnline(string ChannelName, string Title, DateTime StartedAt, CategoryData Category, Platform platform = Platform.Twitch, bool Debug = false)
        {
            try
            {
                ManageOnlineStream(platform);

                ManageBotsStreamStatusChanged(true);

                OnStreamCategoryChanged?.Invoke(this, new() { GameId = Category.CategoryId, GameName = Category.CategoryName });

                DataBot.StreamOnline(StartedAt, Category, (isOnline) => CallbackHandleOnStreamOnline(isOnline, ChannelName, Title, StartedAt, Category, platform, Debug)); // a callback to finish the stream online process
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "HandleOnStreamOnline");
            }
        }

        private void CallbackHandleOnStreamOnline(bool Started, string ChannelName, string Title, DateTime StartedAt, CategoryData Category, Platform platform = Platform.Twitch, bool Debug = false)
        {
            try
            {
                if (Started)
                {
                    bool MultiLive = ActionSystem.CheckStreamDate(StartedAt); // since this call is within another callback through DataBot, we don't need to use DataBot

                    if ((OptionFlags.PostMultiLive && MultiLive) || !MultiLive)
                    {
                        // get message, set a default if otherwise deleted/unavailable
                        string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out bool Enabled, out _);

                        // keys for exchanging codes for representative names
                        Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                        {
                                new(MsgVars.user, ChannelName),
                                new(MsgVars.category, Category.CategoryName),
                                new(MsgVars.title, Title),
                                new(MsgVars.url, ChannelName)
                        });

                        string TempMsg = VariableParser.ParseReplace(msg, dictionary);

                        if (Enabled && !Debug)
                        {
                            DataBot.GetDiscordWebhooks(WebhooksKind.Live, (Webhooks) => PostToDiscord(ChannelName, platform, TempMsg, Webhooks));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "HandleOnStreamOnline");
            }
        }

        private void PostToDiscord(string ChannelName, Platform platform, string TempMsg, IEnumerable<Tuple<bool, Uri>> Webhooks)
        {
            foreach (Tuple<bool, Uri> u in Webhooks)
            {
                DiscordWebhook.SendMessage(u.Item2, VariableParser.ParseReplace(TempMsg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                                                {
                                                                        new(MsgVars.everyone, u.Item1 ? "@everyone" : "")
                                                }
                                            )
                                        ),
                                        VariableParser.BuildPlatformUrl(ChannelName, platform)
                                    );
                DataBot.UpdatedStat(StreamStatType.Discord);
            }
        }

        /// <summary>
        /// When a stream category is found for the viewer, this method is called to handle the found viewer category event, which includes posting messages to the channel about the viewer's category, and posting notifications to any webhooks about the viewer's category. This method is specifically for handling when a viewer's category is found, which may be used for features such as showing the viewer's current category in chat or on stream.
        /// </summary>
        /// <param name="categoryData">Describes the current category data for the viewer.</param>
        public void HandleFoundViewerCategory(CategoryData categoryData)
        {
            DataBot.PostViewerCategory(categoryData);
        }

        /// <summary>
        /// When a stream category is updated, this method is called to handle the stream update event, which includes updating the current category data in the system, posting messages to the channel about the stream's new category, and posting notifications to any webhooks about the stream's new category. This method is specifically for handling when a stream's category is updated while the stream is live, which may be used for features such as showing the stream's current category in chat or on stream, and notifying viewers of the category change.
        /// </summary>
        /// <param name="categoryData">Describes the updated category data for the stream.</param>
        public void HandleOnStreamUpdate(CategoryData categoryData)
        {
            DataBot.SetCategory(categoryData);

            if (OptionFlags.IsStreamOnline)
            {
                DataBot.PostCategoryStream(categoryData);
            }

            OnStreamCategoryChanged?.Invoke(this, new() { GameId = categoryData.CategoryId, GameName = categoryData.CategoryName });
        }

        /// <summary>
        /// When a stream goes offline, this method is called to handle the stream offline event, which includes managing the stream offline status for the platform, managing the bots' stream status based on the new stream offline status, posting messages to the channel about the stream going offline, and posting notifications to any webhooks about the stream going offline. This method is specifically for handling a single channel stream going offline, and is called with the stream data such as hosted channel (if a raid occurred) and raid time (if a raid occurred). The method also checks if the stream was previously online before posting any messages or notifications about the stream going offline.
        /// </summary>
        /// <param name="platform">The platform for which the stream is going offline.</param>
        /// <param name="HostedChannel">The channel that hosted the stream (if a raid occurred).</param>
        /// <param name="RaidTime">The time when the raid occurred (if a raid occurred).</param>
        public void HandleOnStreamOffline(Platform platform, string HostedChannel = null, DateTime? RaidTime = null)
        {
            LogWriter.DebugLog("HandleOnStreamOffline", DebugLogTypes.BotController, "Received a livestream offline status update.");

            ManageOfflineStream(platform);

            if (OptionFlags.IsStreamOnline)
            {
                LogWriter.DebugLog("HandleOnStreamOffline", DebugLogTypes.BotController, "Start notifying about the offline stream.");

                ManageBotsStreamStatusChanged(false);

                DateTime currTime = RaidTime?.ToLocalTime() ?? DateTime.Now.ToLocalTime();
                DataBot.StreamOffline(currTime);

                if (RaidTime != null)
                {
                    LogWriter.DebugLog("HandleOnStreamOffline", DebugLogTypes.BotController, $"A raid occurred to channel: {HostedChannel}.");
                }
                else
                {
                    LogWriter.DebugLog("HandleOnStreamOffline", DebugLogTypes.BotController, $"No raid occurred, stream offline.");
                }

                DataBot.PostOutgoingRaid(HostedChannel ?? "No Raid", currTime);
                //OptionFlags.TwitchOutRaidStarted = false;
            }
        }

        /// <summary>
        /// Call to manage other bots when a monitored stream is detected to be online.
        /// </summary>
        /// <param name="Start">True to start services for stream online, False to stop services for stream offline.</param>
        public static void ManageBotsStreamStatusChanged(bool Start)
        {
            LogWriter.DebugLog("ManageBotsStreamStatusChanged", DebugLogTypes.BotController, $"Starting any stopped bots or " +
                $"stopping any started bots, based on the current active livestream={Start} status.");

            // loop the bots and send message to start or stop based on stream online or offline status
            foreach (IBotTypes bots in BotsList)
            {
                bots.ManageStreamOnlineOfflineStatus(Start);
            }
        }

        #endregion

        #region Chat Bot

        /// <summary>
        /// When a chat bot starts, this method is called to handle the chat bot started event, which includes adding the started chat bot to the list of started chat bots, starting the message sending thread if it's not already started, posting a welcome message to the channel if the option is enabled, and notifying the data bot about the bot start. This method is specifically for handling when a chat bot starts, and may be called multiple times if multiple chat bots are used in the system. The method also checks if it's the first chat bot starting to manage the message sending thread accordingly.
        /// </summary>
        /// <param name="Source">The chat bot that has started.</param>
        /// <param name="args">Event arguments for the chat bot started event.</param>
        public void HandleChatBotStarted(Bots Source, EventArgs args)
        {
            lock (StartedChatBots)
            {
                StartedChatBots.UniqueAdd(Source);
            }

            if (StartedChatBots.Count == 1 && args == null)
            {
                SendThread = ThreadManager.CreateThread(
                                                        "HandleChatBotStarted",
                                                        BeginProcMsgs,
                                                        Priority: ThreadExitPriority.Normal);
                SendThread.Start();

                if (OptionFlags.MsgBotConnection)
                { // only show if user spcified they want the welcome message sent to chat
                    Send( Platform.Default, LocalizedMsgSystem.GetTwineBotAuthorInfo());
                }
                DataBot.NotifyBotStart();
            }
        }

        /// <summary>
        /// When a chat bot is stopping, this method is called to handle the chat bot stopping event, which includes removing the stopping chat bot from the list of started chat bots, stopping the message sending thread if it's no longer needed, and notifying the data bot about the bot stop. This method is specifically for handling when a chat bot is stopping, and may be called multiple times if multiple chat bots are used in the system.
        /// </summary>
        /// <param name="Source">The chat bot that is stopping.</param>
        /// <param name="args">Event arguments for the chat bot stopping event.</param>
        public void HandleChatBotStopping(Bots Source, EventArgs args)
        {
            lock (StartedChatBots)
            {
                StartedChatBots.RemoveAll((s) => s == Source);
            }

            if (StartedChatBots.Count == 0 && args == null)
            {
                DataBot.NotifyBotStop();
            }
            ChatBotStopping = true;
        }

        /// <summary>
        /// When a chat bot has stopped, this method is called to handle the chat bot stopped event, which includes checking if the stopped chat bot was the last one in the list of started chat bots, and if so, stopping the message sending thread and resetting the chat bot stopping status. This method is specifically for handling when a chat bot has fully stopped, and may be called multiple times if multiple chat bots are used in the system. The method also checks if it's the last chat bot stopping to manage the message sending thread accordingly.
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="args"></param>
        public void HandleChatBotStopped(Bots Source, EventArgs args)
        {
            if (Source == Bots.TwitchEventSubBot && args == null)
            {
                ChatBotStopping = false;
            }
        }

        /// <summary>
        /// When a new subscriber event is received, this method is called to handle the new subscriber event, which includes parsing the subscriber data, building the message to be sent to the channel and webhooks, posting messages to the channel about the new subscriber, and posting notifications to any webhooks about the new subscriber. This method is specifically for handling when a new subscriber is detected by the Twitch bot, and may be called multiple times if multiple subscribers are detected. The method also updates the subscription statistics in the data bot and checks for any overlay events related to the new subscriber.
        /// </summary>
        /// <param name="User">The user who subscribed.</param>
        /// <param name="Months">The number of months the user has subscribed.</param>
        /// <param name="Subscription">The type of subscription.</param>
        /// <param name="SubscriptionName">The name of the subscription.</param>
        public void HandleNewSubscriber(LiveUser User, string Months, string Subscription, string SubscriptionName)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out bool Enabled, out short Multi);

            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                new( MsgVars.user, User.UserName ),
                new( MsgVars.submonths, FormatData.Plurality(Months, MsgVars.Pluralmonth, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total)) ),
                new( MsgVars.subplan, Subscription ),
                new( MsgVars.subplanname, SubscriptionName )
                });
            string ParsedMsg = VariableParser.ParseReplace(msg, dictionary);
            string HTMLParsedMsg = VariableParser.ParseReplace(msg, dictionary, true);

            if (Enabled)
            {
                DataBot.GetEventAnnounce(ChannelEventActions.Subscribe, (result) => Send(User.Platform, ParsedMsg, result, Multi));
            }

            DataBot.UpdatedStat(StreamStatType.Sub, StreamStatType.AutoEvents);

            DataBot.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Subscribe, User, UserMsg: HTMLParsedMsg);
            DataBot.AddNewOverlayTickerItem(OverlayTickerItem.LastSubscriber, User.UserName);
        }

        /// <summary>
        /// When a re-subscriber event is received, this method is called to handle the re-subscriber event, which includes parsing the re-subscriber data, building the message to be sent to the channel and webhooks, posting messages to the channel about the re-subscriber, and posting notifications to any webhooks about the re-subscriber. This method is specifically for handling when a re-subscriber is detected by the Twitch bot, and may be called multiple times if multiple re-subscribers are detected. The method also updates the subscription statistics in the data bot and checks for any overlay events related to the re-subscriber. Additionally, if the user has chosen to share their subscription streak, it will include that information in the message.
        /// </summary>
        /// <param name="User">The user who re-subscribed.</param>
        /// <param name="Months">The number of months the user has re-subscribed.</param>
        /// <param name="TotalMonths">The total number of months the user has subscribed.</param>
        /// <param name="Subscription">The type of subscription.</param>
        /// <param name="SubscriptionName">The name of the subscription.</param>
        /// <param name="ShareStreak">Indicates whether the user wants to share their subscription streak.</param>
        /// <param name="StreakMonths">The number of months in the user's subscription streak.</param>
        public void HandleReSubscriber(LiveUser User, int Months, string TotalMonths, string Subscription, string SubscriptionName, bool ShareStreak, string StreakMonths)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out bool Enabled, out short Multi);
            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                new( MsgVars.user, User.UserName ),
                new( MsgVars.months, FormatData.Plurality(Months, MsgVars.Pluralmonth, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total)) ),
                new( MsgVars.submonths, FormatData.Plurality(TotalMonths, MsgVars.Pluralmonth, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total))),
                new( MsgVars.subplan, Subscription),
                new( MsgVars.subplanname,SubscriptionName )
                });

            // add the streak element if user wants their sub streak displayed
            if (ShareStreak)
            {
                VariableParser.AddData(ref dictionary, new Tuple<MsgVars, string>[] { new(MsgVars.streak, StreakMonths) });
            }

            string ParsedMsg = VariableParser.ParseReplace(msg, dictionary);
            string HTMLParsedMsg = VariableParser.ParseReplace(msg, dictionary, true);
            if (Enabled)
            {
                DataBot.GetEventAnnounce(ChannelEventActions.Resubscribe, (result) => Send(User.Platform, ParsedMsg, result, Multi));
            }
            DataBot.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Resubscribe, User, UserMsg: HTMLParsedMsg);

            DataBot.UpdatedStat(StreamStatType.Sub, StreamStatType.AutoEvents);
            DataBot.AddNewOverlayTickerItem(OverlayTickerItem.LastSubscriber, User.UserName);
        }

        /// <summary>
        /// When a gift subscription event is received, this method is called to handle the gift subscription event, which includes parsing the gift subscription data, building the message to be sent to the channel and webhooks, posting messages to the channel about the gift subscription, and posting notifications to any webhooks about the gift subscription. This method is specifically for handling when a gift subscription is detected by the Twitch bot, and may be called multiple times if multiple gift subscriptions are detected. The method also updates the gift subscription statistics in the data bot and checks for any overlay events related to the gift subscription.
        /// </summary>
        /// <param name="User">The user who gifted the subscription.</param>
        /// <param name="Months">The number of months the gift subscription covers.</param>
        /// <param name="RecipientUserName">The username of the recipient of the gift subscription.</param>
        /// <param name="Subscription">The type of subscription.</param>
        /// <param name="SubscriptionName">The name of the subscription.</param>
        public void HandleGiftSubscription(LiveUser User, string Months, string RecipientUserName, string Subscription, string SubscriptionName)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.GiftSub, out bool Enabled, out short Multi);
            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                    new(MsgVars.user, User.UserName ?? "anonymous"),
                    new(MsgVars.months, FormatData.Plurality(Months, MsgVars.Pluralmonth)),
                    new(MsgVars.receiveuser, RecipientUserName ?? "" ),
                    new(MsgVars.subplan, Subscription ?? "" ),
                    new(MsgVars.subplanname, SubscriptionName ?? "")
                });

            string ParsedMsg = VariableParser.ParseReplace(msg, dictionary);
            string HTMLParsedMsg = VariableParser.ParseReplace(msg, dictionary, true);
            if (Enabled)
            {
                DataBot.GetEventAnnounce(ChannelEventActions.GiftSub, (result) => Send(User.Platform, ParsedMsg, result, Multi));
            }
            DataBot.UpdatedStat(StreamStatType.GiftSubs, StreamStatType.AutoEvents);
            DataBot.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.GiftSub, User, UserMsg: HTMLParsedMsg);
            DataBot.AddNewOverlayTickerItem(OverlayTickerItem.LastGiftSub, User.UserName);
            //            SystemsController.AddNewOverlayTickerItem(OverlayTickerItem.LastSubscriber, RecipientUserName);
        }

        /// <summary>
        /// When a community subscription event is received, this method is called to handle the community subscription event, which includes parsing the community subscription data, building the message to be sent to the channel and webhooks, posting messages to the channel about the community subscription, and posting notifications to any webhooks about the community subscription. This method is specifically for handling when a community subscription is detected by the Twitch bot, and may be called multiple times if multiple community subscriptions are detected. The method also updates the gift subscription statistics in the data bot and checks for any overlay events related to the community subscription.
        /// </summary>
        /// <param name="User">The user who subscribed.</param>
        /// <param name="SubCount">The number of months the community subscription covers.</param>
        /// <param name="Subscription">The type of subscription.</param>
        public void HandleCommunitySubscription(LiveUser User, int SubCount, string Subscription)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.CommunitySubs, out bool Enabled, out short Multi);
            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                    new(MsgVars.user, User.UserName ?? "anonymous"),
                    new(MsgVars.count, FormatData.Plurality(SubCount, MsgVars.Pluralsub, Subscription)),
                    new(MsgVars.subplan, Subscription)
                });

            string ParsedMsg = VariableParser.ParseReplace(msg, dictionary);
            string HTMLParsedMsg = VariableParser.ParseReplace(msg, dictionary, true);
            if (Enabled)
            {
                DataBot.GetEventAnnounce(ChannelEventActions.CommunitySubs, (result) => Send(User.Platform, ParsedMsg, result, Multi));
            }

            DataBot.UpdatedStat(StreamStatType.GiftSubs, SubCount);
            DataBot.UpdatedStat(StreamStatType.AutoEvents);

            DataBot.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.CommunitySubs, User, UserMsg: HTMLParsedMsg);
            DataBot.AddNewOverlayTickerItem(OverlayTickerItem.LastGiftSub, User.UserName ?? "anonymous");
        }

        /// <summary>
        /// When a channel cheer event is received, this method is called to handle the channel cheer event, which includes parsing the cheer data, building the message to be sent to the channel and webhooks, posting messages to the channel about the cheer, and posting notifications to any webhooks about the cheer. This method is specifically for handling when a channel cheer is detected by the Twitch bot, and may be called multiple times if multiple cheers are detected. The method also updates the cheer statistics in the data bot and checks for any overlay events related to the cheer.
        /// </summary>
        /// <param name="user">The user who cheered.</param>
        /// <param name="Bits">The number of bits cheered.</param>
        public void HandleChannelCheer(LiveUser user, int Bits)
        {
            DataBot.UserCheered(user, Bits);
        }

        /// <summary>
        /// When a channel is hosted, this method is called to handle the being hosted event, which includes parsing the hosting data, building the message to be sent to the channel and webhooks, posting messages to the channel about being hosted, and posting notifications to any webhooks about being hosted. This method is specifically for handling when a channel is hosted by another channel, and may be called multiple times if multiple hosting events are detected. The method also updates the hosting statistics in the data bot and checks for any overlay events related to being hosted.
        /// </summary>
        /// <param name="User">The user who is being hosted.</param>
        /// <param name="HostedByChannel">The channel that is hosting.</param>
        /// <param name="IsAutoHosted">Indicates if the hosting is auto-hosted.</param>
        /// <param name="Viewers">The number of viewers watching the host channel.</param>
        public void HandleBeingHosted(LiveUser User, string HostedByChannel, bool IsAutoHosted, int Viewers)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BeingHosted, out bool Enabled, out short Multi);
            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                                            {
                    new(MsgVars.user, HostedByChannel ),
                    new(MsgVars.autohost, LocalizedMsgSystem.DetermineHost(IsAutoHosted) ),
                    new(MsgVars.viewers, FormatData.Plurality(Viewers, MsgVars.Pluralviewers
                     ))
                                            });
            string ParsedMsg = VariableParser.ParseReplace(msg, dictionary);
            string HTMLParsedMsg = VariableParser.ParseReplace(msg, dictionary, true);
            if (Enabled)
            {
                DataBot.GetEventAnnounce(ChannelEventActions.BeingHosted, (result) => Send(User.Platform, ParsedMsg, result, Multi));
            }

            DataBot.UpdatedStat(StreamStatType.Hosted, StreamStatType.AutoEvents);
            DataBot.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.BeingHosted, User, UserMsg: HTMLParsedMsg);
        }

        /// <summary>
        /// When a user joins the channel, this method is called to handle the user joined event, which includes parsing the user data, building the message to be sent to the channel and webhooks, posting messages to the channel about the user joining, and posting notifications to any webhooks about the user joining. This method is specifically for handling when a user joins the channel, and may be called multiple times if multiple users join. The method also updates the viewer statistics in the data bot and checks for any overlay events related to the user joining.
        /// </summary>
        /// <param name="Users">The list of users who joined.</param>
        public void HandleUserJoined(List<Models.LiveUser> Users)
        {
            DataBot.UserJoined(Users);
        }

        /// <summary>
        /// When a user leaves the channel, this method is called to handle the user left event, which includes parsing the user data, building the message to be sent to the channel and webhooks, posting messages to the channel about the user leaving, and posting notifications to any webhooks about the user leaving. This method is specifically for handling when a user leaves the channel, and may be called multiple times if multiple users leave. The method also updates the viewer statistics in the data bot and checks for any overlay events related to the user leaving.
        /// </summary>
        /// <param name="User">The user who left.</param>
        public void HandleUserLeft(Models.LiveUser User)
        {
            DataBot.UserLeft(User);
        }

        /// <summary>
        /// When a user is banned from the channel, this method is called to handle the user banned event, which includes parsing the user data, building the message to be sent to the channel and webhooks, posting messages to the channel about the user being banned, and posting notifications to any webhooks about the user being banned. This method is specifically for handling when a user is banned from the channel, and may be called multiple times if multiple users are banned. The method also updates the ban statistics in the data bot and checks for any overlay events related to the user being banned. Additionally, it calls the HandleUserLeft method to manage any necessary actions related to the user leaving due to being banned.
        /// </summary>
        /// <param name="User">The user who was banned.</param>
        public void HandleUserBanned(LiveUser User)
        {
            try
            {
                DataBot.UpdatedStat(StreamStatType.UserBanned);
                HandleUserLeft(User);

                DataBot.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.BannedUser, User);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "HandleUserBanned");
            }
        }

        /// <summary>
        /// When a user is added to the channel (e.g. joins the chat), this method is called to handle the add chat event, which includes parsing the user data, building the message to be sent to the channel and webhooks, posting messages to the channel about the user joining, and posting notifications to any webhooks about the user joining. This method is specifically for handling when a user is added to the channel, such as when they join the chat, and may be called multiple times if multiple users are added. The method also updates the viewer statistics in the data bot and checks for any overlay events related to the user being added.
        /// </summary>
        /// <param name="UserName">The name of the user who joined.</param>
        /// <param name="Source">The platform from which the user joined.</param>
        public void HandleAddChat(string UserName, Platform Source)
        {
            DataBot.UserJoined([new(UserName, Source)]);
        }

        /// <summary>
        /// When a message is received in the chat, this method is called to handle the message received event, which includes parsing the message data, building the message to be sent to the channel and webhooks, posting messages to the channel about the received message, and posting notifications to any webhooks about the received message. This method is specifically for handling when a message is received in the chat, and may be called multiple times if multiple messages are received. The method also checks for any overlay events related to the received message.
        /// </summary>
        /// <param name="MsgReceived">The message that was received.</param>
        /// <param name="Source">The platform from which the message was received.</param>
        public void HandleMessageReceived(Models.CmdMessage MsgReceived, Platform Source)
        {
            DataBot.MessageReceived(MsgReceived, new(MsgReceived.DisplayName, Source, MsgReceived.UserId));
        }

        /// <summary>
        /// When a raid occurs, this method is called to handle the incoming raid data, which includes parsing the raid data, building the message to be sent to the channel and webhooks, posting messages to the channel about the incoming raid, and posting notifications to any webhooks about the incoming raid. This method is specifically for handling when a raid is detected by the Twitch bot, and may be called multiple times if multiple raids are detected. The method also updates the raid statistics in the data bot and checks for any overlay events related to the incoming raid.
        /// </summary>
        /// <param name="User">The user who initiated the raid.</param>
        /// <param name="RaidTime">The time the raid occurred.</param>
        /// <param name="ViewerCount">The number of viewers in the raid.</param>
        /// <param name="Category">The category of the raid.</param>
        public void HandleIncomingRaidData(Models.LiveUser User, DateTime RaidTime, int ViewerCount, CategoryData Category)
        {
            DataBot.PostIncomingRaid(User, RaidTime.ToLocalTime(), ViewerCount, Category);
        }

        /// <summary>
        /// When a raid occurs, this method is called to handle the outgoing raid data, which includes parsing the raid data, building the message to be sent to the channel and webhooks, posting messages to the channel about the outgoing raid, and posting notifications to any webhooks about the outgoing raid. This method is specifically for handling when a raid is detected by the Twitch bot as an outgoing raid (e.g. when the stream goes offline due to a raid), and may be called multiple times if multiple outgoing raids are detected. The method also updates the raid statistics in the data bot and checks for any overlay events related to the outgoing raid.  
        /// </summary>
        /// <param name="ToChannelName">The name of the channel to which the raid is outgoing.</param>
        /// <param name="RaidTime">The time the raid occurred.</param>
        /// <param name="platform">The platform from which the raid was detected.</param>
        public void HandleOutgoingRaidData(string ToChannelName, DateTime RaidTime, Platform platform)
        {
            HandleOnStreamOffline(platform, ToChannelName, RaidTime);
        }

        /// <summary>
        /// When a chat command is received, this method is called to handle the chat command received event, which includes parsing the command data, building the message to be sent to the channel and webhooks, posting messages to the channel about the received command, and posting notifications to any webhooks about the received command. This method is specifically for handling when a chat command is received in the chat, and may be called multiple times if multiple commands are received. The method also checks if the received command matches any giveaway criteria for entering a giveaway, and if so, posts the user to the giveaway. Additionally, it processes the command through the data bot to handle any commands that may be registered in the system.
        /// </summary>
        /// <param name="commandmsg">The chat command message received.</param>
        /// <param name="Source">The platform from which the command was received.</param>
        public void HandleChatCommandReceived(Models.CmdMessage commandmsg, Platform Source)
        {
            if (GiveawayItemType == GiveawayTypes.Command && commandmsg.CommandText == GiveawayItemName)
            {
                HandleGiveawayPostName(new(commandmsg.DisplayName, Source, commandmsg.UserId));
            }
            DataBot.ProcessCommand(commandmsg, Source);
        }

        /// <summary>
        /// When a custom reward redemption is received, this method is called to handle the custom reward redemption event, which includes parsing the reward data, building the message to be sent to the channel and webhooks, posting messages to the channel about the custom reward redemption, and posting notifications to any webhooks about the custom reward redemption. This method is specifically for handling when a custom reward redemption is received from Twitch channel points, and may be called multiple times if multiple redemptions are received. The method also checks if the received custom reward redemption matches any giveaway criteria for entering a giveaway, and if so, posts the user to the giveaway. Additionally, it checks if the custom reward redemption requires approval through the data bot, and if so, posts an approval request to the channel and webhooks. Finally, it checks for any overlay events related to the custom reward redemption.
        /// </summary>
        /// <param name="User">The user who redeemed the custom reward.</param>
        /// <param name="RewardTitle">The title of the custom reward redeemed.</param>
        /// <param name="RewardMsg">The message associated with the custom reward redemption.</param>
        public void HandleCustomReward(LiveUser User, string RewardTitle, string RewardMsg)
        {
            if (GiveawayItemType == GiveawayTypes.CustomRewards && RewardTitle == GiveawayItemName)
            {
                HandleGiveawayPostName(User);
            }

            DataBot.GetApprovalRule(ModActionType.ChannelPoints, RewardTitle, (result) => CustRewardDataBotCallback(User, RewardTitle, RewardMsg, result));
        }

        private void CustRewardDataBotCallback(LiveUser User, string RewardTitle, string RewardMsg, Tuple<string, string> approval)
        {
            if (approval != null)
            {
                LogWriter.DebugLog("HandleCustomReward", DebugLogTypes.TwitchBots, $"Custom reward {RewardTitle} requires approval.");

                switch (User.Platform)
                {
                    case Platform.Twitch:
                        DataBot.PostApproval($"{approval.Item2} {User.UserName} {RewardMsg}",
                            new(() =>
                            {
                                TwitchBots.PostInternalCommand(approval.Item2, [User.UserName, RewardMsg], $"!{approval.Item2} {User.UserName} {RewardMsg}");
                            })
                        );

                        TwitchBots.PostInternalCommand(LocalizedMsgSystem.GetVar(DefaultCommand.approve), [], $"!{LocalizedMsgSystem.GetVar(DefaultCommand.approve)}");
                        break;
                }
            }

            if (OptionFlags.MediaOverlayChannelPoints)
            {
                LogWriter.DebugLog("HandleCustomReward", DebugLogTypes.OverlayBot, $"Checking Channel Point Redemption {RewardTitle} for an Overlay request.");

                DataBot.CheckForOverlayEvent(OverlayTypes.ChannelPoints, RewardTitle, User);
            }
        }

        #region Giveaway
        /// <summary>
        /// Begin Giveaway with provided criteria. Registers the criteria for entering a viewer into the giveaway.
        /// </summary>
        /// <param name="giveawayTypes">The type of giveaway - e.g. channel point redemption, or command</param>
        /// <param name="ItemName">The identifier of the giveaway event.</param>
        public void HandleGiveawayBegin(GiveawayTypes giveawayTypes, string ItemName)
        {
            GiveawayItemType = giveawayTypes;
            GiveawayItemName = ItemName;
            GiveawayStarted = true;

            DataBot.BeginGiveaway();
        }

        /// <summary>
        /// Finish up/end the giveaway. Clears out data for next giveaway to start fresh.
        /// </summary>
        public void HandleGiveawayEnd()
        {
            DataBot.EndGiveaway();

            GiveawayItemType = GiveawayTypes.None;
            GiveawayItemName = "";
            GiveawayStarted = false;
        }

        /// <summary>
        /// Post a user to the giveaway.
        /// </summary>
        /// <param name="User">The user entering the giveaway.</param>
        public void HandleGiveawayPostName(LiveUser User)
        {
            DataBot.ManageGiveaway(User);
        }

        /// <summary>
        /// Finish up and award the giveaway winners.
        /// </summary>
        public void HandleGiveawayWinner()
        {
            if (GiveawayStarted)
            {
                HandleGiveawayEnd();
            }
            DataBot.PostGiveawayResult();
        }


        #endregion

        /// <summary>
        /// Initiates repeat timers.
        /// </summary>
        public void ActivateRepeatTimers()
        {
            DataBot.ActivateRepeatTimers();
        }

        /// <summary>
        /// Resets the repeat timer mode, which may be used to manage the state of repeat timers in the system, such as when a stream goes offline or when certain events occur that require resetting the timers. This method is specifically for resetting the repeat timer mode in the data bot, and may be called in various scenarios where managing the state of repeat timers is necessary.
        /// </summary>
        public void ResetRepeatTimerMode()
        {
            DataBot.ResetRepeatTimerMode();
        }

        #endregion

        #region UserBot

        private void Systems_BanUserRequest(object sender, BanUserRequestEventArgs e)
        {
            if (e.User.Platform == Platform.Twitch)
            {
                // TODO: verify users are correctly determined to be banned before banning, added to log
                LogWriter.WriteLog($"Request to ban or timeout user {e.User.UserName} for {e.BanReason} for {e.Duration} seconds.");
                //TwitchBots.BanUserRequest(e.UserName, e.BanReason, e.Duration);
            }
        }

        #endregion

        #region MediaOverlay

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

        /// <summary>
        /// Sends the initial ticker items to the overlay server bot, which may include information such as the latest subscriber, latest gift subscriber, or other relevant data that should be displayed on the stream overlay when it first loads. This method is specifically for sending the initial ticker items from the data bot to the overlay server bot, and may be called when the stream starts or when the overlay is initialized to ensure that the most up-to-date information is displayed on the overlay.
        /// </summary>
        public void SendInitialTickerItems()
        {
            DataBot.SendInitialTickerItems();
        }

        /// <summary>
        /// Sets the list of channel rewards that the data bot should be aware of for processing custom reward redemptions and managing any related functionality, such as checking for giveaway entries or handling approval processes. This method is specifically for updating the data bot with the current list of channel rewards from Twitch, and may be called whenever there are changes to the channel rewards (e.g. new rewards added, rewards removed, or reward names changed) to ensure that the data bot has the most accurate information for handling custom reward redemptions.
        /// </summary>
        /// <param name="channelPointNames">The list of channel point names.</param>
        public void SetChannelRewardList(List<string> channelPointNames)
        {
            DataBot.SetChannelRewardList(channelPointNames);
        }

        #endregion

        #region Ad Messages

        /// <summary>
        /// Starts the Twitch ad notification thread, which listens for ad-related events from the Twitch bot and sends notifications to the data bot when ads start, end, or are upcoming. This method is specifically for initiating the ad notification system that allows the data bot to manage any necessary actions related to Twitch ads, such as pausing certain functionalities during ads or displaying ad-related information on the stream overlay. The method may be called when the Twitch bot is initialized or when the stream goes live to ensure that ad notifications are properly set up and functioning throughout the stream.
        /// </summary>
        public void StartTwitchAdNotifications()
        {
            TwitchBots.StartAdNotificationThread();
        }

        private void TwitchBots_NotifyAdEnded(object sender, EventArgs e)
        {
            DataBot.NotifyAdEnd(Platform.Twitch);
        }

        private void TwitchBots_NotifyAdStarted(object sender, NotifyAdStartedEventArgs e)
        {
            DataBot.NotifyAdStart(Platform.Twitch, e.AdDuration);
        }

        private void TwitchBots_NotifyAdSoon(object sender, NotifyAdSoonEventArgs e)
        {
            DataBot.NotifyAdSoon(Platform.Twitch, e.SecondsUntilAd, e.AdDuration);
        }

        #endregion

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
