using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.Culture;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;

using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;
using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;

namespace StreamerBotLib.BotClients
{
    public class BotsTwitch : BotsBase
    {
        // TODO: fix condition where OAuth token expires or becomes invalid (if account password changes), yet bot continues to try to run and should otherwise stop if date expires
        public static TwitchBotChatClient TwitchBotChatClient { get; private set; } = new();
        public static TwitchBotFollowerSvc TwitchFollower { get; private set; } = new();
        public static TwitchBotLiveMonitorSvc TwitchLiveMonitor { get; private set; } = new();
        public static TwitchBotClipSvc TwitchBotClipSvc { get; private set; } = new();
        public static TwitchBotUserSvc TwitchBotUserSvc { get; private set; } = new();
        public static TwitchBotPubSub TwitchBotPubSub { get; private set; } = new();

        internal static TwitchTokenBot TwitchTokenBot { get; private set; } = new();

        private Thread BulkLoadFollows;
        private Thread BulkLoadClips;

        private const int BulkFollowSkipCount = 1000;

        public static event EventHandler<EventArgs> RaidCompleted;
        public event EventHandler<InvalidAccessTokenEventArgs> InvalidTwitchAccess;

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
            TwitchBotChatClient.UnRegisterHandlers += TwitchBotChatClient_UnRegisterHandlers;

            TwitchFollower.OnBotStarted += TwitchFollower_OnBotStarted;
            TwitchLiveMonitor.OnBotStarted += TwitchLiveMonitor_OnBotStarted;
            TwitchBotClipSvc.OnBotStarted += TwitchBotClipSvc_OnBotStarted;
            TwitchBotUserSvc.GetChannelGameName += TwitchBotUserSvc_GetChannelGameName;
            TwitchBotUserSvc.StartRaidEventResponse += TwitchBotUserSvc_StartRaidEventResponse;
            TwitchBotUserSvc.CancelRaidEvent += TwitchBotUserSvc_CancelRaidEvent;
            TwitchBotUserSvc.GetStreamsViewerCount += TwitchBotUserSvc_OnGetStreamsViewerCount;
            TwitchBotPubSub.OnBotStarted += TwitchBotPubSub_OnBotStarted;

            TwitchTokenBot.BotAcctAuthCodeExpired += TwitchTokenBot_BotAcctAuthCodeExpired;
            TwitchTokenBot.StreamerAcctAuthCodeExpired += TwitchTokenBot_StreamerAcctAuthCodeExpired;

            DataManager = SystemsController.DataManage;

            // assign token bot to every bot for shared access and refresh token sync
            foreach (TwitchBotsBase bot in BotsList)
            {
                bot.SetTokenBot(TwitchTokenBot);
            }

            TwitchBotUserSvc.SetTokenBot(TwitchTokenBot);

            AddBot(TwitchTokenBot);
        }


        public override void ManageStreamOnlineOfflineStatus(bool Start)
        {
            if (Start)
            {
                if (OptionFlags.TwitchChatBotConnectOnline)
                {
                    TwitchBotChatClient.StartBot();
                }

                if (OptionFlags.TwitchPubSubOnlineMode)
                {
                    TwitchBotPubSub.StartBot();
                }

                if (OptionFlags.TwitchFollowerConnectOnline)
                {
                    TwitchFollower.StartBot();
                }

                if (OptionFlags.TwitchClipConnectOnline)
                {
                    TwitchBotClipSvc.StartBot();
                }
            }
            else
            {
                if (OptionFlags.TwitchChatBotDisconnectOffline && TwitchBotChatClient.IsStarted)
                {
                    TwitchBotChatClient.StopBot();
                }

                if (OptionFlags.TwitchPubSubOnlineMode && TwitchBotPubSub.IsStarted)
                {
                    TwitchBotPubSub.StopBot();
                }

                if (OptionFlags.TwitchClipDisconnectOffline && TwitchBotClipSvc.IsStarted)
                {
                    TwitchBotClipSvc.StopBot();
                }

                if (OptionFlags.TwitchFollowerDisconnectOffline && TwitchFollower.IsStarted)
                {
                    TwitchFollower.StopBot();
                }
            }
        }

        private void TwitchBotUserSvc_GetChannelGameName(object sender, OnGetChannelGameNameEventArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchCategoryUpdate, e);
        }

        private void RegisterHandlers()
        {
            if (TwitchBotChatClient.IsStarted && !TwitchBotChatClient.HandlersAdded)
            {
                TwitchBotChatClient.TwitchChat.OnChatCommandReceived += Client_OnChatCommandReceived;
                TwitchBotChatClient.TwitchChat.OnCommunitySubscription += Client_OnCommunitySubscription;
                TwitchBotChatClient.TwitchChat.OnExistingUsersDetected += Client_OnExistingUsersDetected;
                TwitchBotChatClient.TwitchChat.OnGiftedSubscription += Client_OnGiftedSubscription;
                TwitchBotChatClient.TwitchChat.OnJoinedChannel += Client_OnJoinedChannel;
                TwitchBotChatClient.TwitchChat.OnMessageReceived += Client_OnMessageReceived;
                TwitchBotChatClient.TwitchChat.OnNewSubscriber += Client_OnNewSubscriber;
                TwitchBotChatClient.TwitchChat.OnRaidNotification += Client_OnRaidNotification;
                TwitchBotChatClient.TwitchChat.OnReSubscriber += Client_OnReSubscriber;
                TwitchBotChatClient.TwitchChat.OnUserBanned += Client_OnUserBanned;
                TwitchBotChatClient.TwitchChat.OnUserJoined += Client_OnUserJoined;
                TwitchBotChatClient.TwitchChat.OnUserLeft += Client_OnUserLeft;
                TwitchBotChatClient.TwitchChat.OnUserTimedout += Client_OnUserTimedout;
                TwitchBotChatClient.TwitchChat.OnMessageCleared += Client_OnMessageCleared;

                TwitchBotChatClient.HandlersAdded = true;
            }

            if (TwitchFollower.IsStarted && !TwitchFollower.HandlersAdded)
            {
                TwitchFollower.FollowerService.OnNewFollowersDetected += FollowerService_OnNewFollowersDetected;
                TwitchFollower.FollowerService.OnBulkFollowsUpdate += FollowerService_OnBulkFollowsUpdate;

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

            ThreadManager.CreateThreadStart(() =>
                TwitchBotUserSvc.SetIds(
                    DataManager.GetUserId(new(TwitchBotChatClient.TwitchChannelName, Platform.Twitch)),
                    DataManager.GetUserId(new(TwitchBotChatClient.TwitchBotUserName, Platform.Twitch))
                    )
            );
        }

        private void TwitchBotChatClient_UnRegisterHandlers(object sender, EventArgs e)
        {
            if (TwitchBotChatClient.HandlersAdded)
            {
                TwitchBotChatClient.TwitchChat.OnChatCommandReceived -= Client_OnChatCommandReceived;
                TwitchBotChatClient.TwitchChat.OnCommunitySubscription -= Client_OnCommunitySubscription;
                TwitchBotChatClient.TwitchChat.OnExistingUsersDetected -= Client_OnExistingUsersDetected;
                TwitchBotChatClient.TwitchChat.OnGiftedSubscription -= Client_OnGiftedSubscription;
                TwitchBotChatClient.TwitchChat.OnJoinedChannel -= Client_OnJoinedChannel;
                TwitchBotChatClient.TwitchChat.OnMessageReceived -= Client_OnMessageReceived;
                TwitchBotChatClient.TwitchChat.OnNewSubscriber -= Client_OnNewSubscriber;
                TwitchBotChatClient.TwitchChat.OnRaidNotification -= Client_OnRaidNotification;
                TwitchBotChatClient.TwitchChat.OnReSubscriber -= Client_OnReSubscriber;
                TwitchBotChatClient.TwitchChat.OnUserBanned -= Client_OnUserBanned;
                TwitchBotChatClient.TwitchChat.OnUserJoined -= Client_OnUserJoined;
                TwitchBotChatClient.TwitchChat.OnUserLeft -= Client_OnUserLeft;
                TwitchBotChatClient.TwitchChat.OnUserTimedout -= Client_OnUserTimedout;
                TwitchBotChatClient.TwitchChat.OnMessageCleared -= Client_OnMessageCleared;

                TwitchBotChatClient.HandlersAdded = false;
            }
        }

        #region Twitch Bot Chat Client

        private void TwitchBotChatClient_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();

            InvokeBotEvent(this, BotEvents.TwitchChatBotStarted, null);

            if (OptionFlags.TwitchChatBotConnectOnline || OptionFlags.IsStreamOnline)
            {
                InvokeBotEvent(this, BotEvents.TwitchOnUserJoined, new StreamerOnUserJoinedArgs() { LiveUser = AddUserId(TwitchBotsBase.TwitchBotUserName) });
            }
        }

        private void TwitchBotChatClient_OnBotStopping(object sender, EventArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchChatBotStopping, new());
        }

        private void TwitchBotChatClient_OnBotStopped(object sender, EventArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchChatBotStopped, new());

            TwitchBotChatClient.HandlersAdded = false;
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

        //private void Client_OnBeingHosted(object sender, OnBeingHostedArgs e)
        //{
        //    InvokeBotEvent(this, BotEvents.TwitchBeingHosted, e);
        //}

        private void Client_OnNowHosting(object sender, OnNowHostingArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchNowHosting, e);
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            if (OptionFlags.MsgBotConnection)
            {
                Version version = Assembly.GetEntryAssembly().GetName().Version;
                string s = string.Format(CultureInfo.CurrentCulture,
                    LocalizedMsgSystem.GetTwineBotAuthorInfo(), version.Major, version.Minor, version.Build, version.Revision);

                Send(s);
            }
        }

        private void Client_OnExistingUsersDetected(object sender, OnExistingUsersDetectedArgs e)
        {
            StreamerOnExistingUserDetectedArgs args = new()
            {
                Users = e.Users.ConvertAll((s) =>
                {
                    return AddUserId(s);
                })
            };

            InvokeBotEvent(this, BotEvents.TwitchExistingUsers, args);
        }

        private static Models.LiveUser AddUserId(string s)
        {
            Models.LiveUser user = new(s, Platform.Twitch);

            string userId = DataManager.GetUserId(user);
            if (userId == null || userId == string.Empty)
            {
                user.UserId = TwitchBotUserSvc.GetUserId(user.UserName);
            }
            else
            {
                user.UserId = userId;
            }
            return user;
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
            InvokeBotEvent(this, BotEvents.TwitchOnUserLeft, new StreamerOnUserLeftArgs() { LiveUser = AddUserId(e.Username) });
        }

        private void Client_OnUserJoined(object sender, OnUserJoinedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchOnUserJoined, new StreamerOnUserJoinedArgs() { LiveUser = AddUserId(e.Username) });
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

        private void Client_OnMessageCleared(object sender, OnMessageClearedArgs e)
        {

        }

        #endregion

        #region Twitch User Bot

        public static string GetUserCategory(string UserId = null, string UserName = null)
        {
            return TwitchBotUserSvc.GetUserGameCategory(UserId: UserId, UserName: UserName);
        }

        public static DateTime GetUserAccountAge(string UserId = null, string UserName = null)
        {
            return TwitchBotUserSvc.GetUserCreatedAt(UserName: UserName, UserId: UserId);
        }

        public static bool VerifyUserExist(string UserName)
        {
            return TwitchBotUserSvc.GetUserId(UserName) != null;
        }

        public void BanUserRequest(string UserName, BanReasons Reason, int Duration = 0)
        {
            TwitchBotUserSvc.BanUser(UserName, Reason, Duration);
        }

        public static bool ModifyChannelInformation(string Title = null, string CategoryName = null, string CategoryId = null)
        {
            bool result = false;

            if (Title != null)
            {
                result = TwitchBotUserSvc.SetChannelTitle(Title);
            }
            if (CategoryName != null || CategoryId != null)
            {
                result = TwitchBotUserSvc.SetChannelCategory(CategoryName, CategoryId);
            }

            return result;
        }

        public string GetUserId(string UserName)
        {
            return TwitchBotUserSvc.GetUserId(UserName);
        }

        public static void RaidChannel(string ToUserName)
        {
            TwitchBotUserSvc.RaidChannel(ToUserName);
        }

        public static void CancelRaidChannel()
        {
            TwitchBotUserSvc.CancelRaidChannel();
        }

        private void TwitchBotUserSvc_StartRaidEventResponse(object sender, OnStreamRaidResponseEventArgs e)
        {
            StartRaid(e.ToChannel, e.CreatedAt.ToLocalTime());
        }

        private void TwitchBotUserSvc_CancelRaidEvent(object sender, EventArgs e)
        {
            CancelRaidLoop();
        }

        public static void GetViewerCount()
        {
            TwitchBotUserSvc.GetViewerCount(TwitchBotsBase.TwitchChannelName);
        }
        private void TwitchBotUserSvc_OnGetStreamsViewerCount(object sender, GetStreamsEventArgs e)
        {
            PostInternalCommand(LocalizedMsgSystem.GetVar(DefaultCommand.uptime), new() { e.ViewerCount.ToString() }, $"!{LocalizedMsgSystem.GetVar(MsgVars.uptime)} {e.ViewerCount}");
        }

        internal void PostInternalCommand(string Com, List<string> ComArgs, string ComMessage)
        {
            InvokeBotEvent(this, BotEvents.TwitchBotCommandCall, new SendBotCommandEventArgs()
            {
                CmdMessage = new()
                {
                    CommandArguments = ComArgs,
                    CommandText = Com,
                    DisplayName = TwitchBotsBase.TwitchBotUserName,
                    Channel = TwitchBotsBase.TwitchChannelName,
                    IsBroadcaster = TwitchBotsBase.TwitchBotUserName == TwitchBotsBase.TwitchChannelName,
                    IsHighlighted = false,
                    IsMe = false,
                    IsModerator = TwitchBotsBase.TwitchBotUserName != TwitchBotsBase.TwitchChannelName,
                    IsPartner = false,
                    IsSkippingSubMode = false,
                    IsStaff = false,
                    IsSubscriber = false,
                    IsTurbo = false,
                    IsVip = false,
                    Message = ComMessage
                }
            });
        }

        #endregion

        #region Follower Bot

        private void TwitchFollower_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();

            if (!TwitchFollower.RestartRefreshAccessToken) // avoid the bulk follower call due to access token refresh
            {
                // otherwise, update all followers per the user's action to restart the bot
                GetAllFollowers();
            }
        }

        private void FollowerService_OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
            List<ChannelFollower> newFollows = new();

            foreach (ChannelFollower F in e.NewFollowers)
            {
                if (!DataManager.CheckFollower(F.UserName))
                {
                    newFollows.Add(F);
                }
            }

            if (newFollows.Count > 0)
            {
                e.NewFollowers = newFollows;
                InvokeBotEvent(this, BotEvents.TwitchPostNewFollowers, e);
            }
        }

        #endregion

        #region Twitch LiveMonitor

        private void TwitchLiveMonitor_OnBotStarted(object sender, EventArgs e)
        {
            RegisterHandlers();
        }

        public static TwitchBotLiveMonitorSvc LiveMonitorSvc => TwitchLiveMonitor;

        private void LiveStreamMonitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            if (e.Channel != TwitchBotsBase.TwitchChannelName)
            {
                TwitchLiveMonitor.SendMultiLiveMsg(e);
            }
            else
            {
                InvokeBotEvent(this, BotEvents.TwitchStreamOnline, e);

                if (!OptionFlags.TwitchChatBotConnectOnline && TwitchBotChatClient.IsStarted)
                {
                    InvokeBotEvent(this, BotEvents.TwitchOnUserJoined, new OnUserJoinedArgs() { Username = TwitchBotsBase.TwitchBotUserName, Channel = TwitchBotsBase.TwitchChannelName });
                }
            }
        }

        private void LiveStreamMonitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
            if (e.Channel == TwitchBotsBase.TwitchChannelName)
            {
                InvokeBotEvent(this, BotEvents.TwitchStreamUpdate, e);
            }
        }

        private void LiveStreamMonitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            if (e.Channel == TwitchBotsBase.TwitchChannelName)
            {
                if (OptionFlags.IsStreamOnline)
                {
                    InvokeBotEvent(this, BotEvents.TwitchStreamOffline, e);
                }
            }
        }

        private void CheckStreamOnlineChatBot()
        {
            while (OptionFlags.IsStreamOnline && !TwitchBotChatClient.IsStarted)
            {
                if (OptionFlags.TwitchChatBotConnectOnline)
                {
                    TwitchBotChatClient.StartBot();
                }
                Thread.Sleep(5000);
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

        /// <summary>
        /// Get the clips for a specific user channel.
        /// </summary>
        /// <param name="ChannelName">Channel to get the clips.</param>
        /// <param name="ReturnData">The callback method when the clips are found.</param>
        public void GetChannelClips(string ChannelName, Action<List<Models.Clip>> ReturnData)
        {
            ThreadManager.CreateThreadStart(() => ProcessChannelClips(ChannelName, ReturnData));
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

        #region Twitch Token Bot

        public void TwitchActivateAuthCode(string clientId, Action<string> action, Action AuthenticationFinished)
        {
            ThreadManager.CreateThreadStart(() =>
            {
                TwitchTokenBot.GenerateAuthCodeURL(clientId, action, AuthenticationFinished);
            });
        }

        private void TwitchTokenBot_StreamerAcctAuthCodeExpired(object sender, TwitchAuthCodeExpiredEventArgs e)
        {
            if (e?.CallAction != null)
            {
                // start an http listener to receive auth code
                ThreadManager.CreateThreadStart(() =>
                {
                    HttpListener httpListener = new();
                    httpListener.Prefixes.Add(OptionFlags.TwitchAuthRedirectURL.EndsWith('/') ? OptionFlags.TwitchAuthRedirectURL : $"{OptionFlags.TwitchAuthRedirectURL}/");

                    httpListener.Start();
                    HttpListenerRequest request = httpListener.GetContext().Request;

                    Uri uridata = request.Url;
                    httpListener.Close();

                    /*
                     * from: https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#authorization-code-grant-flow
                     * expected return, affirm or deny, when attempting to authorize the application
                     * 
                     If the user authorized your app by clicking Authorize, the server sends the authorization code to your redirect URI (see the code query parameter):

    http://localhost:3000/
        ?code=gulfwdmys5lsm6qyz4xiz9q32l10
        &scope=channel%3Amanage%3Apolls+channel%3Aread%3Apolls
        &state=c3ab8aa609ea11e793ae92361f002671

    If the user didn’t authorize your app, the server sends the error code and description to your redirect URI (see the error and error_description query parameters):

    http://localhost:3000/
        ?error=access_denied
        &error_description=The+user+denied+you+access
        &state=c3ab8aa609ea11e793ae92361f002671

                     */

                    var QueryValues = HttpUtility.ParseQueryString(uridata.Query);

                    if (!QueryValues.AllKeys.Contains("error") && QueryValues["state"] == e.State)
                    {
                        OptionFlags.TwitchAuthStreamerAuthCode = QueryValues["code"];
                        TwitchTokenBot.CheckToken(); // proceed with getting a refresh token and access token
                    }
                    else
                    {
                        e.CallAction(Msgs.MsgTwitchAuthFailedAuthentication);
                        OptionFlags.TwitchAuthStreamerAuthCode = null;
                    }
                });

                // call the provided method to give user the web based app authorization URL
                e.CallAction(e.AuthURL);
            }
            else
            {
                foreach (IIOModule bot in BotsList)
                {
                    bot.StopBot();
                }
                InvalidTwitchAccess?.Invoke(this, new(Platform.Twitch, e.BotType));
            }
        }

        private void TwitchTokenBot_BotAcctAuthCodeExpired(object sender, TwitchAuthCodeExpiredEventArgs e)
        {
            if (e.CallAction != null)
            {
                // start an http listener to receive auth code
                ThreadManager.CreateThreadStart(() =>
                {
                    HttpListener httpListener = new();
                    httpListener.Prefixes.Add(OptionFlags.TwitchAuthRedirectURL.EndsWith('/') ? OptionFlags.TwitchAuthRedirectURL : $"{OptionFlags.TwitchAuthRedirectURL}/");

                    httpListener.Start();
                    HttpListenerRequest request = httpListener.GetContext().Request;

                    Uri uridata = request.Url;
                    httpListener.Close();

                    /*
                     * from: https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#authorization-code-grant-flow
                     * expected return, affirm or deny, when attempting to authorize the application
                     * 
                     If the user authorized your app by clicking Authorize, the server sends the authorization code to your redirect URI (see the code query parameter):

    http://localhost:3000/
        ?code=gulfwdmys5lsm6qyz4xiz9q32l10
        &scope=channel%3Amanage%3Apolls+channel%3Aread%3Apolls
        &state=c3ab8aa609ea11e793ae92361f002671

    If the user didn’t authorize your app, the server sends the error code and description to your redirect URI (see the error and error_description query parameters):

    http://localhost:3000/
        ?error=access_denied
        &error_description=The+user+denied+you+access
        &state=c3ab8aa609ea11e793ae92361f002671

                     */

                    var QueryValues = HttpUtility.ParseQueryString(uridata.Query);

                    if (!QueryValues.AllKeys.Contains("error") && QueryValues["state"] == e.State)
                    {
                        OptionFlags.TwitchAuthBotAuthCode = QueryValues["code"];
                        TwitchTokenBot.CheckToken(); // proceed with getting a refresh token and access token
                    }
                    else
                    {
                        e.CallAction(Msgs.MsgTwitchAuthFailedAuthentication);
                        OptionFlags.TwitchAuthBotAuthCode = null;
                    }
                });

                // call the provided method to give user the web based app authorization URL
                e.CallAction(e.AuthURL);
            }
            else
            {
                foreach (IIOModule bot in BotsList)
                {
                    bot.StopBot();
                }
                InvalidTwitchAccess?.Invoke(this, new(Platform.Twitch, e.BotType));
            }
        }

        #endregion

        #region Threaded Ops

        public override void GetAllFollowers()
        {
            GetAllFollowers(false);
        }

        public override void GetAllFollowers(bool OverrideUpdateFollowers = false)
        {
            if (OptionFlags.ManageFollowers && (OptionFlags.TwitchAddFollowersStart || OverrideUpdateFollowers) && TwitchFollower.IsStarted)
            {
                BulkLoadFollows = ThreadManager.CreateThread(() =>
                {
                    string ChannelName = TwitchBotsBase.TwitchChannelName;

                    InvokeBotEvent(this, BotEvents.TwitchStartBulkFollowers, null);

                    try
                    {
                        while (!TwitchFollower.GetAllFollowersBulkAsync().Result) { }
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                    }

                    InvokeBotEvent(this, BotEvents.TwitchStopBulkFollowers, null);
                });
                MultiThreadOps.Add(BulkLoadFollows);
                BulkLoadFollows.Start();
            }
        }

        private void FollowerService_OnBulkFollowsUpdate(object sender, OnNewFollowersDetectedArgs e)
        {
            InvokeBotEvent(
                this,
                BotEvents.TwitchBulkPostFollowers,
                new OnNewFollowersDetectedArgs()
                {
                    NewFollowers = e.NewFollowers
                });
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

        private async void ProcessChannelClips(string ChannelName, Action<List<Models.Clip>> ActionCallback)
        {
            List<Clip> result = new();
            TwitchBotClipSvc ChannelClips = new();
            if (ChannelClips.StartBot())
            {
                result = await ChannelClips.GetAllClipsAsync(ChannelName);
                ChannelClips.StopBot();
            }

            ActionCallback?.Invoke(BotIOController.BotController.ConvertClips(result));
        }

        private readonly TimeSpan DefaultOutRaid = new(0, 0, 90);
        private DateTime OutRaidStarted;
        private Thread RaidLoop = null;
        private readonly string RaidLock = "lock";

        private void StartRaid(string ToChannelName, DateTime RaidCreated)
        {
            lock (RaidLock)
            {
                OutRaidStarted = RaidCreated;
            }

            if (RaidLoop == null && OptionFlags.IsStreamOnline) // create only 1 thread & when stream is online
            {
                RaidLoop = ThreadManager.CreateThread(() =>
                {
                    // declare locals, so we can use a thread-safe lock
                    DateTime LocalRaidStart;
                    bool LocalRaidStarted;

                    lock (RaidLock)
                    {
                        LocalRaidStart = OutRaidStarted;
                        LocalRaidStarted = OptionFlags.TwitchOutRaidStarted;
                    }

                    while (DateTime.Now - LocalRaidStart <= DefaultOutRaid && LocalRaidStarted)
                    { // check for 90 seconds
                        lock (RaidLock)
                        { // update values, in case they changed - use thread lock safety as another thread may change these
                            LocalRaidStart = OutRaidStarted;
                            LocalRaidStarted = OptionFlags.TwitchOutRaidStarted;
                        }
                        Thread.Sleep(10);
                    }

                    // if the raid wasn't canceled after the loop finished, send raid event to main bot
                    if (LocalRaidStarted)
                    {
                        InvokeBotEvent(this, BotEvents.TwitchOutgoingRaid,
                            new OnStreamRaidResponseEventArgs()
                            {
                                ToChannel = ToChannelName,
                                CreatedAt = OutRaidStarted
                            });

                        RaidCompleted?.Invoke(this, new());
                    }
                    RaidLoop = null;
                });
                RaidLoop.Start();
            }

        }

        private void CancelRaidLoop()
        {
            lock (RaidLock)
            {
                OptionFlags.TwitchOutRaidStarted = false;
            }
        }

        #endregion


    }
}
