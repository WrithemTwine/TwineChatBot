#if DEBUG
#define LOGGING
#endif

using ChatBot_Net5.Clients;
using ChatBot_Net5.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

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

            Stats.StreamOffline();
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

            if ((DateTime.Now - e.Stream.StartedAt.ToLocalTime()).TotalSeconds < 10 * TwitchIO.FrequencyTime)
            {

                // get message, set a default if otherwise deleted/unavailable
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Live);
                msg ??= "@everyone, #user is now live streaming #category - #title! &lt;br/&gt; Come join and say hi at: #url";

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
                }
            }
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

            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.Follow))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Follow);
                msg ??= "Thanks #user for the follow!";

                foreach (Follow f in e.NewFollowers)
                {
                    if (DataManage.AddFollower(f.FromUserName, f.FollowedAt) && !DataManage.UpdatingFollowers)
                    {
                        Send(msg.Replace("#user", "@" + f.FromUserName));
                    }
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

            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.Subscribe))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Subscribe);
                msg ??= "Thanks #user for the subscribing!";

                Dictionary<string, string> dictionary = new() {
                { "#user", e.Subscriber.DisplayName },
                { "#submonths", Plurality(e.Subscriber.MsgParamCumulativeMonths, "total month", "total months") },
                { "#subplan", e.Subscriber.SubscriptionPlan.ToString() },
                { "#subplanname", e.Subscriber.SubscriptionPlanName }
                };

                Send(ParseReplace(msg, dictionary));
            }
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

            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.Resubscribe))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Resubscribe);
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

            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.GiftSub))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.GiftSub);
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
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.CommunitySubs))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.CommunitySubs);
                msg ??= "Thanks #user for giving #count to the community!";

                Dictionary<string, string> dictionary = new() {
                { "#user", e.GiftedSubscription.DisplayName },
                { "#count", Plurality(e.GiftedSubscription.MsgParamSenderCount, e.GiftedSubscription.MsgParamSubPlan+" subscription", e.GiftedSubscription.MsgParamSubPlan+" subscriptions") },
                { "#subplan", e.GiftedSubscription.MsgParamSubPlan.ToString() }
                };

                Send(ParseReplace(msg, dictionary));
            }
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
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.BeingHosted))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.BeingHosted);
                msg ??= "Thanks #user for #autohost this channel!";

                Dictionary<string, string> dictionary = new()
                {
                { "#user", e.BeingHostedNotification.HostedByChannel },
                { "#autohost", e.BeingHostedNotification.IsAutoHosted ? "auto-hosting" : "hosting" },
                { "#viewers", Plurality( e.BeingHostedNotification.Viewers, "viewer", "viewers" ) }
                };

                Send(ParseReplace(msg, dictionary));
            }
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

            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.Raid))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Raid);
                msg ??= "Thanks #user for bringing #viewers and raiding the channel!";

                Dictionary<string, string> dictionary = new() {
                { "#user", e.RaidNotification.DisplayName },
                { "#viewers", Plurality(e.RaidNotification.MsgParamViewerCount, "viewer", "viewers" ) }
                };

                Send(ParseReplace(msg, dictionary));
            }
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

            AddChatString(e.Command.ChatMessage);
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

            // handle bit cheers
            if (e.ChatMessage.Bits > 0)
            {
                if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.Bits))
                {
                    string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Bits);
                    msg ??= "Thanks #user for giving #bits!";

                    Dictionary<string, string> dictionary = new() {
                    { "#user", e.ChatMessage.DisplayName },
                    { "#bits", Plurality(e.ChatMessage.Bits, "bit", "bits" ) }
                    };

                    Send(ParseReplace(msg, dictionary));
                }
            }

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
            Stats.UserJoined(e.Username, DateTime.Now);
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
                Stats.UserJoined(user, DateTime.Now);
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
