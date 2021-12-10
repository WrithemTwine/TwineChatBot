﻿using StreamerBot.BotClients;
using StreamerBot.BotClients.Twitch;
using StreamerBot.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBot.Enum;
using StreamerBot.Events;
using StreamerBot.Interfaces;
using StreamerBot.Static;
using StreamerBot.Systems;

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

namespace StreamerBot.BotIOController
{
    public class BotController
    {
        private Dispatcher AppDispatcher { get; set; }
        public SystemsController Systems { get; private set; }
        internal Collection<IBotTypes> BotsList { get; private set; } = new();

        public BotsTwitch TwitchBots { get; private set; }

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
            Send(e.Msg);
        }

        /// <summary>
        /// Send a response message to all bots incorporated into this app. The messages send through a thread managing a message delay to not flood the channel with immediate messages, channels often have limited received messages per minute.
        /// </summary>
        /// <param name="s">The string to send.</param>
        public void Send(string s)
        {
            foreach (IBotTypes bot in BotsList)
            {
                lock (Operations)
                {
                    Operations.Enqueue(new Task(() =>
                    {
                        bot.Send(s);
                    }));
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
                TwitchBots.GetAllFollowers();
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

        #region Twitch Bot Events
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
                    FollowedAt = f.FollowedAt,
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

        public static void TwitchStreamupdate(OnStreamUpdateArgs e)
        {
            HandleOnStreamUpdate(e.Stream.GameId, e.Stream.GameName);
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
            HandleUserJoined(e.Users);
        }

        public void TwitchOnUserJoined(OnUserJoinedArgs e)
        {
            HandleUserJoined(new() { e.Username });
        }

        public static void TwitchOnUserLeft(OnUserLeftArgs e)
        {
            HandleUserLeft(e.Username);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Calling method invokes this method and provides event arg parameter")]
        public void TwitchOnUserTimedout(OnUserTimedoutArgs e = null)
        {
            HandleUserTimedOut();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Calling method invokes this method and provides event arg parameter")]
        public void TwitchOnUserBanned(OnUserBannedArgs e = null)
        {
            HandleUserBanned();
        }

        public void TwitchRitualNewChatter(OnRitualNewChatterArgs e)
        {
            HandleAddChat(e.RitualNewChatter.DisplayName);
        }

        public void TwitchMessageReceived(OnMessageReceivedArgs e)
        {
            HandleMessageReceived(e.ChatMessage.DisplayName, e.ChatMessage.IsSubscriber, e.ChatMessage.IsVip, e.ChatMessage.IsModerator, e.ChatMessage.Bits, e.ChatMessage.Message);
        }

        public void TwitchIncomingRaid(OnIncomingRaidArgs e)
        {
            HandleIncomingRaidData(e.DisplayName, e.RaidTime, e.ViewerCount, e.Category);
        }

        public void TwitchChatCommandReceived(OnChatCommandReceivedArgs e)
        {
            HandleChatCommandReceived( new()
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
                Message = e.Command.ChatMessage.Message,
                UserType = System.Enum.Parse<ViewerTypes>(e.Command.ChatMessage.UserType.ToString())
            });
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

                    if ((OptionFlags.PostMultiLive && MultiLive) || !MultiLive)
                    {
                        // get message, set a default if otherwise deleted/unavailable
                        string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out bool Enabled);

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

        public static void HandleOnStreamUpdate(string gameId, string gameName)
        {
            SystemsController.SetCategory(gameId, gameName);
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
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out bool Enabled);
            if (Enabled)
            {
                Send(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                new( MsgVars.user, DisplayName ),
                new( MsgVars.submonths, FormatData.Plurality(Months, MsgVars.Pluralmonths, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total)) ),
                new( MsgVars.subplan, Subscription ),
                new( MsgVars.subplanname, SubscriptionName )
                })));
            }

            Systems.UpdatedStat(StreamStatType.Sub, StreamStatType.AutoEvents);
        }

        public void HandleReSubscriber(string DisplayName, int Months, string TotalMonths, string Subscription, string SubscriptionName, bool ShareStreak, string StreakMonths)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out bool Enabled);
            if (Enabled)
            {
                Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                new( MsgVars.user, DisplayName ),
                new( MsgVars.months, FormatData.Plurality(Months, MsgVars.Pluralmonths, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total)) ),
                new( MsgVars.submonths, FormatData.Plurality(TotalMonths, MsgVars.Pluralmonths, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total))),
                new( MsgVars.subplan, Subscription),
                new( MsgVars.subplanname,SubscriptionName )
                });

                // add the streak element if user wants their sub streak displayed
                if (ShareStreak)
                {
                    VariableParser.AddData(ref dictionary, new Tuple<MsgVars, string>[] { new(MsgVars.streak, StreakMonths) });
                }

                Send(VariableParser.ParseReplace(msg, dictionary));
            }

            Systems.UpdatedStat(StreamStatType.Sub, StreamStatType.AutoEvents);
        }

        public void HandleGiftSubscription(string DisplayName, string Months, string RecipientUserName, string Subscription, string SubscriptionName)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.GiftSub, out bool Enabled);
            if (Enabled)
            {
                Send(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                    new(MsgVars.user,DisplayName),
                    new(MsgVars.months, FormatData.Plurality(Months, MsgVars.Pluralmonths)),
                    new(MsgVars.receiveuser, RecipientUserName ),
                    new(MsgVars.subplan, Subscription ),
                    new(MsgVars.subplanname, SubscriptionName)
                })));
            }
            Systems.UpdatedStat(StreamStatType.GiftSubs, StreamStatType.AutoEvents);
        }

        public void HandleCommunitySubscription(string DisplayName, int SubCount, string Subscription)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.CommunitySubs, out bool Enabled);
            if (Enabled)
            {
                Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                    new(MsgVars.user, DisplayName),
                    new(MsgVars.count, FormatData.Plurality(SubCount, MsgVars.Pluralsub, Subscription)),
                    new(MsgVars.subplan, Subscription)
                });

                Send(VariableParser.ParseReplace(msg, dictionary));
            }

            Systems.UpdatedStat(StreamStatType.GiftSubs, SubCount);
            Systems.UpdatedStat(StreamStatType.AutoEvents);
        }

        public void HandleBeingHosted(string HostedByChannel, bool IsAutoHosted, int Viewers)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BeingHosted, out bool Enabled);
            if (Enabled)
            {
                Send(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                {
                    new(MsgVars.user, HostedByChannel ),
                    new(MsgVars.autohost, LocalizedMsgSystem.DetermineHost(IsAutoHosted) ),
                    new(MsgVars.viewers, FormatData.Plurality(Viewers, MsgVars.Pluralviewers
                     ))
                })));
            }

            Systems.UpdatedStat(StreamStatType.Hosted, StreamStatType.AutoEvents);
        }

        public void HandleUserJoined(List<string> Users)
        {
            Systems.UserJoined(Users);
        }

        public static void HandleUserLeft(string Users)
        {
            SystemsController.UserLeft(Users);
        }

        public void HandleUserTimedOut()
        {
            Systems.UpdatedStat(StreamStatType.UserTimedOut);
        }

        public void HandleUserBanned()
        {
            Systems.UpdatedStat(StreamStatType.UserBanned);
        }

        public void HandleAddChat(string UserName)
        {
            Systems.AddChat(UserName);
        }

        public void HandleMessageReceived(string UserName, bool IsSubscriber, bool IsVip, bool IsModerator, int Bits, string Message)
        {
            Systems.MessageReceived(UserName, IsSubscriber, IsVip, IsModerator, Bits, Message);
        }

        public void HandleIncomingRaidData(string UserName, DateTime RaidTime, string ViewerCount, string Category)
        {
            Systems.PostIncomingRaid(UserName, RaidTime, ViewerCount, Category);
        }

        public void HandleChatCommandReceived(Models.CmdMessage commandmsg)
        {
            Systems.ProcessCommand(commandmsg);
        }

        #endregion

        #endregion

    }
}
