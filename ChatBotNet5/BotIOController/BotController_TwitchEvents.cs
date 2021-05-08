#if DEBUG
//#define LOGGING
#endif

using ChatBot_Net5.Clients;
using ChatBot_Net5.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;

namespace ChatBot_Net5.BotIOController
{
    public sealed partial class BotController
    {

        /// <summary>
        /// Register event handlers for the chat services
        /// </summary>
        private void RegisterHandlers()
        {
            TwitchIO.TwitchChat.OnBeingHosted += Client_OnBeingHosted;
            TwitchIO.TwitchChat.OnChannelStateChanged += Client_OnChannelStateChanged;
            TwitchIO.TwitchChat.OnChatCleared += Client_OnChatCleared;
            TwitchIO.TwitchChat.OnChatColorChanged += Client_OnChatColorChanged;
            TwitchIO.TwitchChat.OnChatCommandReceived += Client_OnChatCommandReceived;
            TwitchIO.TwitchChat.OnCommunitySubscription += Client_OnCommunitySubscription;
            TwitchIO.TwitchChat.OnConnected += Client_OnConnected;
            TwitchIO.TwitchChat.OnConnectionError += Client_OnConnectionError;
            TwitchIO.TwitchChat.OnDisconnected += Client_OnDisconnected;
            TwitchIO.TwitchChat.OnError += Client_OnError;
            TwitchIO.TwitchChat.OnExistingUsersDetected += Client_OnExistingUsersDetected;
            TwitchIO.TwitchChat.OnFailureToReceiveJoinConfirmation += Client_OnFailureToReceiveJoinConfirmation;
            TwitchIO.TwitchChat.OnGiftedSubscription += Client_OnGiftedSubscription;
            TwitchIO.TwitchChat.OnHostingStarted += Client_OnHostingStarted;
            TwitchIO.TwitchChat.OnHostingStopped += Client_OnHostingStopped;
            TwitchIO.TwitchChat.OnHostLeft += Client_OnHostLeft;
            TwitchIO.TwitchChat.OnIncorrectLogin += Client_OnIncorrectLogin;
            TwitchIO.TwitchChat.OnJoinedChannel += Client_OnJoinedChannel;
            TwitchIO.TwitchChat.OnLeftChannel += Client_OnLeftChannel;
            TwitchIO.TwitchChat.OnMessageCleared += Client_OnMessageCleared;
            TwitchIO.TwitchChat.OnMessageReceived += Client_OnMessageReceived;
            TwitchIO.TwitchChat.OnMessageSent += Client_OnMessageSent;
            TwitchIO.TwitchChat.OnMessageThrottled += Client_OnMessageThrottled;
            TwitchIO.TwitchChat.OnModeratorJoined += Client_OnModeratorJoined;
            TwitchIO.TwitchChat.OnModeratorLeft += Client_OnModeratorLeft;
            TwitchIO.TwitchChat.OnModeratorsReceived += Client_OnModeratorsReceived;
            TwitchIO.TwitchChat.OnNewSubscriber += Client_OnNewSubscriber;
            TwitchIO.TwitchChat.OnNoPermissionError += Client_OnNoPermissionError;
            TwitchIO.TwitchChat.OnNowHosting += Client_OnNowHosting;
            TwitchIO.TwitchChat.OnRaidedChannelIsMatureAudience += Client_OnRaidedChannelIsMatureAudience;
            TwitchIO.TwitchChat.OnRaidNotification += Client_OnRaidNotification;
            TwitchIO.TwitchChat.OnReconnected += Client_OnReconnected;
            TwitchIO.TwitchChat.OnReSubscriber += Client_OnReSubscriber;
            TwitchIO.TwitchChat.OnRitualNewChatter += Client_OnRitualNewChatter;
            TwitchIO.TwitchChat.OnSelfRaidError += Client_OnSelfRaidError;
            TwitchIO.TwitchChat.OnSendReceiveData += Client_OnSendReceiveData;
            TwitchIO.TwitchChat.OnUnaccountedFor += Client_OnUnaccountedFor;
            TwitchIO.TwitchChat.OnUserBanned += Client_OnUserBanned;
            TwitchIO.TwitchChat.OnUserJoined += Client_OnUserJoined;
            TwitchIO.TwitchChat.OnUserLeft += Client_OnUserLeft;
            TwitchIO.TwitchChat.OnUserStateChanged += Client_OnUserStateChanged;
            TwitchIO.TwitchChat.OnUserTimedout += Client_OnUserTimedout;
            TwitchIO.TwitchChat.OnVIPsReceived += Client_OnVIPsReceived;
            TwitchIO.TwitchChat.OnWhisperCommandReceived += Client_OnWhisperCommandReceived;
            TwitchIO.TwitchChat.OnWhisperReceived += Client_OnWhisperReceived;
            TwitchIO.TwitchChat.OnWhisperSent += Client_OnWhisperSent;
            TwitchIO.TwitchChat.OnWhisperThrottled += Client_OnWhisperThrottled;

            IOModuleTwitch.FollowerService.OnNewFollowersDetected += FollowerService_OnNewFollowersDetected;
            IOModuleTwitch.LiveStreamMonitor.OnStreamOnline += LiveStreamMonitor_OnStreamOnline;
            IOModuleTwitch.LiveStreamMonitor.OnStreamUpdate += LiveStreamMonitor_OnStreamUpdate;
            IOModuleTwitch.LiveStreamMonitor.OnStreamOffline += LiveStreamMonitor_OnStreamOffline;
        }

        #region Stream On, Off, Updated
        /// <summary>
        /// Event called when the stream is detected to be offline.
        /// </summary>
        /// <param name="sender">The calling object.</param>
        /// <param name="e">Contains the offline arguments.</param>
        private void LiveStreamMonitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.Stream.ToString());
            }
#endif

            Stats.StreamOffline(DateTime.Now);
        }

        /// <summary>
        /// Event called when the stream is detected to be updated.
        /// </summary>
        /// <param name="sender">The calling object.</param>
        /// <param name="e">Contains the update arguments.</param>
        private void LiveStreamMonitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        /// <summary>
        /// Event called when the stream is detected to be online.
        /// </summary>
        /// <param name="sender">The calling object.</param>
        /// <param name="e">Contains the online arguments.</param>
        private void LiveStreamMonitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

            Stats.StreamOnline();

            if (OptionFlags.PostMultiLive || !DataManage.GetTodayStream(e.Stream.StartedAt))
            {
                // get message, set a default if otherwise deleted/unavailable
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Live);
                msg ??= "@everyone, #user is now live streaming #category - #title! Come join and say hi at: #url";

                // keys for exchanging codes for representative names
                Dictionary<string, string> dictionary = new()
                {
                    { "#user", e.Stream.UserName },
                    { "#category", e.Stream.GameName },
                    { "#title", e.Stream.Title },
                    { "#url", "https://www.twitch.tv/" + e.Stream.UserName }
                };

                foreach (Uri u in DataManage.GetWebhooks(WebhooksKind.Live))
                {
                    DiscordWebhook.SendLiveMessage(u, ParseReplace(msg, dictionary)).Wait();
                    Stats.AddDiscord();
                }
            }
            Stats.StartStreamOnline(e.Stream.StartedAt);

        }
        #endregion Stream On, Off, Updated

        #region Followers
        private void FollowerService_OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif
            bool FollowEnabled = (bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.Follow);

            string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Follow);
            msg ??= "Thanks #user for the follow!";

            foreach (Follow f in e.NewFollowers)
            {
                if (DataManage.AddFollower(f.FromUserName, f.FollowedAt) && !DataManage.UpdatingFollowers)
                {
                    if (FollowEnabled)
                    {
                        Send(msg.Replace("#user", "@" + f.FromUserName));
                    }
                    Stats.AddFollow();
                    Stats.AddAutoEvents();
                }
            }
            
        }

        #endregion Followers

        #region Subscriptions
        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.Subscribe))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Subscribe);
                msg ??= "Thanks #user for the subscribing!";

                Dictionary<string, string> dictionary = new() {
                { "#user", e.Subscriber.DisplayName },
                { "#submonths", Plurality(e.Subscriber.MsgParamCumulativeMonths, "total month", "total months") },
                { "#subplan", e.Subscriber.SubscriptionPlan.ToString() },
                { "#subplanname", e.Subscriber.SubscriptionPlanName }
                };

                Send(ParseReplace(msg, dictionary));
            }

            Stats.AddSub();
            Stats.AddAutoEvents();
        }

        private void Client_OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.Resubscribe))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Resubscribe);
                msg ??= "Thanks #user for re-subscribing!";

                Dictionary<string, string> dictionary = new() {
                { "#user", e.ReSubscriber.DisplayName },
                { "#months", Plurality(e.ReSubscriber.Months, "total month", "total months") },
                { "#submonths", Plurality(e.ReSubscriber.MsgParamCumulativeMonths, "month total", "months total") },
                { "#subplan", e.ReSubscriber.SubscriptionPlan.ToString() },
                { "#subplanname", e.ReSubscriber.SubscriptionPlanName }
                };

                // add the streak element if user wants their sub streak displayed
                if (e.ReSubscriber.MsgParamShouldShareStreak)
                {
                    dictionary.Add("#streak", e.ReSubscriber.MsgParamStreakMonths);
                }

                Send(ParseReplace(msg, dictionary));
            }

            Stats.AddSub();
            Stats.AddAutoEvents();
        }

        private void Client_OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.GiftSub))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.GiftSub);
                msg ??= "Thanks #user for gifting a #subplan subscription to #receiveuser!";

                Dictionary<string, string> dictionary = new() {
                { "#user", e.GiftedSubscription.DisplayName },
                { "#months", Plurality(e.GiftedSubscription.MsgParamMonths, "month", "months") },
                { "#receiveuser", e.GiftedSubscription.MsgParamRecipientUserName },
                { "#subplan", e.GiftedSubscription.MsgParamSubPlan.ToString() },
                { "#subplanname", e.GiftedSubscription.MsgParamSubPlanName}
                };

                Send(ParseReplace(msg, dictionary));
            }

            Stats.AddGiftSubs();
            Stats.AddAutoEvents();
        }

        private void Client_OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.CommunitySubs))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.CommunitySubs);
                msg ??= "Thanks #user for giving #count to the community!";

                Dictionary<string, string> dictionary = new() {
                { "#user", e.GiftedSubscription.DisplayName },
                { "#count", Plurality(e.GiftedSubscription.MsgParamSenderCount, e.GiftedSubscription.MsgParamSubPlan+" subscription", e.GiftedSubscription.MsgParamSubPlan+" subscriptions") },
                { "#subplan", e.GiftedSubscription.MsgParamSubPlan.ToString() }
                };

                Send(ParseReplace(msg, dictionary));
            }

            Stats.AddGiftSubs(e.GiftedSubscription.MsgParamMassGiftCount);
            Stats.AddAutoEvents();
        }

        #endregion Subscriptions

        #region Hosting
        private void Client_OnBeingHosted(object sender, OnBeingHostedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }

#endif
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.BeingHosted))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.BeingHosted);
                msg ??= "Thanks #user for #autohost this channel!";

                Dictionary<string, string> dictionary = new()
                {
                { "#user", e.BeingHostedNotification.HostedByChannel },
                { "#autohost", e.BeingHostedNotification.IsAutoHosted ? "auto-hosting" : "hosting" },
                { "#viewers", Plurality( e.BeingHostedNotification.Viewers, "viewer", "viewers" ) }
                };

                Send(ParseReplace(msg, dictionary));
            }

            Stats.AddHosted();
            Stats.AddAutoEvents();
        }

        private void Client_OnHostLeft(object sender, EventArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnHostingStopped(object sender, OnHostingStoppedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnHostingStarted(object sender, OnHostingStartedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnNowHosting(object sender, OnNowHostingArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }
        #endregion Hosting

        #region Raid events
        private void Client_OnRaidNotification(object sender, OnRaidNotificationArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.Raid))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Raid);
                msg ??= "Thanks #user for bringing #viewers and raiding the channel!";

                Dictionary<string, string> dictionary = new() {
                { "#user", e.RaidNotification.DisplayName },
                { "#viewers", Plurality(e.RaidNotification.MsgParamViewerCount, "viewer", "viewers" ) }
                };

                Send(ParseReplace(msg, dictionary));
            }
            Stats.AddRaids();
            Stats.AddAutoEvents();
        }

        private void Client_OnSelfRaidError(object sender, EventArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnRaidedChannelIsMatureAudience(object sender, EventArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        #endregion Raid events

        #region Msg IO
        private void Client_OnWhisperThrottled(object sender, TwitchLib.Communication.Events.OnWhisperThrottledEventArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnWhisperSent(object sender, OnWhisperSentArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnWhisperCommandReceived(object sender, OnWhisperCommandReceivedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

            Stats.AddCommands();
            AddChat(e.Command.ChatMessage.DisplayName);

            try
            {
                string response = ProcessCommands.ParseCommand(e.Command.CommandText, e.Command.ArgumentsAsList, e.Command.ChatMessage);
                if (response != "")
                {
                    Send(response);
                }
                //AddChatString(response);
            }
            catch (InvalidOperationException InvalidOp)
            {
                Send(InvalidOp.Message);
            }

        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

            AddChatString(e.ChatMessage);
            Stats.AddTotalChats();

            if (e.ChatMessage.IsSubscriber)
            {
                Stats.SubJoined(e.ChatMessage.DisplayName);
            }
            if (e.ChatMessage.IsVip)
            {
                Stats.VIPJoined(e.ChatMessage.DisplayName);
            }

            // handle bit cheers
            if (e.ChatMessage.Bits > 0)
            {
                if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.Bits))
                {
                    string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Bits);
                    msg ??= "Thanks #user for giving #bits!";

                    Dictionary<string, string> dictionary = new() {
                    { "#user", e.ChatMessage.DisplayName },
                    { "#bits", Plurality(e.ChatMessage.Bits, "bit", "bits" ) }
                    };

                    Send(ParseReplace(msg, dictionary));
                    Stats.AddBits(e.ChatMessage.Bits);
                    Stats.AddAutoEvents();
                }
            }

            AddChat(e.ChatMessage.DisplayName);            
        }

        private void Client_OnMessageThrottled(object sender, TwitchLib.Communication.Events.OnMessageThrottledEventArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnMessageSent(object sender, OnMessageSentArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnMessageCleared(object sender, OnMessageClearedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnSendReceiveData(object sender, OnSendReceiveDataArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnRitualNewChatter(object sender, OnRitualNewChatterArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif
            AddChat(e.RitualNewChatter.DisplayName);
        }

        #region Chat changes
        private void Client_OnChatColorChanged(object sender, OnChatColorChangedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnChatCleared(object sender, OnChatClearedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        #endregion

        #endregion Msg IO

        #region User state changes

        #region badge changes
        private void Client_OnVIPsReceived(object sender, OnVIPsReceivedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnModeratorsReceived(object sender, OnModeratorsReceivedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }


        #endregion

        #region Moderators

        private void Client_OnModeratorJoined(object sender, OnModeratorJoinedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif
            Stats.ModJoined(e.Username);
        }

        private void Client_OnModeratorLeft(object sender, OnModeratorLeftArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }
        #endregion

        private void Client_OnUserJoined(object sender, OnUserJoinedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }

#endif
            if (Stats.UserJoined(e.Username, DateTime.Now))
            {
                if (OptionFlags.FirstUserJoinedMsg)
                {
                    RegisterJoinedUser(e.Username);
                }
            }
        }

        private void AddChat(string Username)
        {
            if (Stats.UserChat(Username))
            {
                if (OptionFlags.FirstUserChatMsg)
                {
                    RegisterJoinedUser(Username);
                }
            }
        }

        private void RegisterJoinedUser(string Username)
        {
            if (OptionFlags.FirstUserJoinedMsg || OptionFlags.FirstUserChatMsg)
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.UserJoined);
                msg ??= "Thanks #user for stopping by the channel!";

                Dictionary<string, string> dictionary = new()
                {
                    { "#user", Username }
                };

                Send(ParseReplace(msg, dictionary));
            }

            if (OptionFlags.AutoShout)
            {
                bool output = ProcessCommands.CheckShout(Username, out string response);
                if (output) Send(response);
            }
        }

        private void Client_OnUserLeft(object sender, OnUserLeftArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

            Stats.UserLeft(e.Username, DateTime.Now);
        }

        private void Client_OnUserBanned(object sender, OnUserBannedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif
            Stats.AddUserBanned();


        }

        private void Client_OnUserStateChanged(object sender, OnUserStateChangedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif
        }

        private void Client_OnUserTimedout(object sender, OnUserTimedoutArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif
            Stats.AddUserTimedOut();
        }

        private void Client_OnExistingUsersDetected(object sender, OnExistingUsersDetectedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

            foreach (string user in e.Users)
            {
                if (Stats.UserJoined(user, DateTime.Now))
                {
                    RegisterJoinedUser(user);
                }
            }
        }

        #endregion

        #region Connection and Channel Info

        #region Channel Join Leave
        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

            if (TwitchIO.ShowConnectionMsg)
            {
                Version version = Assembly.GetEntryAssembly().GetName().Version;
                string s = "Twine Chatbot by WrithemTwine, version " + string.Format(CultureInfo.CurrentCulture, "{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision) + ", is now connected!";

                Send(s);
            }
        }

        private void Client_OnLeftChannel(object sender, OnLeftChannelArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        #endregion


        #region Connecting
        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.AutoJoinChannel + " " + e.BotUsername);
            }
#endif

        }

        private void Client_OnReconnected(object sender, TwitchLib.Communication.Events.OnReconnectedEventArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        #endregion

        private void Client_OnChannelStateChanged(object sender, OnChannelStateChangedArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ChannelState.ToString());
            }
#endif

        }

        #region Error checking
        private void Client_OnNoPermissionError(object sender, EventArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnError(object sender, TwitchLib.Communication.Events.OnErrorEventArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnIncorrectLogin(object sender, OnIncorrectLoginArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnFailureToReceiveJoinConfirmation(object sender, OnFailureToReceiveJoinConfirmationArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        private void Client_OnUnaccountedFor(object sender, OnUnaccountedForArgs e)
        {
#if LOGGING
            MethodBase b = MethodBase.GetCurrentMethod();
            _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Method Name: " + b.Name);
            if (b.GetParameters().Length > 0)
            {
                _TraceLogWriter?.WriteLine(DateTime.Now.ToString() + " Parameter: e " + e.ToString());
            }
#endif

        }

        #endregion

        #endregion

    }
}
