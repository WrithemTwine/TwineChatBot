using StreamerBotLib.BotClients;
using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.PubSub;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Threading;

using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;
using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;


// TODO: Add Bot contacts users to invoke conversation; carry-on conversation with existing

namespace StreamerBotLib.BotIOController
{
    public class BotController
    {
        public event EventHandler<PostChannelMessageEventArgs> OutputSentToBots;
        public event EventHandler<OnGetChannelGameNameEventArgs> OnStreamCategoryChanged;
        public event EventHandler<InvalidAccessTokenEventArgs> InvalidAuthorizationToken;

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
            ActionSystem.BotUserName = TwitchBotsBase.TwitchBotUserName;
            ActionSystem.ChannelName = TwitchBotsBase.TwitchChannelName;
            OutputSentToBots += ActionSystem.OutputSentToBotsHandler;

            SetNewOverlayEventHandler();

            BotsList.Add(TwitchBots);
            BotsList.Add(OverlayServerBot);

            TwitchBots.InvalidTwitchAccess += TwitchBots_InvalidTwitchAccess;
        }

        /// <summary>
        /// Initializes a Helix api.
        /// </summary>
        public static void InitializeHelix()
        {
            BotsTwitch.InitializeHelix();
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
            AppDispatcher.Invoke(() =>
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Event, {e.MethodName}, received from bots to post into system.");

                try
                {
                    if (e.MethodName.Contains('.'))
                    {
                        throw new ArgumentException("Method name to invoke contains '.', which is invalid.");
                    }

                    _ = typeof(BotController).InvokeMember(
                            name: e.MethodName,
                            invokeAttr: BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.OptionalParamBinding,
                            binder: null,
                            target: this,
                            args: e.e == null ? null : [e.e],
                            culture: null);

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

                TwitchStreamOffline();

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Sending an exit to the data system.");

                Systems.Exit();

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Waiting for any queued messages to finish sending to the the channel.");

                SendThread?.Join(); // wait until all the messages are sent to ask bots to close

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Stopping bots.");

                foreach (IBotTypes bot in BotsList)
                {
                    bot.StopBots();
                }

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Closing/Exiting all of the output logs, including me!");
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
            if (OptionFlags.ManageFollowers)
            {
                foreach (IBotTypes bot in BotsList)
                {
                    bot.GetAllFollowers();
                }
            }
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
        /// Send a "Set Discord Webhooks Enabled" toggle request to the system database.
        /// </summary>
        /// <param name="Enabled">True or False to set Discord Webhooks in bulk.</param>
        public static void SetDiscordWebhooksEnabled(bool Enabled)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received a \"Set Discord Webhooks Enabled\" " +
                $"request to set all events to {Enabled}.");

            SystemsController.SetDiscordWebhooksEnabled(Enabled);
        }

        /// <summary>
        /// Insert a new AutoShoutUser entry into the database.
        /// </summary>
        /// <param name="UserName">The username to add into the database for the autoshout table.</param>
        public static void AddNewAutoShoutUser(string UserName, string Userid, string platform)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, $"Received an \"Add New Auto Shout User\" " +
                $"request to add= {UserName} =to the database.");

            SystemsController.AddNewAutoShoutUser(UserName, Userid, platform);
        }

        #endregion

        #region Query Bots

        public void CheckTwitchChannelBotIds()
        {
            BotsTwitch.CheckStreamerBotIds();
        }

        /// <summary>
        /// Part of the Twitch-Auth-Code Token operation method.
        /// Call to clear out the Twitch Authorization Code(s) to permit the user to re-authorize the application.
        /// </summary>
        public void ForceTwitchAuthReauthorization()
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

                return BotsTwitch.GetUserCategory(UserId: UserId, UserName: ChannelName);
            }
            else
            {
                return "";
            }
            //return bots switch
            //{
            //    Bots.TwitchUserBot or Bots.TwitchChatBot => c,
            //    Bots.Default => throw new NotImplementedException(),
            //    Bots.TwitchLiveBot => throw new NotImplementedException(),
            //    Bots.TwitchFollowBot => throw new NotImplementedException(),
            //    Bots.TwitchClipBot => throw new NotImplementedException(),
            //    Bots.TwitchMultiBot => throw new NotImplementedException(),
            //    Bots.TwitchPubSub => throw new NotImplementedException(),
            //    _ => ""
            //};
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

                ThreadManager.CreateThreadStart(() =>
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
        /// <param name="OpenBrowser"></param>
        /// <param name="AuthenticationFinished">A callback method once the bot concludes using the auth code to get an access/refresh token.</param>
        public static void TwitchTokenAuthCodeAuthorize(string clientId, Action<string> OpenBrowser, Action AuthenticationFinished)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Received request to activate " +
                "a Twitch authorization code for a specific client Id, which returns an initial access token and a refresh token to begin accessing Twitch.");

            BotsTwitch.TwitchActivateAuthCode(clientId, OpenBrowser, AuthenticationFinished);
        }

        /// <summary>
        /// Interface method to ask the bot(s) to create a clip; which asks Twitch to create a clip.
        /// </summary>
        public static void CreateClip()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Recieved a request to create a Twitch clip.");

            BotsTwitch.CreateClip();
        }

        #endregion

        #region Twitch Bot Events

        public static void ConnectTwitchMultiLive()
        {
            BotsTwitch.LiveMonitorSvc.MultiConnect();
        }

        public static void DisconnectTwitchMultiLive()
        {
            BotsTwitch.LiveMonitorSvc.MultiDisconnect();
        }

        public static void StartTwitchMultiLive()
        {
            BotsTwitch.LiveMonitorSvc.StartMultiLive();
        }

        public static void StopTwitchMultiLive()
        {
            BotsTwitch.LiveMonitorSvc.StopMultiLive();
        }

        public static void UpdateTwitchMultiLiveChannels()
        {
            BotsTwitch.LiveMonitorSvc.UpdateChannels();
        }

        public void TwitchStartUpdateAllFollowers(bool OverrideUpdateFollows = false)
        {
            TwitchBots.GetAllFollowers(OverrideUpdateFollows);
        }

        public void TwitchPostNewFollowers(OnNewFollowersDetectedArgs Follower)
        {
            HandleBotEventNewFollowers(ConvertFollowers(Follower.NewFollowers, Platform.Twitch));
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
                    Source
                );
            });
        }

        public void TwitchChatBotStarted(EventArgs args = null)
        {
            HandleChatBotStarted(Bots.TwitchChatBot, args);
        }

        public void TwitchChatBotStopping(EventArgs args = null)
        {
            HandleChatBotStopped(Bots.TwitchChatBot, args);
        }

        public void TwitchChatBotStopped(EventArgs args = null)
        {
            HandleChatBotStopped(Bots.TwitchChatBot, args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Calling method invokes this method and provides event arg parameter")]
        public static void TwitchStartBulkFollowers(EventArgs args = null)
        {
            HandleBotEventStartBulkFollowers();
        }

        public static void TwitchBulkPostFollowers(OnNewFollowersDetectedArgs Follower)
        {
            HandleBotEventBulkPostFollowers(ConvertFollowers(Follower.NewFollowers, Platform.Twitch));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Calling method invokes this method and provides event arg parameter")]
        public static void TwitchStopBulkFollowers(EventArgs args = null)
        {
            HandleBotEventStopBulkFollowers();
        }

        public void TwitchClipSvcOnClipFound(ClipFoundEventArgs clips)
        {
            HandleBotEventPostNewClip(ConvertClips(clips.ClipList));
        }

        public static List<Models.Clip> ConvertClips(List<Clip> clips)
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
                    Url = SrcClip.Url
                };
            });
        }

        public void TwitchPostNewClip(OnNewClipsDetectedArgs clips)
        {
            HandleBotEventPostNewClip(ConvertClips(clips.Clips));
        }

        public void TwitchStreamOnline(OnStreamOnlineArgs e)
        {
            HandleOnStreamOnline(e.Stream.UserName, e.Stream.Title, e.Stream.StartedAt.ToLocalTime(), e.Stream.GameId, e.Stream.GameName);
        }

        public void TwitchStreamUpdate(OnStreamUpdateArgs e)
        {
            HandleOnStreamUpdate(e.Stream.GameId, e.Stream.GameName);
        }

        public void TwitchCategoryUpdate(OnGetChannelGameNameEventArgs e)
        {
            HandleOnStreamUpdate(e.GameId, e.GameName);
        }

        public static void TwitchStreamOffline(OnStreamOfflineArgs e = null)
        {
            if (!OptionFlags.TwitchOutRaidStarted && e == null)
            {
                HandleOnStreamOffline();
            }
        }

        public void TwitchNewSubscriber(OnNewSubscriberArgs e)
        {
            HandleNewSubscriber(
                e.Subscriber.DisplayName,
                e.Subscriber.MsgParamCumulativeMonths,
                e.Subscriber.SubscriptionPlan.ToString(),
                e.Subscriber.SubscriptionPlanName);
        }

        public void TwitchReSubscriber(OnReSubscriberArgs e)
        {
            HandleReSubscriber(
                e.ReSubscriber.DisplayName,
                e.ReSubscriber.Months,
                e.ReSubscriber.MsgParamCumulativeMonths,
                e.ReSubscriber.SubscriptionPlan.ToString(),
                e.ReSubscriber.SubscriptionPlanName,
                e.ReSubscriber.MsgParamShouldShareStreak,
                e.ReSubscriber.MsgParamStreakMonths);
        }

        public void TwitchGiftSubscription(OnGiftedSubscriptionArgs e)
        {
            HandleGiftSubscription(
                e.GiftedSubscription.DisplayName,
                e.GiftedSubscription.MsgParamMonths,
                e.GiftedSubscription.MsgParamRecipientUserName,
                e.GiftedSubscription.MsgParamSubPlan.ToString(),
                e.GiftedSubscription.MsgParamSubPlanName);
        }

        public void TwitchCommunitySubscription(OnCommunitySubscriptionArgs e)
        {
            HandleCommunitySubscription(
                e.GiftedSubscription.DisplayName,
                e.GiftedSubscription.MsgParamSenderCount,
                e.GiftedSubscription.MsgParamSubPlan.ToString());
        }

        public void TwitchExistingUsers(StreamerOnExistingUserDetectedArgs e)
        {
            HandleUserJoined(e.Users);
        }

        public void TwitchOnUserJoined(StreamerOnUserJoinedArgs e)
        {
            HandleUserJoined([e.LiveUser]);
        }

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
            HandleUserBanned(e.UserBan.Username, Platform.Twitch);
        }

        public void TwitchMessageReceived(OnMessageReceivedArgs e)
        {
            HandleMessageReceived(
                new()
                {
                    DisplayName = e.ChatMessage.DisplayName,
                    Channel = e.ChatMessage.Channel,
                    IsBroadcaster = e.ChatMessage.IsBroadcaster,
                    IsHighlighted = e.ChatMessage.IsHighlighted,
                    IsMe = e.ChatMessage.IsMe,
                    IsModerator = e.ChatMessage.IsModerator,
                    IsPartner = e.ChatMessage.IsPartner,
                    IsSkippingSubMode = e.ChatMessage.IsSkippingSubMode,
                    IsStaff = e.ChatMessage.IsStaff,
                    IsSubscriber = e.ChatMessage.IsSubscriber,
                    IsTurbo = e.ChatMessage.IsTurbo,
                    IsVip = e.ChatMessage.IsVip,
                    Message = e.ChatMessage.Message,
                    Bits = e.ChatMessage.Bits
                }
                , Platform.Twitch);
        }

        public void TwitchIncomingRaid(OnIncomingRaidArgs e)
        {
            HandleIncomingRaidData(new(e.DisplayName, Platform.Twitch), e.RaidTime, e.ViewerCount, e.Category);
        }

        public static void TwitchOutgoingRaid(OnStreamRaidResponseEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "");

            HandleOutgoingRaidData(e.ToChannel, e.CreatedAt);
        }

        public void TwitchChatCommandReceived(OnChatCommandReceivedArgs e)
        {
            HandleChatCommandReceived(new()
            {
                CommandArguments = e.Command.ArgumentsAsList,
                CommandText = e.Command.CommandText.ToLower(),
                DisplayName = e.Command.ChatMessage.DisplayName,
                Channel = e.Command.ChatMessage.Channel,
                IsBroadcaster = e.Command.ChatMessage.IsBroadcaster,
                IsHighlighted = e.Command.ChatMessage.IsHighlighted,
                IsMe = e.Command.ChatMessage.IsMe,
                IsModerator = e.Command.ChatMessage.IsModerator,
                IsPartner = e.Command.ChatMessage.IsPartner,
                IsSkippingSubMode = e.Command.ChatMessage.IsSkippingSubMode,
                IsStaff = e.Command.ChatMessage.IsStaff,
                IsSubscriber = e.Command.ChatMessage.IsSubscriber,
                IsTurbo = e.Command.ChatMessage.IsTurbo,
                IsVip = e.Command.ChatMessage.IsVip,
                Message = e.Command.ChatMessage.Message
            }, Platform.Twitch);
        }

        public void TwitchBotCommandCall(SendBotCommandEventArgs e)
        {
            HandleChatCommandReceived(e.CmdMessage, Platform.Twitch);
        }

        public void TwitchChannelPointsRewardRedeemed(OnChannelPointsRewardRedeemedArgs e)
        {
            // currently only need the invoking user DisplayName and the reward title, for determining the reward is used for the giveaway.
            // much more data exists in the resulting data output

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, $"Received Twitch Channel Point Reward \"{e.RewardRedeemed.Redemption.Reward.Title}\". Now processing.");

            HandleCustomReward(e.RewardRedeemed.Redemption.User.DisplayName, e.RewardRedeemed.Redemption.Reward.Title, e.RewardRedeemed.Redemption.UserInput, Platform.Twitch);
        }

        #endregion

        #region Handle Bot Events

        /// <summary>
        /// Empty non-event just to satisfy the "BotEvent" handler through the bot interface - some bots don't call this event handler.
        /// </summary>
        public static void HandleBotEventEmpty()
        {
            // I do nothing else.
        }

        #region Followers

        public void HandleBotEventNewFollowers(List<Models.Follow> follows)
        {
            Systems.AddNewFollowers(follows);
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

        private void PostGameCategoryEvent(string GameId, string GameName)
        {
            OnStreamCategoryChanged?.Invoke(this, new() { GameId = GameId, GameName = GameName });
        }

        public void HandleOnStreamOnline(string ChannelName, string Title, DateTime StartedAt, string GameId, string Category, Platform platform = Platform.Twitch, bool Debug = false)
        {
            try
            {
                ManageBotsStreamStatusChanged(true);

                bool Started = Systems.StreamOnline(StartedAt);
                ActionSystem.ChannelName = ChannelName;

                if (Started)
                {
                    bool MultiLive = ActionSystem.CheckStreamTime(StartedAt);
                    SystemsController.SetCategory(GameId, Category);
                    PostGameCategoryEvent(GameId, Category);

                    if (OptionFlags.PostMultiLive && MultiLive || !MultiLive)
                    {
                        // get message, set a default if otherwise deleted/unavailable
                        string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out bool Enabled, out _);

                        // keys for exchanging codes for representative names
                        Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                        {
                                new(MsgVars.user, ChannelName),
                                new(MsgVars.category, Category),
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

        public void HandleOnStreamUpdate(string gameId, string gameName)
        {
            SystemsController.SetCategory(gameId, gameName);
            PostGameCategoryEvent(gameId, gameName);
        }

        public static void HandleOnStreamOffline(string HostedChannel = null, DateTime? RaidTime = null)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.BotController, "Received a livestream offline status update.");

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
                SendThread = ThreadManager.CreateThread(BeginProcMsgs, Priority: ThreadExitPriority.Normal);
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
            if (Source == Bots.TwitchChatBot && args == null)
            {
                ChatBotStopping = false;
            }
        }

        public void HandleNewSubscriber(string DisplayName, string Months, string Subscription, string SubscriptionName)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out bool Enabled, out short Multi);

            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                new( MsgVars.user, DisplayName ),
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

            Systems.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Subscribe, DisplayName, UserMsg: HTMLParsedMsg);
            SystemsController.AddNewOverlayTickerItem(OverlayTickerItem.LastSubscriber, DisplayName);
        }

        public void HandleReSubscriber(string DisplayName, int Months, string TotalMonths, string Subscription, string SubscriptionName, bool ShareStreak, string StreakMonths)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out bool Enabled, out short Multi);
            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                new( MsgVars.user, DisplayName ),
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
            Systems.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Resubscribe, DisplayName, UserMsg: HTMLParsedMsg);

            Systems.UpdatedStat(StreamStatType.Sub, StreamStatType.AutoEvents);
            SystemsController.AddNewOverlayTickerItem(OverlayTickerItem.LastSubscriber, DisplayName);
        }

        public void HandleGiftSubscription(string DisplayName, string Months, string RecipientUserName, string Subscription, string SubscriptionName)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.GiftSub, out bool Enabled, out short Multi);
            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                    new(MsgVars.user,DisplayName),
                    new(MsgVars.months, FormatData.Plurality(Months, MsgVars.Pluralmonth)),
                    new(MsgVars.receiveuser, RecipientUserName ),
                    new(MsgVars.subplan, Subscription ),
                    new(MsgVars.subplanname, SubscriptionName)
                });

            string ParsedMsg = VariableParser.ParseReplace(msg, dictionary);
            string HTMLParsedMsg = VariableParser.ParseReplace(msg, dictionary, true);
            if (Enabled)
            {
                Send(ParsedMsg, Multi);
            }
            Systems.UpdatedStat(StreamStatType.GiftSubs, StreamStatType.AutoEvents);
            Systems.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.GiftSub, DisplayName, UserMsg: HTMLParsedMsg);
            SystemsController.AddNewOverlayTickerItem(OverlayTickerItem.LastGiftSub, DisplayName);
            //            SystemsController.AddNewOverlayTickerItem(OverlayTickerItem.LastSubscriber, RecipientUserName);
        }

        public void HandleCommunitySubscription(string DisplayName, int SubCount, string Subscription)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.CommunitySubs, out bool Enabled, out short Multi);
            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                    new(MsgVars.user, DisplayName),
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

            Systems.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.CommunitySubs, DisplayName, UserMsg: HTMLParsedMsg);
            SystemsController.AddNewOverlayTickerItem(OverlayTickerItem.LastGiftSub, DisplayName);
        }

        public void HandleBeingHosted(string HostedByChannel, bool IsAutoHosted, int Viewers)
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
            Systems.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.BeingHosted, UserName: HostedByChannel, UserMsg: HTMLParsedMsg);
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

        public void HandleUserBanned(string UserName, Platform Source)
        {
            try
            {
                Systems.UpdatedStat(StreamStatType.UserBanned);
                HandleUserLeft(new(UserName, Source));

                Systems.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.BannedUser, UserName: UserName);
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
            Systems.MessageReceived(MsgReceived, new(MsgReceived.DisplayName, Source));
        }

        public void HandleIncomingRaidData(Models.LiveUser User, DateTime RaidTime, string ViewerCount, string Category)
        {
            Systems.PostIncomingRaid(User, RaidTime.ToLocalTime(), ViewerCount, Category);
        }

        public static void HandleOutgoingRaidData(string ToChannelName, DateTime RaidTime)
        {
            HandleOnStreamOffline(ToChannelName, RaidTime);
        }

        public void HandleChatCommandReceived(Models.CmdMessage commandmsg, Platform Source)
        {
            if (GiveawayItemType == GiveawayTypes.Command && commandmsg.CommandText == GiveawayItemName)
            {
                HandleGiveawayPostName(commandmsg.DisplayName);
            }
            Systems.ProcessCommand(commandmsg, Source);
        }

        public void HandleCustomReward(string DisplayName, string RewardTitle, string RewardMsg, Platform Source)
        {
            if (GiveawayItemType == GiveawayTypes.CustomRewards && RewardTitle == GiveawayItemName)
            {
                HandleGiveawayPostName(DisplayName);
            }

            Tuple<string, string> approval = SystemsController.GetApprovalRule(ModActionType.ChannelPoints, RewardTitle);

            if (approval != null)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, $"Custom reward {RewardTitle} requires approval.");

                switch (Source)
                {
                    case Platform.Twitch:
                        Systems.PostApproval($"{approval.Item2} {DisplayName} {RewardMsg}",
                            new(() =>
                            {
                                TwitchBots.PostInternalCommand(approval.Item2, [DisplayName, RewardMsg], $"!{approval.Item2} {DisplayName} {RewardMsg}");
                            })
                        );

                        TwitchBots.PostInternalCommand(LocalizedMsgSystem.GetVar(DefaultCommand.approve), [], $"!{LocalizedMsgSystem.GetVar(DefaultCommand.approve)}");
                        break;
                }
            }

            if (OptionFlags.MediaOverlayChannelPoints)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"Checking Channel Point Redemption {RewardTitle} for an Overlay request.");

                Systems.CheckForOverlayEvent(OverlayTypes.ChannelPoints, RewardTitle, DisplayName);
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

        public void HandleGiveawayPostName(string DisplayName)
        {
            Systems.ManageGiveaway(DisplayName);
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
            if (e.User.Source == Platform.Twitch)
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
