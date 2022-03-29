﻿using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;

namespace StreamerBotLib.BotClients
{
    public class BotsTwitch : BotsBase
    {
        public static TwitchBotChatClient TwitchBotChatClient { get; private set; } = new();
        public static TwitchBotFollowerSvc TwitchFollower { get; private set; } = new();
        public static TwitchBotLiveMonitorSvc TwitchLiveMonitor { get; private set; } = new();
        public static TwitchBotClipSvc TwitchBotClipSvc { get; private set; } = new();
        public static TwitchBotUserSvc TwitchBotUserSvc { get; private set; } = new();
        public static TwitchBotPubSub TwitchBotPubSub { get; private set; } = new();

        private Thread BulkLoadFollows;
        private Thread BulkLoadClips;

        private const int BulkFollowSkipCount = 1000;

        public BotsTwitch()
        {
            // not including "TwitchBotUserSvc" bot, it's an authentication on-demand bot to get the info
            AddBot(TwitchFollower);
            AddBot(TwitchLiveMonitor);
            AddBot(TwitchBotClipSvc);
            AddBot(TwitchBotChatClient);
            AddBot(TwitchBotPubSub);

            TwitchBotChatClient.OnBotStarted += TwitchBotChatClient_OnBotStarted;
            TwitchBotChatClient.OnBotStopping += TwitchBotChatClient_OnBotStopping;
            TwitchBotChatClient.OnBotStopped += TwitchBotChatClient_OnBotStopped;
            TwitchFollower.OnBotStarted += TwitchFollower_OnBotStarted;
            TwitchLiveMonitor.OnBotStarted += TwitchLiveMonitor_OnBotStarted;
            TwitchBotClipSvc.OnBotStarted += TwitchBotClipSvc_OnBotStarted;
            TwitchBotUserSvc.GetChannelGameName += TwitchBotUserSvc_GetChannelGameName;
            TwitchBotPubSub.OnBotStarted += TwitchBotPubSub_OnBotStarted;
        }

        private void TwitchBotUserSvc_GetChannelGameName(object sender, OnGetChannelGameNameEventArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchCategoryUpdate, e);
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
                TwitchBotClipSvc.ClipMonitorService.OnNewClipFound += ClipMonitorServiceOnNewClipFound;

                TwitchBotClipSvc.HandlersAdded = true;
            }

            if (TwitchBotPubSub.IsStarted && !TwitchBotPubSub.HandlersAdded)
            {
                TwitchBotPubSub.TwitchPubSub.OnChannelPointsRewardRedeemed += TwitchPubSub_OnChannelPointsRewardRedeemed;
                TwitchBotPubSub.HandlersAdded = true;
            }
        }


        #region Twitch Bot Chat Client

        private void TwitchBotChatClient_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();

            InvokeBotEvent(this, BotEvents.TwitchChatBotStarted, new());
        }

        private void TwitchBotChatClient_OnBotStopping(object sender, EventArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchChatBotStopping, new());
        }

        private void TwitchBotChatClient_OnBotStopped(object sender, EventArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchChatBotStopped, new());
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
            InvokeBotEvent(this, BotEvents.TwitchGiftSubscription, e);
        }

        private void Client_OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchCommunitySubscription, e);
        }

        private void Client_OnBeingHosted(object sender, OnBeingHostedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchBeingHosted, e);
        }

        private void Client_OnNowHosting(object sender, OnNowHostingArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchNowHosting, e);
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            if (IOModule.ShowConnectionMsg)
            {
                Version version = Assembly.GetEntryAssembly().GetName().Version;
                string s = string.Format(CultureInfo.CurrentCulture,
                    LocalizedMsgSystem.GetTwineBotAuthorInfo(), version.Major, version.Minor, version.Build, version.Revision);

                Send(s);
            }
        }

        private void Client_OnExistingUsersDetected(object sender, OnExistingUsersDetectedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchExistingUsers, e);
        }

        private void Client_OnUserBanned(object sender, OnUserBannedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchOnUserBanned, e);
        }

        private void Client_OnUserTimedout(object sender, OnUserTimedoutArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchOnUserTimedout, e);
        }

        private void Client_OnUserLeft(object sender, OnUserLeftArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchOnUserLeft, e);
        }

        private void Client_OnUserJoined(object sender, OnUserJoinedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchOnUserJoined, e);
        }

        private void Client_OnRitualNewChatter(object sender, OnRitualNewChatterArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchRitualNewChatter, e);
        }

        private void Client_OnRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            string CategoryName = GetUserCategory(e.RaidNotification.UserId);

            InvokeBotEvent(this, BotEvents.TwitchIncomingRaid, new OnIncomingRaidArgs() { Category = CategoryName, RaidTime = DateTime.Now.ToLocalTime(), ViewerCount = e.RaidNotification.MsgParamViewerCount, DisplayName = e.RaidNotification.DisplayName });
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchMessageReceived, e);
        }

        private void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchChatCommandReceived, e);
        }

        #endregion

        #region Twitch User Bot

        public static string GetUserCategory(string UserId = null, string UserName = null)
        {
            return TwitchBotUserSvc.GetUserGameCategory(UserId: UserId, UserName: UserName);
        }

        public static bool VerifyUserExist(string UserName)
        {
            return TwitchBotUserSvc.GetUserId(UserName) != null;
        }

        #endregion

        #region Follower Bot

        private void TwitchFollower_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();

            GetAllFollowers();
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

        public static TwitchBotLiveMonitorSvc LiveMonitorSvc => TwitchLiveMonitor;

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
                InvokeBotEvent(this, BotEvents.TwitchOnUserJoined, new OnUserJoinedArgs() { Username = TwitchBotsBase.TwitchBotUserName, Channel = TwitchBotsBase.TwitchChannelName });
            }
        }

        #endregion

        #region Twitch Bot Clip Service

        private void TwitchBotClipSvc_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();

            // start thread to retrieve all clips
            BulkLoadClips = ThreadManager.CreateThread(ProcessClips);
            MultiThreadOps.Add(BulkLoadClips);
            BulkLoadClips.Start();
        }

        public void ClipMonitorServiceOnNewClipFound(object sender, OnNewClipsDetectedArgs e)
        {
            while (StartClips) { } // wait while receiving new clips

            InvokeBotEvent(this, BotEvents.TwitchPostNewClip, e);
        }

        #endregion

        #region PubSub
        private void TwitchBotPubSub_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();
        }

        private void TwitchPubSub_OnChannelPointsRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs e)
        {
            Twitch.TwitchLib.Events.PubSub.OnChannelPointsRewardRedeemedArgs local = new() { ChannelId = e.ChannelId, RewardRedeemed = e.RewardRedeemed };
            InvokeBotEvent(this, BotEvents.TwitchChannelPointsRewardRedeemed, local);
        }

        #endregion

        #region Threaded Ops

        public override void GetAllFollowers()
        {
            if (OptionFlags.ManageFollowers && OptionFlags.TwitchAddFollowersStart && TwitchFollower.IsStarted)
            {
                BulkLoadFollows = ThreadManager.CreateThread(() =>
                {
                    string ChannelName = TwitchBotsBase.TwitchChannelName;

                    InvokeBotEvent(this, BotEvents.TwitchStartBulkFollowers, new EventArgs());

                    // TODO: convert to permit Async to post significant followers to update in bulk, would otherwise generate significant memory to store until processed - consider creating a data stream
                    List<Follow> follows = TwitchFollower.GetAllFollowersAsync().Result;

                    follows.Reverse();

                    for (int i = 0; i < follows.Count; i++)
                    {
                        // break up the follower list so chunks of the big list are sent in parts via event
                        List<Follow> pieces = new(follows.Skip(i * BulkFollowSkipCount).Take(BulkFollowSkipCount));

                        InvokeBotEvent(
                            this,
                            BotEvents.TwitchBulkPostFollowers,
                            new OnNewFollowersDetectedArgs()
                            {
                                NewFollowers = pieces
                            });
                    }

                    InvokeBotEvent(this, BotEvents.TwitchStopBulkFollowers, new EventArgs());
                });
                MultiThreadOps.Add(BulkLoadFollows);
                BulkLoadFollows.Start();
            }
        }

        private List<Clip> ClipList { get; set; }

        private bool StartClips { get; set; }

        private void ProcessClips()
        {
            StartClips = true;
            ClipList = TwitchBotClipSvc.GetAllClipsAsync().Result;

            InvokeBotEvent(this, BotEvents.TwitchClipSvcOnClipFound, new ClipFoundEventArgs() { ClipList = ClipList });
            StartClips = false;
        }

        #endregion


    }
}
