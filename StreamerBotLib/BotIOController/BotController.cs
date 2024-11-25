using StreamerBotLib.BotClients;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Threading;

using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;
using TwitchLib.EventSub.Core.Models.Chat;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

// TODO: add/verify "DataManage.UpdateStats" updates; including commands, chats, clips, channel redeems
// TODO: Add Bot contacts users to invoke conversation; carry-on conversation with existing

namespace StreamerBotLib.BotIOController
{
    public class BotController
    {
        public event EventHandler<PostChannelMessageEventArgs> OutputSentToBots;
        public event EventHandler<InvalidAccessTokenEventArgs> InvalidAuthorizationToken;
        public event EventHandler TokensInitialized;

        public event EventHandler OnStreamOnline;
        public event EventHandler<OnGetChannelGameNameEventArgs> OnStreamCategoryChanged;
        public event EventHandler OnStreamOffline;

        private Dictionary<Platform, bool> PlatformOnlineStatus = new(from Platform P in Enum.GetValues<Platform>()
                                                                      select new KeyValuePair<Platform, bool>(P, false));

        private static Dispatcher AppDispatcher { get; set; }
        public SystemsController Systems { get; private set; }
        internal static Collection<IBotTypes> BotsList { get; private set; } = [];
        public List<Bots> StartedChatBots { get; private set; } = [];
        private bool ChatBotStopping;

        private GiveawayTypes GiveawayItemType = GiveawayTypes.None;
        private string GiveawayItemName = "";
        private bool GiveawayStarted = false;

        private BotsTwitch TwitchBots { get; set; }
        public static BotOverlayServer OverlayServerBot { get; set; } = new();

        private const int SendMsgDelay = 750;
        // 600ms between messages, permits about 100 messages max in 60 seconds == 1 minute
        // 759ms between messages, permits about 80 messages max in 60 seconds == 1 minute
        private Queue<Task> Operations { get; set; } = new();   // an ordered list, enqueue into one end, dequeue from other end
        private Thread SendThread;  // the thread for sending messages back to the monitored channels

        public BotController()
        {
            OptionFlags.ActiveToken = true;

            Systems = new();
            Systems.PostChannelMessage += Systems_PostChannelMessage;
            Systems.BanUserRequest += Systems_BanUserRequest;

            TwitchBots = new();
            TwitchBots.BotEvent += HandleBotEvent;
            OutputSentToBots += ActionSystem.OutputSentToBotsHandler;

            SetNewOverlayEventHandler();

            BotsList.Add(TwitchBots);
            BotsList.Add(OverlayServerBot);

            TwitchBots.InvalidTwitchAccess += TwitchBots_InvalidTwitchAccess;
            TwitchBots.OnTwitchTokensInitialized += TwitchBots_OnTwitchTokensInitialized;
        }

        private void TwitchBots_OnTwitchTokensInitialized(object sender, EventArgs e)
        {
            TokensInitialized?.Invoke(this, new());
            GetUserCategory();
        }

        /// <summary>
        /// Initializes a Helix api.
        /// </summary>
        public static void TwitchInitializeHelix()
        {
            BotsTwitch.InitializeHelix();
        }

        public static void NotifyInvalidTwitchTokens()
        {
            BotsTwitch.NotifyInvalidTwitchTokens();
        }

        /// <summary>
        /// Notify when authorized bots fail and access/refresh tokens are now invalid and can't be renewed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchBots_InvalidTwitchAccess(object sender, InvalidAccessTokenEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "");

            InvalidAuthorizationToken?.Invoke(this, e);
        }

        /// <summary>
        /// Associate the dispatcher from the GUI thread, necessary to run code based on the GUI thread objects.
        /// </summary>
        /// <param name="dispatcher">The GUI thread Application.Dispatcher</param>
        public void SetDispatcher(Dispatcher dispatcher)
        {
            AppDispatcher = dispatcher;
            Systems.SetDispatcher(dispatcher);
        }

        /// <summary>
        /// Receives a bundled event from the bots, which is unpackaged and now runs on the GUI thread dispatcher.
        /// </summary>
        /// <param name="sender">Unused.</param>
        /// <param name="e">The parameters to include the method name to invoke, and the event arguments for the invoked method.</param>
        private void HandleBotEvent(object sender, BotEventArgs e)
        {
            AppDispatcher.BeginInvoke(() =>
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Event, {e.MethodName}, received from bots to post into system.");

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
                            TwitchCategoryUpdate((OnGetChannelGameNameEventArgs)e.e);
                            break;
                        case BotEvents.TwitchNowHosting:
                            break;
                        case BotEvents.TwitchOnUserLeft:
                            TwitchOnUserLeft((StreamerOnUserLeftArgs)e.e);
                            break;
                        case BotEvents.TwitchOnUserTimedout:
                            TwitchOnUserTimedout((OnUserTimedoutArgs)e.e);
                            break;
                        case BotEvents.TwitchOnUserBanned:
                            TwitchOnUserBanned((OnUserBannedArgs)e.e);
                            break;
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
                        case BotEvents.HandleBotEventEmpty:
                            break;
                    }

                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
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
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received message to post to chat: {e.Msg}");

            Send(e.Msg, e.RepeatMsg);
        }

        /// <summary>
        /// Send a response message to all bots incorporated into this app. The messages send through a thread managing a message delay to not flood the channel with immediate messages, channels often have limited received messages per minute.
        /// </summary>
        /// <param name="s">The string to send.</param>
        public void Send(string s, int Repeat = 0)
        {
            OutputSentToBots?.Invoke(this, new() { Msg = s });

            foreach (IBotTypes bot in BotsList)
            {
                lock (Operations)
                {
                    for (int x = 0; x <= Repeat; x++)
                    {
                        Operations.Enqueue(new Task(() =>
                        {
                            bot.Send(s);
                        }));
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
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "User wants to exit the bot.");
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Sending a Twitch Stream Offline message.");

                TwitchStreamOffline(null);

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Waiting for any queued messages to finish sending to the channel.");

                SendThread?.Join(); // wait until all the messages are sent to ask bots to close

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Stopping bots.");

                foreach (IBotTypes bot in BotsList)
                {
                    bot.StopBots();
                }

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Sending an exit to the data system.");
                Systems.Exit();
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// This method checks the user settings and will delete any DB data if the user unchecks the setting. 
        /// Other methods to manage users & followers will adapt to if the user adjusted the setting
        /// </summary>
        public static void ManageDatabase()
        {
            SystemsController.ManageDatabase();
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

        #region Send Data Updates to Database

        /// <summary>
        /// Send a 'Clear Watch Time' to the system database.
        /// </summary>
        public static void ClearWatchTime()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received \"Clear Watch Time\" request.");

            SystemsController.ClearWatchTime();
        }

        /// <summary>
        /// Send a 'clear all currency values' to the system database.
        /// </summary>
        public static void ClearAllCurrenciesValues()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Received \"Clear All Currencies Values\" request.");

            SystemsController.ClearAllCurrenciesValues();
        }

        /// <summary>
        /// Send a 'clear all users non followers' to the system database.
        /// </summary>
        public static void ClearUsersNonFollowers()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Received \"Clear Users Non Followers\" request.");

            SystemsController.ClearUsersNonFollowers();
        }

        /// <summary>
        /// Send a "Set System Events Enabled" toggle request to the system database.
        /// </summary>
        /// <param name="Enabled">True or False to set System Events in bulk.</param>
        public static void SetSystemEventsEnabled(bool Enabled)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received a \"Set System Events Enabled\" " +
                $"request to set all events to {Enabled}.");

            SystemsController.SetSystemEventsEnabled(Enabled);
        }

        /// <summary>
        /// Send a "Set BuiltIn Commands Enabled" toggle request to the system database.
        /// </summary>
        /// <param name="Enabled">True or False to set built-in commands in bulk.</param>
        public static void SetBuiltInCommandsEnabled(bool Enabled)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received a \"Set Built-in " +
                $"Commands Enabled\" request set all events to {Enabled}.");

            SystemsController.SetBuiltInCommandsEnabled(Enabled);
        }

        /// <summary>
        /// Send a "Set User Defined Commands Enabled" toggle request to the system database.
        /// </summary>
        /// <param name="Enabled">True or False to set user defined commands in bulk.</param>
        public static void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received a \"Set User Defined " +
                $"Commands Enabled\" request set all events to {Enabled}.");

            SystemsController.SetUserDefinedCommandsEnabled(Enabled);
        }

        /// <summary>
        /// Send a "Set WebHooks Webhooks Enabled" toggle request to the system database.
        /// </summary>
        /// <param name="Enabled">True or False to set WebHooks Webhooks in bulk.</param>
        public static void SetDiscordWebhooksEnabled(bool Enabled)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received a \"Set WebHooks Webhooks Enabled\" " +
                $"request to set all events to {Enabled}.");

            SystemsController.SetDiscordWebhooksEnabled(Enabled);
        }

        /// <summary>
        /// Insert a new AutoShoutUser entry into the database.
        /// </summary>
        /// <param name="UserName">The username to add into the database for the autoshout table.</param>
        public static void AddNewAutoShoutUser(string Userid, Platform platform)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received an \"Add New Auto Shout User\" " +
                $"request to add= {Userid} =to the database.");

            SystemsController.AddNewAutoShoutUser(Userid, platform);
        }

        #endregion

        #region Query Bots

        /// <summary>
        /// Part of the Twitch-Auth-Code Token operation method.
        /// Call to clear out the Twitch Authorization Code(s) to permit the user to re-authorize the application.
        /// </summary>
        public static void ForceTwitchAuthReauthorization()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Received request to invalidate Twitch Authorization Codes so user can re-authorize application.");
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "It's okay, there's a button in the GUI for the user to click and perform this operation.");

            BotsTwitch.ForceTwitchReauthorization();
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
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Received a request for the Bot username.");

            return Source switch
            {
                Platform.Twitch => OptionFlags.TwitchBotUserName,
                _ => "None",
            };
        }

        public void GetUserCategory()
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
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received request to provide the " +
                    $"streaming category for the channel named: {ChannelName}, with {UserId} userId.");

                return BotsTwitch.GetUserCategory(UserId: UserId, UserName: ChannelName).CategoryName;
            }
            else
            {
                return "";
            }
        }

        public static DateTime GetUserAccountAge(string UserName, Platform bots)
        {
            if (bots == Platform.Twitch)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received request " +
                    $"to ask Twitch for {UserName}'s account age.");

                return BotsTwitch.GetUserAccountAge(UserName: UserName);
            }
            else
            {
                return DateTime.MaxValue;
            }
        }

        public static bool VerifyUserExist(string ChannelName, Platform bots)
        {
            if (bots == Platform.Twitch)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received request " +
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

        public static bool ModifyChannelInformation(Platform bots, string Title = null, string CategoryName = null, string CategoryId = null)
        {
            bool result = false;

            if (bots == Platform.Twitch)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received request to " +
                    $"change the Twitch channel information to Title: {Title}, Category: {CategoryName}.");
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "One of these values can be null, because there are " +
                    "separate !settitle and !setcategory commands, where these separate values can come through this class method.");

                result = BotsTwitch.ModifyChannelInformation(Title, CategoryName, CategoryId);
            }

            return result;
        }

        public static void RaidChannel(string ToChannelName, Platform bots)
        {
            if (bots == Platform.Twitch)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received a request " +
                    $"to raid the: {ToChannelName}, Twitch channel.");

                BotsTwitch.RaidChannel(ToChannelName);
            }
        }

        public static void CancelRaidChannel(Platform bots)
        {
            if (bots == Platform.Twitch)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received a request to " +
                    $"cancel the pending Twitch channel raid.");

                BotsTwitch.CancelRaidChannel();
            }
        }

        public static void GetViewerCount(Platform bots)
        {
            if (OptionFlags.IsStreamOnline)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Found the stream is online.");

                ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
                {
                    if (bots == Platform.Twitch || bots == Platform.Default)
                    {
                        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Received a request to get the " +
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
        /// <param name="OpenBrowser"></param>
        /// <param name="AuthenticationFinished">A callback method once the bot concludes using the auth code to get an access/refresh token.</param>
        public static void TwitchTokenAuthCodeAuthorize(string clientId, bool NoScopes, Action<string> OpenBrowser, Action AuthenticationFinished)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Received request to activate " +
                "a Twitch authorization code for a specific client Id, which returns an initial access token and a refresh token to begin accessing Twitch.");

            BotsTwitch.TwitchActivateAuthCode(clientId, NoScopes, OpenBrowser, AuthenticationFinished);
        }

        /// <summary>
        /// Interface method to ask the bot(s) to create a clip; which asks Twitch to create a clip.
        /// </summary>
        public static void CreateClip()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Recieved a request to create a Twitch clip.");

            BotsTwitch.CreateClip();
        }

        public static string GetMultiChannelUserId(string UserName)
        {
            return BotsTwitch.GetUserId(UserName);
        }

        #endregion

        #region Twitch Bot Events

        public void TwitchStartUpdateAllFollowers()
        {
            TwitchBots.GetAllFollowers();
        }

        internal void TwitchPostNewFollowers(NewChannelFollowEventArgs Follower)
        {
            HandleBotEventNewFollowers(ConvertFollowers(Follower.Channel, Platform.Twitch));
        }

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

        public void TwitchBotEventSubStarted(EventArgs args = null)
        {
            HandleChatBotStarted(Bots.TwitchBotEventSub, args);
        }

        public void TwitchBotEventSubStopping(EventArgs args = null)
        {
            HandleChatBotStopped(Bots.TwitchBotEventSub, args);
        }

        public void TwitchBotEventSubStopped(EventArgs args = null)
        {
            HandleChatBotStopped(Bots.TwitchBotEventSub, args);
        }

        public static void TwitchStartBulkFollowers()
        {
            HandleBotEventStartBulkFollowers();
        }

        public static void TwitchBulkPostFollowers(OnNewFollowersDetectedArgs Follower)
        {
            HandleBotEventBulkPostFollowers(ConvertFollowers(Follower.NewFollowers, Platform.Twitch));
        }

        public void TwitchClipSvcOnClipFound(ClipFoundEventArgs clips)
        {
            HandleBotEventPostNewClip(ConvertClips(clips.ClipList));
        }

        public static List<Models.Clip> ConvertClips(List<TwitchLib.Api.Helix.Models.Clips.GetClips.Clip> clips)
        {
            return clips.ConvertAll((SrcClip) =>
            {
                return new Models.Clip()
                {
                    ClipId = SrcClip.Id,
                    CreatedAt = SrcClip.CreatedAt,
                    Duration = SrcClip.Duration,
                    GameId = SrcClip.GameId,
                    Language = SrcClip.Language,
                    Title = SrcClip.Title,
                    Url = SrcClip.Url,
                    FromUserId = SrcClip.CreatorId,
                    FromUserName = SrcClip.CreatorName
                };
            });
        }

        public void TwitchPostNewClip(OnNewClipsDetectedArgs clips)
        {
            HandleBotEventPostNewClip(ConvertClips(clips.Clips));
        }

        /// <summary>
        /// Send notification messages based on a monitored channel stream went live.
        /// </summary>
        /// <param name="e"></param>
        internal void TwitchMultiStreamOnline(OnStreamOnlineArgs e)
        {
            HandleMultiLiveOnStreamOnline(e.Stream.UserId, e.Stream.UserName, e.Stream.Title,
                e.Stream.StartedAt.ToLocalTime(), e.Stream.GameId, e.Stream.GameName);
        }

        internal void TwitchStreamOnline(NewStreamOnlineEventArgs e)
        {
            Stream CurrStream = TwitchBots.CurrStream;

            HandleOnStreamOnline(
                e.StreamOnline.BroadcasterUserName,
                CurrStream.Title,
                CurrStream.StartedAt.ToLocalTime(),
                new(CurrStream.GameId, CurrStream.GameName)
                );
        }

        internal void TwitchResumeStreamOnline(ResumeStreamOnlineEventArgs e)
        {
            HandleOnStreamOnline(
                e.Stream.UserName,
                e.Stream.Title,
                e.Stream.StartedAt.ToLocalTime(),
                new(e.Stream.GameId, e.Stream.GameName)
                );
        }

        internal void TwitchStreamUpdate(NewChannelUpdateEventArgs e)
        {
            HandleOnStreamUpdate(new(e.ChannelUpdate.CategoryId, e.ChannelUpdate.CategoryName));
        }

        public void TwitchCategoryUpdate(OnGetChannelGameNameEventArgs e)
        {
            HandleOnStreamUpdate(new(e.GameId, e.GameName));
        }

        internal void TwitchStreamOffline(NewStreamOfflineEventArgs e)
        {
            HandleOnStreamOffline(Platform.Twitch);
        }

        internal void TwitchNewSubscriber(NewChannelSubscribeEventArgs e)
        {
            HandleNewSubscriber(
                new LiveUser(e.ChannelSubscribe.UserName, Platform.Twitch, e.ChannelSubscribe.UserId),
                "1",
                e.ChannelSubscribe.Tier.Replace("0", ""),
                e.ChannelSubscribe.Tier.Replace("0", ""));
        }

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

        internal void TwitchGiftSubscription(NewChannelSubscribeEventArgs e)
        {
            HandleGiftSubscription(
                new(null, Platform.Twitch, null),
                "1",
                e.ChannelSubscribe.UserName,
                e.ChannelSubscribe.Tier.Replace("0", ""),
                e.ChannelSubscribe.Tier.Replace("0", ""));
        }

        internal void TwitchCommunitySubscription(NewChannelSubscriptionGiftEventArgs e)
        {
            HandleCommunitySubscription(
                e.ChannelSubscriptionGift.IsAnonymous ? new(null, Platform.Twitch, null) : new(e.ChannelSubscriptionGift.UserName, Platform.Twitch, e.ChannelSubscriptionGift.UserId),
                e.ChannelSubscriptionGift.Total,
                e.ChannelSubscriptionGift.Tier.Replace("0", ""));
        }

        public void TwitchCurrentUsers(StreamerOnExistingUserDetectedArgs e)
        {
            HandleUserJoined(e.Users);
        }

        //public void TwitchOnUserJoined(StreamerOnUserJoinedArgs e)
        //{
        //    HandleUserJoined([e.LiveUser]);
        //}

        public void TwitchOnUserLeft(StreamerOnUserLeftArgs e)
        {
            HandleUserLeft(e.LiveUser);
        }

        public void TwitchOnUserTimedout(OnUserTimedoutArgs e = null)
        {
            HandleUserTimedOut(e);
        }

        public void TwitchOnUserBanned(OnUserBannedArgs e = null)
        {
            HandleUserBanned(new(e.UserBan.TargetUserId, Platform.Twitch, e.UserBan.Username));
        }

        public void TwitchMessageReceived(ChannelChatMessageEventArgs e)
        {
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

        public void TwitchIncomingRaid(OnIncomingRaidArgs e)
        {
            HandleIncomingRaidData(e.LiveUser, e.RaidTime, e.ViewerCount, e.Category);
        }

        public void TwitchOutgoingRaid(OnStreamRaidResponseEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "");

            HandleOutgoingRaidData(e.ToChannel, e.CreatedAt, Platform.Twitch);
        }

        public void TwitchChatCommandReceived(ChannelChatMessageEventArgs e)
        {
            string commandtext = "";
            List<string> cmdarglist = [];
            bool foundcommand = false;

            foreach (ChatMessageFragment f in e.ChannelChatMessage.Message.Fragments)
            {
                if (f.Text.StartsWith('!') && !foundcommand)
                {
                    foundcommand = true;
                    commandtext = f.Text[1..].ToLower();
                }
                else if (foundcommand)
                {
                    cmdarglist.Add(f.Text);
                }
            }

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

        public void TwitchBotCommandCall(SendBotCommandEventArgs e)
        {
            HandleChatCommandReceived(e.CmdMessage, Platform.Twitch);
        }

        internal void TwitchChannelPointsRewardRedeemed(NewChannelCustomRewardRedemptionEventArgs e)
        {
            // currently only need the invoking user DisplayName and the reward title, for determining the reward is used for the giveaway.
            // much more data exists in the resulting data output

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, $"Received Twitch Channel Point Reward {e.ChannelPointsCustomRewardRedemption.Reward.Title}. Now processing.");

            HandleCustomReward(
                new(e.ChannelPointsCustomRewardRedemption.UserName, Platform.Twitch, e.ChannelPointsCustomRewardRedemption.UserId),
                e.ChannelPointsCustomRewardRedemption.Reward.Title,
                e.ChannelPointsCustomRewardRedemption.UserInput
                );
        }

        internal void TwitchChannelCheered(NewChannelCheerEventArgs channelCheer)
        {
            HandleChannelCheer(new(channelCheer.ChannelCheer.UserName, Platform.Twitch, channelCheer.ChannelCheer.UserId), channelCheer.ChannelCheer.Bits);
        }

        #endregion

        #region Handle Bot Events

        #region Followers

        public void HandleBotEventNewFollowers(Models.Follow follow)
        {
            Systems.AddNewFollowers([follow]);
        }

        public static void HandleBotEventStartBulkFollowers()
        {
            SystemsController.StartBulkFollowers();
        }

        public static void HandleBotEventBulkPostFollowers(List<Models.Follow> follows)
        {
            SystemsController.UpdateFollowers(follows);
        }

        public static void HandleBotEventStopBulkFollowers()
        {
            SystemsController.StopBulkFollowers();
        }

        #endregion

        #region Clips

        public void HandleBotEventPostNewClip(List<Models.Clip> clips)
        {
            Systems.ClipHelper(clips);
        }

        #endregion

        #region LiveStream

        private void PostGameCategoryEvent(CategoryData categoryData)
        {
            OnStreamCategoryChanged?.Invoke(this, new() { GameId = categoryData.CategoryId, GameName = categoryData.CategoryName });
        }

        private void HandleMultiLiveOnStreamOnline(string userid, string username, string Title, DateTime StartedAt, string GameId, string Category, Platform platform = Platform.Twitch)
        {
            DateTime CurrTime = StartedAt.ToLocalTime();

            // true posted new event, false did not post
            bool PostedLive = SystemsController.DataManage.PostMultiStreamDate(userid, username, Platform.Twitch, CurrTime);

            if (PostedLive)
            {
                bool MultiLive = SystemsController.DataManage.CheckMultiStreamDate(userid, Platform.Twitch, CurrTime);

                if ((OptionFlags.PostMultiLive && MultiLive) || !MultiLive)
                {
                    // get message, set a default if otherwise deleted/unavailable
                    string msg = OptionFlags.MsgLive ?? "@everyone, #user is now live streaming #category - #title! Come join and say hi at: #url";

                    // keys for exchanging codes for representative names
                    Dictionary<string, string> dictionary = new()
                        {
                            { "#user", username },
                            { "#category", Category },
                            { "#title", Title },
                            { "#url", username }
                        };

                    SystemsController.DataManage.PostMultiLiveLog(VariableParser.ParseReplace(msg, dictionary));

                    foreach (Tuple<WebhooksSource, Uri> u in SystemsController.DataManage.GetMultiWebHooks())
                    {
                        if (u.Item1 == WebhooksSource.Discord)
                        {
                            DiscordWebhook.SendMessage(u.Item2,
                                VariableParser.ParseReplace(msg, dictionary),
                                VariableParser.BuildPlatformUrl(username, Platform.Twitch));
                        }
                    }
                }
            }

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

        public void HandleOnStreamOnline(string ChannelName, string Title, DateTime StartedAt, CategoryData Category, Platform platform = Platform.Twitch, bool Debug = false)
        {
            try
            {
                ManageOnlineStream(platform);

                ManageBotsStreamStatusChanged(true);

                bool Started = Systems.StreamOnline(StartedAt);

                if (Started)
                {
                    bool MultiLive = ActionSystem.CheckStreamTime(StartedAt);
                    SystemsController.SetCategory(Category);
                    PostGameCategoryEvent(Category);

                    if (OptionFlags.PostMultiLive && MultiLive || !MultiLive)
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
                            foreach (Tuple<bool, Uri> u in ActionSystem.GetDiscordWebhooks(WebhooksKind.Live))
                            {
                                DiscordWebhook.SendMessage(u.Item2, VariableParser.ParseReplace(TempMsg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                                                                {
                                                                        new(MsgVars.everyone, u.Item1 ? "@everyone" : "")
                                                                }
                                                            )
                                                        ),
                                                        VariableParser.BuildPlatformUrl(ChannelName, platform)
                                                    );
                                Systems.UpdatedStat(StreamStatType.Discord);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void HandleOnStreamUpdate(CategoryData categoryData)
        {
            SystemsController.SetCategory(categoryData);
            PostGameCategoryEvent(categoryData);
        }

        public void HandleOnStreamOffline(Platform platform, string HostedChannel = null, DateTime? RaidTime = null)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Received a livestream offline status update.");

            ManageOfflineStream(platform);

            if (OptionFlags.IsStreamOnline)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Start notifying about the offline stream.");

                ManageBotsStreamStatusChanged(false);

                DateTime currTime = RaidTime?.ToLocalTime() ?? DateTime.Now.ToLocalTime();
                SystemsController.StreamOffline(currTime);
                SystemsController.PostOutgoingRaid(HostedChannel ?? "No Raid", currTime);
                OptionFlags.TwitchOutRaidStarted = false;
            }
        }

        /// <summary>
        /// Call to manage other bots when a monitored stream is detected to be online.
        /// </summary>
        /// <param name="Start">True to start services for stream online, False to stop services for stream offline.</param>
        public static void ManageBotsStreamStatusChanged(bool Start)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Starting any stopped bots or " +
                $"stopping any started bots, based on the current active livestream={Start} status.");

            // loop the bots and send message to start or stop based on stream online or offline status
            foreach (IBotTypes bots in BotsList)
            {
                bots.ManageStreamOnlineOfflineStatus(Start);
            }
        }

        #endregion

        #region Chat Bot

        public void HandleChatBotStarted(Bots Source, EventArgs args)
        {
            lock (StartedChatBots)
            {
                StartedChatBots.UniqueAdd(Source);
            }

            if (StartedChatBots.Count == 1 && args == null)
            {
                SendThread = ThreadManager.CreateThread(
                                                        MethodBase.GetCurrentMethod().Name,
                                                        BeginProcMsgs,
                                                        Priority: ThreadExitPriority.Normal);
                SendThread.Start();
                Systems.NotifyBotStart();
            }
        }

        public void HandleChatBotStopping(Bots Source, EventArgs args)
        {
            lock (StartedChatBots)
            {
                StartedChatBots.RemoveAll((s) => s == Source);
            }

            if (StartedChatBots.Count == 0 && args == null)
            {
                Systems.NotifyBotStop();
            }
            ChatBotStopping = true;
        }

        public void HandleChatBotStopped(Bots Source, EventArgs args)
        {
            if (Source == Bots.TwitchBotEventSub && args == null)
            {
                ChatBotStopping = false;
            }
        }

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
                Send(ParsedMsg, Multi);

            }

            Systems.UpdatedStat(StreamStatType.Sub, StreamStatType.AutoEvents);

            Systems.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Subscribe, User, UserMsg: HTMLParsedMsg);
            SystemsController.AddNewOverlayTickerItem(OverlayTickerItem.LastSubscriber, User.UserName);
        }

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
                Send(ParsedMsg, Multi);
            }
            Systems.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Resubscribe, User, UserMsg: HTMLParsedMsg);

            Systems.UpdatedStat(StreamStatType.Sub, StreamStatType.AutoEvents);
            SystemsController.AddNewOverlayTickerItem(OverlayTickerItem.LastSubscriber, User.UserName);
        }

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
                Send(ParsedMsg, Multi);
            }
            Systems.UpdatedStat(StreamStatType.GiftSubs, StreamStatType.AutoEvents);
            Systems.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.GiftSub, User, UserMsg: HTMLParsedMsg);
            SystemsController.AddNewOverlayTickerItem(OverlayTickerItem.LastGiftSub, User.UserName);
            //            SystemsController.AddNewOverlayTickerItem(OverlayTickerItem.LastSubscriber, RecipientUserName);
        }

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
                Send(ParsedMsg, Multi);
            }

            Systems.UpdatedStat(StreamStatType.GiftSubs, SubCount);
            Systems.UpdatedStat(StreamStatType.AutoEvents);

            Systems.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.CommunitySubs, User, UserMsg: HTMLParsedMsg);
            SystemsController.AddNewOverlayTickerItem(OverlayTickerItem.LastGiftSub, User.UserName ?? "anonymous");
        }

        public void HandleChannelCheer(LiveUser user, int Bits)
        {
            Systems.UserCheered(user, Bits);
        }

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
                Send(ParsedMsg, Multi);
            }

            Systems.UpdatedStat(StreamStatType.Hosted, StreamStatType.AutoEvents);
            Systems.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.BeingHosted, User, UserMsg: HTMLParsedMsg);
        }

        public void HandleUserJoined(List<Models.LiveUser> Users)
        {
            Systems.UserJoined(Users);
        }

        public void HandleUserLeft(Models.LiveUser User)
        {
            Systems.UserLeft(User);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Calling method invokes this method and provides event arg parameter")]
        public void HandleUserTimedOut(OnUserTimedoutArgs e)
        {
            Systems.UpdatedStat(StreamStatType.UserTimedOut);
        }

        public void HandleUserBanned(LiveUser User)
        {
            try
            {
                Systems.UpdatedStat(StreamStatType.UserBanned);
                HandleUserLeft(User);

                Systems.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.BannedUser, User);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void HandleAddChat(string UserName, Platform Source)
        {
            Systems.UserJoined([new(UserName, Source)]);
        }

        public void HandleMessageReceived(Models.CmdMessage MsgReceived, Platform Source)
        {
            Systems.MessageReceived(MsgReceived, new(MsgReceived.DisplayName, Source, MsgReceived.UserId));
        }

        public void HandleIncomingRaidData(Models.LiveUser User, DateTime RaidTime, int ViewerCount, CategoryData Category)
        {
            Systems.PostIncomingRaid(User, RaidTime.ToLocalTime(), ViewerCount, Category);
        }

        public void HandleOutgoingRaidData(string ToChannelName, DateTime RaidTime, Platform platform)
        {
            HandleOnStreamOffline(platform, ToChannelName, RaidTime);
        }

        public void HandleChatCommandReceived(Models.CmdMessage commandmsg, Platform Source)
        {
            if (GiveawayItemType == GiveawayTypes.Command && commandmsg.CommandText == GiveawayItemName)
            {
                HandleGiveawayPostName(new(commandmsg.DisplayName, Source, commandmsg.UserId));
            }
            Systems.ProcessCommand(commandmsg, Source);
        }

        public void HandleCustomReward(LiveUser User, string RewardTitle, string RewardMsg)
        {
            if (GiveawayItemType == GiveawayTypes.CustomRewards && RewardTitle == GiveawayItemName)
            {
                HandleGiveawayPostName(User);
            }

            Tuple<string, string> approval = SystemsController.GetApprovalRule(ModActionType.ChannelPoints, RewardTitle);

            if (approval != null)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, $"Custom reward {RewardTitle} requires approval.");

                switch (User.Platform)
                {
                    case Platform.Twitch:
                        Systems.PostApproval($"{approval.Item2} {User.UserName} {RewardMsg}",
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
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"Checking Channel Point Redemption {RewardTitle} for an Overlay request.");

                Systems.CheckForOverlayEvent(OverlayTypes.ChannelPoints, RewardTitle, User);
            }
        }

        #region Giveaway
        public void HandleGiveawayBegin(GiveawayTypes giveawayTypes, string ItemName)
        {
            GiveawayItemType = giveawayTypes;
            GiveawayItemName = ItemName;
            GiveawayStarted = true;

            Systems.BeginGiveaway();
        }

        public void HandleGiveawayEnd()
        {
            Systems.EndGiveaway();

            GiveawayItemType = GiveawayTypes.None;
            GiveawayItemName = "";
            GiveawayStarted = false;
        }

        public void HandleGiveawayPostName(LiveUser User)
        {
            Systems.ManageGiveaway(User);
        }

        public void HandleGiveawayWinner()
        {
            if (GiveawayStarted)
            {
                HandleGiveawayEnd();
            }
            Systems.PostGiveawayResult();
        }

        public void ActivateRepeatTimers()
        {
            Systems.ActivateRepeatTimers();
        }

        #endregion

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

        #region MediaOverlay Server

        /// <summary>
        /// Connect the Overlay System event notification to the Overlay Server bot to process any new Overlay actions detected.
        /// </summary>
        private void SetNewOverlayEventHandler()
        {
            Systems.SetNewOverlayEventHandler(
                OverlayServerBot.NewOverlayEventHandler,
                OverlayServerBot.UpdatedTickerEventHandler
                );
        }

        #endregion

        #endregion

    }
}
