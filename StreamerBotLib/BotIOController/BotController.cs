using StreamerBotLib.BotClients;
using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.PubSub;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;


// TODO: Thread Manager - include count total threads created
// TODO: Add Bot contacts users to invoke conversation; carry-on conversation with existing
// TODO: add streaming category count, track number of streams per category 

namespace StreamerBotLib.BotIOController
{
    public class BotController
    {
        public event EventHandler<PostChannelMessageEventArgs> OutputSentToBots;
        public event EventHandler<OnGetChannelGameNameEventArgs> OnStreamCategoryChanged;

        private Dispatcher AppDispatcher { get; set; }
        public SystemsController Systems { get; private set; }
        internal Collection<IBotTypes> BotsList { get; private set; } = new();

        private GiveawayTypes GiveawayItemType = GiveawayTypes.None;
        private string GiveawayItemName = "";
        private bool GiveawayStarted = false;

        private BotsTwitch TwitchBots { get; set; }

        private const int SendMsgDelay = 750;
        // 600ms between messages, permits about 100 messages max in 60 seconds == 1 minute
        // 759ms between messages, permits about 80 messages max in 60 seconds == 1 minute
        private Queue<Task> Operations { get; set; } = new();   // an ordered list, enqueue into one end, dequeue from other end
        private readonly Thread SendThread;  // the thread for sending messages back to the monitored  channels

        public BotController()
        {
            OptionFlags.ActiveToken = true;

            Systems = new();
            Systems.PostChannelMessage += Systems_PostChannelMessage;

            TwitchBots = new();
            TwitchBots.BotEvent += HandleBotEvent;
            SystemsBase.BotUserName = TwitchBotsBase.TwitchBotUserName;
            SystemsBase.ChannelName = TwitchBotsBase.TwitchChannelName;
            OutputSentToBots += SystemsBase.OutputSentToBotsHandler;

            BotsList.Add(TwitchBots);


            SendThread = new(new ThreadStart(BeginProcMsgs));
            SendThread.Start();
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
                _ = typeof(BotController).InvokeMember(name: e.MethodName, invokeAttr: BindingFlags.InvokeMethod, binder: null, target: this, args: new[] { e.e }, culture: CultureInfo.CurrentCulture);
            });
        }

        /// <summary>
        /// Captures send events from the systems object to send to every bot with a send method. Some bots don't have 'send' implemented, so the message only sends for bots implementing send.
        /// </summary>
        /// <param name="sender">Unused - object invoking the event.</param>
        /// <param name="e">Contains the message to send to the bots.</param>
        private void Systems_PostChannelMessage(object sender, PostChannelMessageEventArgs e)
        {
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
            while (OptionFlags.ActiveToken || Operations.Count > 0)
            {
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
                Systems.Exit();

                SendThread.Join(); // wait until all the messages are sent to ask bots to close

                foreach (IBotTypes bot in BotsList)
                {
                    bot.StopBots();
                }
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
        public void ManageDatabase()
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

        public static void ClearWatchTime()
        {
            SystemsController.ClearWatchTime();
        }

        public static void ClearAllCurrenciesValues()
        {
            SystemsController.ClearAllCurrenciesValues();
        }

        public static void SetSystemEventsEnabled(bool Enabled)
        {
            SystemsController.SetSystemEventsEnabled(Enabled);
        }

        public static void SetBuiltInCommandsEnabled(bool Enabled)
        {
            SystemsController.SetBuiltInCommandsEnabled(Enabled);
        }

        public static void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            SystemsController.SetUserDefinedCommandsEnabled(Enabled);
        }

        public static void SetDiscordWebhooksEnabled(bool Enabled)
        {
            SystemsController.SetDiscordWebhooksEnabled(Enabled);
        }

        public static string GetUserCategory(string ChannelName, Bots bots)
        {
            return bots switch
            {
                Bots.TwitchUserBot or Bots.TwitchChatBot => BotsTwitch.GetUserCategory(UserName: ChannelName),
                Bots.Default => throw new NotImplementedException(),
                Bots.TwitchLiveBot => throw new NotImplementedException(),
                Bots.TwitchFollowBot => throw new NotImplementedException(),
                Bots.TwitchClipBot => throw new NotImplementedException(),
                Bots.TwitchMultiBot => throw new NotImplementedException(),
                Bots.TwitchPubSub => throw new NotImplementedException(),
                _ => ""
            };
        }

        public static bool VerifyUserExist(string ChannelName, Bots bots)
        {
            return bots switch
            {
                Bots.TwitchChatBot or Bots.TwitchUserBot => BotsTwitch.VerifyUserExist(ChannelName),
                Bots.Default => throw new NotImplementedException(),
                Bots.TwitchLiveBot => throw new NotImplementedException(),
                Bots.TwitchFollowBot => throw new NotImplementedException(),
                Bots.TwitchClipBot => throw new NotImplementedException(),
                Bots.TwitchMultiBot => throw new NotImplementedException(),
                Bots.TwitchPubSub => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

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

        public void TwitchPostNewFollowers(OnNewFollowersDetectedArgs Follower)
        {
            HandleBotEventNewFollowers(ConvertFollowers(Follower.NewFollowers));
        }

        /// <summary>
        /// Convert from Twitch Follower objects to generic "Models.Follow" objects.
        /// </summary>
        /// <param name="follows">The Twitch follows list to convert.</param>
        /// <returns>The follower list converted to the generic "Models.Follow" list.</returns>
        private static List<Models.Follow> ConvertFollowers(List<Follow> follows)
        {
            return follows.ConvertAll((f) =>
            {
                return new Models.Follow()
                {
                    FollowedAt = f.FollowedAt.ToLocalTime(),
                    FromUserId = f.FromUserId,
                    FromUserName = f.FromUserName,
                    ToUserId = f.ToUserId,
                    ToUserName = f.ToUserName
                };
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Calling method invokes this method and provides event arg parameter")]
        public static void TwitchStartBulkFollowers(EventArgs args = null)
        {
            HandleBotEventStartBulkFollowers();
        }

        public static void TwitchBulkPostFollowers(OnNewFollowersDetectedArgs Follower)
        {
            HandleBotEventBulkPostFollowers(ConvertFollowers(Follower.NewFollowers));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Calling method invokes this method and provides event arg parameter")]
        public static void TwitchStopBulkFollowers(EventArgs args = null)
        {
            HandleBotEventStopBulkFollowers();
        }

        public void TwitchClipSvcOnClipFound(ClipFoundEventArgs clips)
        {
            ConvertClips(clips.ClipList);
        }

        private void ConvertClips(List<Clip> clips)
        {
            HandleBotEventPostNewClip(clips.ConvertAll((SrcClip) =>
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
            }));
        }

        public void TwitchPostNewClip(OnNewClipsDetectedArgs clips)
        {
            ConvertClips(clips.Clips);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Calling method invokes this method and provides event arg parameter")]
        public static void TwitchStreamOffline(OnStreamOfflineArgs e = null)
        {
            HandleOnStreamOffline();
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

        public void TwitchBeingHosted(OnBeingHostedArgs e)
        {
            HandleBeingHosted(e.BeingHostedNotification.HostedByChannel, e.BeingHostedNotification.IsAutoHosted, e.BeingHostedNotification.Viewers);
        }

        public static void TwitchNowHosting(OnNowHostingArgs e)
        {
            HandleOnStreamOffline(HostedChannel: e.HostedChannel);
        }

        public void TwitchExistingUsers(OnExistingUsersDetectedArgs e)
        {
            HandleUserJoined(e.Users, Bots.TwitchChatBot);
        }

        public void TwitchOnUserJoined(OnUserJoinedArgs e)
        {
            HandleUserJoined(new() { e.Username }, Bots.TwitchChatBot);
        }

        public void TwitchOnUserLeft(OnUserLeftArgs e)
        {
            HandleUserLeft(e.Username);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Calling method invokes this method and provides event arg parameter")]
        public void TwitchOnUserTimedout(OnUserTimedoutArgs e = null)
        {
            HandleUserTimedOut(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Calling method invokes this method and provides event arg parameter")]
        public void TwitchOnUserBanned(OnUserBannedArgs e = null)
        {
            HandleUserBanned(e.UserBan.Username);
        }

        public void TwitchRitualNewChatter(OnRitualNewChatterArgs e)
        {
            HandleAddChat(e.RitualNewChatter.DisplayName, Bots.TwitchChatBot);
        }

        public void TwitchMessageReceived(OnMessageReceivedArgs e)
        {
            HandleMessageReceived(e.ChatMessage.DisplayName, e.ChatMessage.IsSubscriber, e.ChatMessage.IsVip, e.ChatMessage.IsModerator, e.ChatMessage.Bits, e.ChatMessage.Message, Bots.TwitchChatBot);
        }

        public void TwitchIncomingRaid(OnIncomingRaidArgs e)
        {
            HandleIncomingRaidData(e.DisplayName, e.RaidTime, e.ViewerCount, e.Category, Bots.TwitchChatBot);
        }

        public void TwitchChatCommandReceived(OnChatCommandReceivedArgs e)
        {
            HandleChatCommandReceived(new()
            {
                CommandArguments = e.Command.ArgumentsAsList,
                CommandText = e.Command.CommandText,
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
            }, Bots.TwitchChatBot); ;
        }


        public void TwitchChannelPointsRewardRedeemed(OnChannelPointsRewardRedeemedArgs e)
        {
            // currently only need the invoking user DisplayName and the reward title, for determining the reward is used for the giveaway.
            // much more data exists in the resulting data output

            HandleCustomReward(e.RewardRedeemed.Redemption.User.DisplayName, e.RewardRedeemed.Redemption.Reward.Title);
        }

        #endregion

        #region Handle Bot Events

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

        public void HandleOnStreamOnline(string ChannelName, string Title, DateTime StartedAt, string GameId, string Category, bool Debug = false)
        {
            try
            {
                bool Started = Systems.StreamOnline(StartedAt);
                SystemsBase.ChannelName = ChannelName;

                if (Started)
                {
                    bool MultiLive = StatisticsSystem.CheckStreamTime(StartedAt);
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
                            foreach (Tuple<bool, Uri> u in StatisticsSystem.GetDiscordWebhooks(WebhooksKind.Live))
                            {
                                DiscordWebhook.SendMessage(u.Item2, VariableParser.ParseReplace(TempMsg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                                                                {
                                                                        new(MsgVars.everyone, u.Item1 ? "@everyone" : "")
                                                                }
                                                            )
                                                        )
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

        public static void HandleOnStreamOffline(string HostedChannel = null)
        {
            if (OptionFlags.IsStreamOnline)
            {
                DateTime currTime = DateTime.Now.ToLocalTime();
                SystemsController.StreamOffline(currTime);
                SystemsController.PostOutgoingRaid(HostedChannel ?? "No Raid", currTime);
            }
        }

        #endregion

        #region Chat Bot

        public void HandleNewSubscriber(string DisplayName, string Months, string Subscription, string SubscriptionName)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out bool Enabled, out short Multi);
            if (Enabled)
            {
                Send(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                new( MsgVars.user, DisplayName ),
                new( MsgVars.submonths, FormatData.Plurality(Months, MsgVars.Pluralmonth, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total)) ),
                new( MsgVars.subplan, Subscription ),
                new( MsgVars.subplanname, SubscriptionName )
                })), Multi);
            }

            Systems.UpdatedStat(StreamStatType.Sub, StreamStatType.AutoEvents);
        }

        public void HandleReSubscriber(string DisplayName, int Months, string TotalMonths, string Subscription, string SubscriptionName, bool ShareStreak, string StreakMonths)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out bool Enabled, out short Multi);
            if (Enabled)
            {
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

                Send(VariableParser.ParseReplace(msg, dictionary), Multi);
            }

            Systems.UpdatedStat(StreamStatType.Sub, StreamStatType.AutoEvents);
        }

        public void HandleGiftSubscription(string DisplayName, string Months, string RecipientUserName, string Subscription, string SubscriptionName)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.GiftSub, out bool Enabled, out short Multi);
            if (Enabled)
            {
                Send(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                    new(MsgVars.user,DisplayName),
                    new(MsgVars.months, FormatData.Plurality(Months, MsgVars.Pluralmonth)),
                    new(MsgVars.receiveuser, RecipientUserName ),
                    new(MsgVars.subplan, Subscription ),
                    new(MsgVars.subplanname, SubscriptionName)
                })), Multi);
            }
            Systems.UpdatedStat(StreamStatType.GiftSubs, StreamStatType.AutoEvents);
        }

        public void HandleCommunitySubscription(string DisplayName, int SubCount, string Subscription)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.CommunitySubs, out bool Enabled, out short Multi);
            if (Enabled)
            {
                Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                    new(MsgVars.user, DisplayName),
                    new(MsgVars.count, FormatData.Plurality(SubCount, MsgVars.Pluralsub, Subscription)),
                    new(MsgVars.subplan, Subscription)
                });

                Send(VariableParser.ParseReplace(msg, dictionary), Multi);
            }

            Systems.UpdatedStat(StreamStatType.GiftSubs, SubCount);
            Systems.UpdatedStat(StreamStatType.AutoEvents);
        }

        public void HandleBeingHosted(string HostedByChannel, bool IsAutoHosted, int Viewers)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BeingHosted, out bool Enabled, out short Multi);
            if (Enabled)
            {
                Send(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                {
                    new(MsgVars.user, HostedByChannel ),
                    new(MsgVars.autohost, LocalizedMsgSystem.DetermineHost(IsAutoHosted) ),
                    new(MsgVars.viewers, FormatData.Plurality(Viewers, MsgVars.Pluralviewers
                     ))
                })), Multi);
            }

            Systems.UpdatedStat(StreamStatType.Hosted, StreamStatType.AutoEvents);
        }

        public void HandleUserJoined(List<string> Users, Bots Source)
        {
            Systems.UserJoined(Users, Source);
        }

        public void HandleUserLeft(string Users)
        {
            SystemsController.UserLeft(Users);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Calling method invokes this method and provides event arg parameter")]
        public void HandleUserTimedOut(OnUserTimedoutArgs e)
        {
            Systems.UpdatedStat(StreamStatType.UserTimedOut);
        }

        public void HandleUserBanned(string UserName)
        {
            try
            {
                Systems.UpdatedStat(StreamStatType.UserBanned);
                HandleUserLeft(UserName);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void HandleAddChat(string UserName, Bots Source)
        {
            Systems.UserJoined(new() { UserName }, Source);
        }

        public void HandleMessageReceived(string UserName, bool IsSubscriber, bool IsVip, bool IsModerator, int Bits, string Message, Bots Source)
        {
            Systems.MessageReceived(UserName, IsSubscriber, IsVip, IsModerator, Bits, Message, Source);
        }

        public void HandleIncomingRaidData(string UserName, DateTime RaidTime, string ViewerCount, string Category, Bots Source)
        {
            Systems.PostIncomingRaid(UserName, RaidTime.ToLocalTime(), ViewerCount, Category, Source);
        }

        public void HandleChatCommandReceived(Models.CmdMessage commandmsg, Bots Source)
        {
            if (GiveawayItemType == GiveawayTypes.Command && commandmsg.CommandText == GiveawayItemName)
            {
                HandleGiveawayPostName(commandmsg.DisplayName);
            }
            Systems.ProcessCommand(commandmsg, Source);
        }

        public void HandleCustomReward(string DisplayName, string RewardTitle)
        {
            if (GiveawayItemType == GiveawayTypes.CustomRewards && RewardTitle == GiveawayItemName)
            {
                HandleGiveawayPostName(DisplayName);
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
        #endregion

        #endregion

        #endregion

    }
}
