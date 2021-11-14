using StreamerBot.BotClients.Twitch;
using StreamerBot.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBot.Enum;
using StreamerBot.Events;
using StreamerBot.Static;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Api.ThirdParty.ModLookup;
using TwitchLib.Client.Events;
using TwitchLib.Communication.Events;

namespace StreamerBot.BotClients
{
    public class BotsTwitch : BotsBase
    {
        public event EventHandler<OnNewFollowersDetectedArgs> OnCompletedDownloadFollowers;
        public event EventHandler<ClipFoundEventArgs> OnClipFound;

        public static TwitchBotChatClient TwitchBotChatClient { get; private set; } = new();
        public static TwitchBotFollowerSvc TwitchFollower { get; private set; } = new();
        public static TwitchBotLiveMonitorSvc TwitchLiveMonitor { get; private set; } = new();
        public static TwitchBotClipSvc TwitchBotClipSvc { get; private set; } = new();
        public static TwitchBotUserSvc TwitchBotUserSvc { get; private set; } = new();

        public BotsTwitch()
        {
            AddBot(TwitchBotChatClient);
            AddBot(TwitchFollower);
            AddBot(TwitchLiveMonitor);
            AddBot(TwitchBotClipSvc);

            TwitchBotChatClient.OnBotStarted += TwitchBotChatClient_OnBotStarted;
            TwitchFollower.OnBotStarted += TwitchFollower_OnBotStarted;
            TwitchLiveMonitor.OnBotStarted += TwitchLiveMonitor_OnBotStarted;
            TwitchBotClipSvc.OnBotStarted += TwitchBotClipSvc_OnBotStarted;

            OnCompletedDownloadFollowers += BotsTwitch_OnCompletedDownloadFollowers;

            TwitchBotUserSvc.ConnectUserService();
        }

        private void RegisterHandlers()
        {
            if (TwitchBotChatClient.IsStarted && !TwitchBotChatClient.HandlersAdded)
            {
                TwitchBotChatClient.TwitchChat.OnBeingHosted += Client_OnBeingHosted;
                TwitchBotChatClient.TwitchChat.OnChatCommandReceived += Client_OnChatCommandReceived;
                TwitchBotChatClient.TwitchChat.OnCommunitySubscription += Client_OnCommunitySubscription;
                TwitchBotChatClient.TwitchChat.OnExistingUsersDetected += Client_OnExistingUsersDetected;
                TwitchBotChatClient.TwitchChat.OnGiftedSubscription += Client_OnGiftedSubscription;
                TwitchBotChatClient.TwitchChat.OnJoinedChannel += Client_OnJoinedChannel;
                TwitchBotChatClient.TwitchChat.OnMessageReceived += Client_OnMessageReceived;
                TwitchBotChatClient.TwitchChat.OnMessageThrottled += Client_OnMessageThrottled;
                TwitchBotChatClient.TwitchChat.OnNewSubscriber += Client_OnNewSubscriber;
                TwitchBotChatClient.TwitchChat.OnNowHosting += Client_OnNowHosting;
                TwitchBotChatClient.TwitchChat.OnRaidNotification += Client_OnRaidNotification;
                TwitchBotChatClient.TwitchChat.OnReSubscriber += Client_OnReSubscriber;
                TwitchBotChatClient.TwitchChat.OnRitualNewChatter += Client_OnRitualNewChatter;
                TwitchBotChatClient.TwitchChat.OnUserBanned += Client_OnUserBanned;
                TwitchBotChatClient.TwitchChat.OnUserJoined += Client_OnUserJoined;
                TwitchBotChatClient.TwitchChat.OnUserLeft += Client_OnUserLeft;
                TwitchBotChatClient.TwitchChat.OnUserTimedout += Client_OnUserTimedout;

                TwitchBotChatClient.HandlersAdded = true;
            }

            if (TwitchFollower.IsStarted && !TwitchFollower.HandlersAdded)
            {
                TwitchFollower.FollowerService.OnNewFollowersDetected += FollowerService_OnNewFollowersDetected;

                TwitchFollower.HandlersAdded = true;
            }

            if (TwitchLiveMonitor.IsStarted && !TwitchLiveMonitor.HandlersAdded)
            {
                TwitchLiveMonitor.LiveStreamMonitor.OnStreamOnline += LiveStreamMonitor_OnStreamOnline;
                TwitchLiveMonitor.LiveStreamMonitor.OnStreamUpdate += LiveStreamMonitor_OnStreamUpdate;
                TwitchLiveMonitor.LiveStreamMonitor.OnStreamOffline += LiveStreamMonitor_OnStreamOffline;

                TwitchLiveMonitor.HandlersAdded = true;
            }

            if (TwitchBotClipSvc.IsStarted && !TwitchBotClipSvc.HandlersAdded)
            {
                TwitchBotClipSvc.clipMonitorService.OnNewClipFound += ClipMonitorServiceOnNewClipFound;

                TwitchBotClipSvc.HandlersAdded = true;
            }
        }

        #region Twitch Bot Chat Client

        private void TwitchBotChatClient_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();
        }

        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchNewSubscriber, e);
        }

        private void Client_OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchReSubscriber, e);
        }
        
        private void Client_OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            throw new NotImplementedException();
        }

        private void Client_OnUserTimedout(object sender, OnUserTimedoutArgs e)
        {
            throw new NotImplementedException();
        }

        private void Client_OnUserLeft(object sender, OnUserLeftArgs e)
        {
            throw new NotImplementedException();
        }

        private void Client_OnUserJoined(object sender, OnUserJoinedArgs e)
        {
            throw new NotImplementedException();
        }

        private void Client_OnUserBanned(object sender, OnUserBannedArgs e)
        {
            throw new NotImplementedException();
        }

        private void Client_OnRitualNewChatter(object sender, OnRitualNewChatterArgs e)
        {
            throw new NotImplementedException();
        }



        private void Client_OnRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            throw new NotImplementedException();
        }

        private void Client_OnNowHosting(object sender, OnNowHostingArgs e)
        {
            throw new NotImplementedException();
        }



        private void Client_OnMessageThrottled(object sender, OnMessageThrottledEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            throw new NotImplementedException();
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            throw new NotImplementedException();
        }



        private void Client_OnExistingUsersDetected(object sender, OnExistingUsersDetectedArgs e)
        {
            throw new NotImplementedException();
        }

        private void Client_OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
        {
            throw new NotImplementedException();
        }

        private void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            throw new NotImplementedException();
        }

        private void Client_OnBeingHosted(object sender, OnBeingHostedArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Follower Bot

        private void TwitchFollower_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();

            GetAllFollowers();
        }

        private void BotsTwitch_OnCompletedDownloadFollowers(object sender, OnNewFollowersDetectedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchBulkPostFollowers, e);
        }

        private void FollowerService_OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchPostNewFollowers, e);
        }

        #endregion

        #region Twitch LiveMonitor

        private void TwitchLiveMonitor_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();
        }

        public TwitchBotLiveMonitorSvc GetLiveMonitorSvc()
        {
            return TwitchLiveMonitor;
        }

        private void LiveStreamMonitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            if (OptionFlags.TwitchChatBotDisconnectOffline && TwitchBotChatClient.IsStarted)
            {
                TwitchBotChatClient.StopBot();
            }

            if (OptionFlags.IsStreamOnline)
            {
                InvokeBotEvent(this, BotEvents.TwitchStreamOffline, e);
            }
        }

        private void LiveStreamMonitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchStreamUpdate, e);
        }

        private void LiveStreamMonitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            if (e.Channel != TwitchBotsBase.TwitchChannelName)
            {
                TwitchLiveMonitor.SendMultiLiveMsg(e);
            }
            else
            {
                if (OptionFlags.TwitchChatBotConnectOnline && TwitchBotChatClient.IsStopped)
                {
                    TwitchBotChatClient.StartBot();
                }

                InvokeBotEvent(this, BotEvents.TwitchStreamOnline, e);
            }
        }

        #endregion

        #region Twitch Bot Clip Service

        private void TwitchBotClipSvc_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();
        }

        public void ClipMonitorServiceOnNewClipFound(object sender, OnNewClipsDetectedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchPostNewClip, e);
        }

        #endregion

        #region Threaded Ops

        public void GetAllFollowers()
        {
            if (OptionFlags.ManageFollowers && OptionFlags.TwitchAddFollowersStart && TwitchFollower.IsStarted)
            {
                new Thread(new ThreadStart(() =>
                {
                    string ChannelName = TwitchBotsBase.TwitchChannelName;

                    List<Follow> follows = TwitchFollower.GetAllFollowersAsync().Result;

                    OnCompletedDownloadFollowers?.Invoke(this, new() { NewFollowers = follows });
                })).Start();
            }
        }

        #endregion



 
    }
}
