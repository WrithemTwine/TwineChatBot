
using ChatBot_Net5.Clients;
using ChatBot_Net5.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace ChatBot_Net5.BotIOController
{
    public sealed partial class BotController
    {
        #region Register Event Handlers for Chat services
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

            TwitchIO.FollowerService.OnNewFollowersDetected += FollowerService_OnNewFollowersDetected;
            TwitchIO.LiveStreamMonitor.OnStreamOnline += LiveStreamMonitor_OnStreamOnline;
            TwitchIO.LiveStreamMonitor.OnStreamUpdate += LiveStreamMonitor_OnStreamUpdate;
            TwitchIO.LiveStreamMonitor.OnStreamOffline += LiveStreamMonitor_OnStreamOffline;
        }

        private void LiveStreamMonitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            
        }

        private void LiveStreamMonitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
            string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Live);
            msg ??= "@everyone, #user is now live! #title and playing #category at #url";

            Dictionary<string, string> dictionary = new Dictionary<string, string>() {
                { "#user", "@"+e.Stream.UserName },
                { "#category", e.Stream.GameName },
                { "#title", e.Stream.Title },
                { "#url", "https://wwww.twitch.tv/" + e.Stream.UserId }
            };

            DiscordWebhook.SendLiveMessage(DataManage.GetWebhooks(WebhooksKind.Live), ParseReplace(msg, dictionary));
        }

        private void LiveStreamMonitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Live);
            msg ??= "@everyone, #user is now live! #title and playing #category at #url";

            Dictionary<string, string> dictionary = new Dictionary<string, string>() {
                { "#user", "@"+e.Stream.UserName },
                { "#category", e.Stream.GameName },
                { "#title", e.Stream.Title },
                { "#url", "https://wwww.twitch.tv/" + e.Stream.UserId }
            };

            DiscordWebhook.SendLiveMessage(DataManage.GetWebhooks(WebhooksKind.Live), ParseReplace(msg, dictionary));
        }

        private void FollowerService_OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.Follow))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Follow);
                msg ??= "Thanks #user for the follow!";

                foreach (Follow f in e.NewFollowers)
                {
                    if (!DataManage.CheckFollower(f.FromUserName))
                    {
                        Send(msg.Replace("#user", "@" + f.FromUserName));
                        DataManage.AddFollower(f.FromUserName, f.FollowedAt);
                    }
                }
            }
        }

        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.Subscribe))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Subscribe);
                msg ??= "Thanks #user for the subscribing!";

                Dictionary<string, string> dictionary = new Dictionary<string, string>() {
                { "#user", "@"+e.Subscriber.DisplayName },
                { "#submonths", Plurality(e.Subscriber.MsgParamCumulativeMonths, "total month", "total months") },
                { "#subplan", e.Subscriber.SubscriptionPlan.ToString() },
                { "#subplanname", e.Subscriber.SubscriptionPlanName }
            };

                Send(ParseReplace(msg, dictionary));
            }
        }

        private void Client_OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.Resubscribe))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Resubscribe);
                msg ??= "Thanks #user for re-subscribing!";

                Dictionary<string, string> dictionary = new Dictionary<string, string>() {
                { "#user", "@"+e.ReSubscriber.DisplayName },
                { "#months", Plurality(e.ReSubscriber.Months, "month", "months") },
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
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.GiftSub))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.GiftSub);
                msg ??= "Thanks #user for gifting a #subplan subscription to #receiveuser!";

                Dictionary<string, string> dictionary = new Dictionary<string, string>() {
                { "#user", "@"+e.GiftedSubscription.DisplayName },
                { "#months", Plurality(e.GiftedSubscription.MsgParamMonths, "month", "months") },
                { "#receiveuser", e.GiftedSubscription.MsgParamRecipientUserName },
                { "#subplan", e.GiftedSubscription.MsgParamSubPlan.ToString() },
                { "#subplanname", e.GiftedSubscription.MsgParamSubPlanName}
                };

                Send(ParseReplace(msg, dictionary));
            }
        }

        private void Client_OnBeingHosted(object sender, OnBeingHostedArgs e)
        {
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.BeingHosted))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.BeingHosted);
                msg ??= "Thanks #user for #autohost this channel!";

                Dictionary<string, string> dictionary = new Dictionary<string, string>() {
                { "#user", "@"+e.BeingHostedNotification.HostedByChannel },
                { "#autohost", e.BeingHostedNotification.IsAutoHosted ? "auto-hosting" : "hosting" },
                { "#viewers", Plurality( e.BeingHostedNotification.Viewers, "viewer", "viewers" ) }
            };

                Send(ParseReplace(msg, dictionary));
            }
        }

        private void Client_OnRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.Raid))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Raid);
                msg ??= "Thanks #user for bringing #viewers and raiding the channel!";

                Dictionary<string, string> dictionary = new Dictionary<string, string>() {
                { "#user", "@"+e.RaidNotification.DisplayName },
                { "#viewers", Plurality(e.RaidNotification.MsgParamViewerCount, "viewer", "viewers" ) }
            };

                Send(ParseReplace(msg, dictionary));
            }
        }


        private void Client_OnWhisperThrottled(object sender, TwitchLib.Communication.Events.OnWhisperThrottledEventArgs e)
        {
            
        }

        private void Client_OnWhisperSent(object sender, OnWhisperSentArgs e)
        {
            
        }

        private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            
        }

        private void Client_OnWhisperCommandReceived(object sender, OnWhisperCommandReceivedArgs e)
        {

        }

        private void Client_OnVIPsReceived(object sender, OnVIPsReceivedArgs e)
        {
            
        }

        private void Client_OnUserTimedout(object sender, OnUserTimedoutArgs e)
        {

        }

        private void Client_OnUserStateChanged(object sender, OnUserStateChangedArgs e)
        {

        }

        private void Client_OnUserLeft(object sender, OnUserLeftArgs e)
        {
            DataManage.UserLeft(e.Username, DateTime.Now);
        }

        private void Client_OnUserJoined(object sender, OnUserJoinedArgs e)
        {
            DataManage.UserJoined(e.Username, DateTime.Now);
        }

        private void Client_OnUserBanned(object sender, OnUserBannedArgs e)
        {

        }

        private void Client_OnUnaccountedFor(object sender, OnUnaccountedForArgs e)
        {

        }

        private void Client_OnSendReceiveData(object sender, OnSendReceiveDataArgs e)
        {

        }

        private void Client_OnSelfRaidError(object sender, EventArgs e)
        {

        }

        private void Client_OnRitualNewChatter(object sender, OnRitualNewChatterArgs e)
        {

        }

        private void Client_OnReconnected(object sender, TwitchLib.Communication.Events.OnReconnectedEventArgs e)
        {

        }

        private void Client_OnRaidedChannelIsMatureAudience(object sender, EventArgs e)
        {

        }

        private void Client_OnNowHosting(object sender, OnNowHostingArgs e)
        {

        }

        private void Client_OnNoPermissionError(object sender, EventArgs e)
        {

        }

        private void Client_OnModeratorsReceived(object sender, OnModeratorsReceivedArgs e)
        {

        }

        private void Client_OnModeratorLeft(object sender, OnModeratorLeftArgs e)
        {

        }

        private void Client_OnModeratorJoined(object sender, OnModeratorJoinedArgs e)
        {

        }

        private void Client_OnMessageThrottled(object sender, TwitchLib.Communication.Events.OnMessageThrottledEventArgs e)
        {

        }

        private void Client_OnMessageSent(object sender, OnMessageSentArgs e)
        {

        }

        private void Client_OnMessageCleared(object sender, OnMessageClearedArgs e)
        {

        }


        private void Client_OnLeftChannel(object sender, OnLeftChannelArgs e)
        {

        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            string s = "Twine Chatbot by WrithemTwine, version " + string.Format(CultureInfo.CurrentCulture, "{0}.{1}.{2}.{3}",  version.Major, version.MajorRevision, version.Minor, version.MinorRevision) + ", is now connected!";

            Send(s);
        }

        private void Client_OnIncorrectLogin(object sender, OnIncorrectLoginArgs e)
        {

        }

        private void Client_OnHostLeft(object sender, EventArgs e)
        {

        }

        private void Client_OnHostingStopped(object sender, OnHostingStoppedArgs e)
        {

        }

        private void Client_OnHostingStarted(object sender, OnHostingStartedArgs e)
        {

        }

        private void Client_OnFailureToReceiveJoinConfirmation(object sender, OnFailureToReceiveJoinConfirmationArgs e)
        {

        }

        private void Client_OnExistingUsersDetected(object sender, OnExistingUsersDetectedArgs e)
        {

        }

        private void Client_OnError(object sender, TwitchLib.Communication.Events.OnErrorEventArgs e)
        {
            
        }

        private void Client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            
        }

        private void Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            
        }

        private void Client_OnChatColorChanged(object sender, OnChatColorChangedArgs e)
        {
            
        }

        private void Client_OnChatCleared(object sender, OnChatClearedArgs e)
        {
            
        }

        private void Client_OnChannelStateChanged(object sender, OnChannelStateChangedArgs e)
        {
           
        }


        private void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            AddChatString(e.Command.ChatMessage);
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            AddChatString(e.ChatMessage);

            // handle bit cheers
            if (e.ChatMessage.Bits > 0)
            {
                if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, CommandAction.Bits))
                {
                    string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, CommandAction.Bits);
                    msg ??= "Thanks #user for donating #bits!";

                    Dictionary<string, string> dictionary = new Dictionary<string, string>() {
                    { "#user", "@"+e.ChatMessage.DisplayName },
                    { "#viewers", Plurality(e.ChatMessage.Bits, "bit", "bits" ) }
                };

                    Send(ParseReplace(msg, dictionary));
                }
            }

        }

        private void Client_OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
        {
            
        }

        #endregion Register Event Handlers to Chat Services

    }
}
