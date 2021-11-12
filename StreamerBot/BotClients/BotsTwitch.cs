using StreamerBot.BotClients.Twitch;
using StreamerBot.Enum;
using StreamerBot.Events;
using StreamerBot.Static;

using System;
using System.Collections.Generic;
using System.Threading;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services.Events.FollowerService;

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
            AddBot(TwitchFollower);

            TwitchFollower.OnServiceConnected += TwitchFollower_OnServiceConnected;
            TwitchFollower.OnBotStarted += TwitchFollower_OnBotStarted;
            OnCompletedDownloadFollowers += BotsTwitch_OnCompletedDownloadFollowers;
        }

        private void TwitchFollower_OnBotStarted(object sender, EventArgs e)
        {
            GetAllFollowers();
        }

        private void TwitchFollower_OnServiceConnected(object sender, EventArgs e)
        {
            TwitchFollower.FollowerService.OnNewFollowersDetected += FollowerService_OnNewFollowersDetected;
        }

        private void BotsTwitch_OnCompletedDownloadFollowers(object sender, OnNewFollowersDetectedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchBulkPostFollowers, e);
        }

        private void FollowerService_OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchPostNewFollowers, e);
        }

        #region Threaded Ops
        
        public void GetAllFollowers()
        {
            if (OptionFlags.ManageFollowers && OptionFlags.TwitchAddFollowersStart && TwitchFollower.IsStarted)
            {
                new Thread(new ThreadStart(()=> {
                    string ChannelName = TwitchBotsBase.TwitchChannelName;

                    List<Follow> follows = TwitchFollower.GetAllFollowersAsync().Result;

                    OnCompletedDownloadFollowers?.Invoke(this, new() { NewFollowers = follows });
                })).Start();
            }
        }

        #endregion

        public TwitchBotLiveMonitorSvc GetLiveMonitorSvc()
        {
            return TwitchLiveMonitor;
        }


    }
}
