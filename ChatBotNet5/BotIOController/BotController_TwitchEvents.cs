using ChatBot_Net5.Clients;
using ChatBot_Net5.Data;
using ChatBot_Net5.Enum;
using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;
using TwitchLib.Communication.Events;

namespace ChatBot_Net5.BotIOController
{
    public sealed partial class BotController
    {
        // TODO: Add Twitch Clips Service-posting to Discord, including getting a clip message, Discord clip link

        /// <summary>
        /// Register event handlers for the chat services
        /// </summary>
        private void RegisterHandlers()
        {
            if (TwitchIO.IsStarted && !TwitchIO.HandlersAdded)
            {
                TwitchIO.TwitchChat.OnBeingHosted += Client_OnBeingHosted;
                //TwitchIO.TwitchChat.OnChannelStateChanged += Client_OnChannelStateChanged;
                //TwitchIO.TwitchChat.OnChatCleared += Client_OnChatCleared;
                //TwitchIO.TwitchChat.OnChatColorChanged += Client_OnChatColorChanged;
                TwitchIO.TwitchChat.OnChatCommandReceived += Client_OnChatCommandReceived;
                TwitchIO.TwitchChat.OnCommunitySubscription += Client_OnCommunitySubscription;
                //TwitchIO.TwitchChat.OnConnectionError += Client_OnConnectionError;
                //TwitchIO.TwitchChat.OnError += Client_OnError;
                TwitchIO.TwitchChat.OnExistingUsersDetected += Client_OnExistingUsersDetected;
                //TwitchIO.TwitchChat.OnFailureToReceiveJoinConfirmation += Client_OnFailureToReceiveJoinConfirmation;
                TwitchIO.TwitchChat.OnGiftedSubscription += Client_OnGiftedSubscription;
                //TwitchIO.TwitchChat.OnHostingStarted += Client_OnHostingStarted;
                //TwitchIO.TwitchChat.OnHostingStopped += Client_OnHostingStopped;
                //TwitchIO.TwitchChat.OnHostLeft += Client_OnHostLeft;
                //TwitchIO.TwitchChat.OnIncorrectLogin += Client_OnIncorrectLogin;
                TwitchIO.TwitchChat.OnJoinedChannel += Client_OnJoinedChannel;
                //TwitchIO.TwitchChat.OnLeftChannel += Client_OnLeftChannel;
                //TwitchIO.TwitchChat.OnMessageCleared += Client_OnMessageCleared;
                TwitchIO.TwitchChat.OnMessageReceived += Client_OnMessageReceived;
                //TwitchIO.TwitchChat.OnMessageSent += Client_OnMessageSent;
                TwitchIO.TwitchChat.OnMessageThrottled += Client_OnMessageThrottled;
                TwitchIO.TwitchChat.OnModeratorJoined += Client_OnModeratorJoined;
                //TwitchIO.TwitchChat.OnModeratorLeft += Client_OnModeratorLeft;
                //TwitchIO.TwitchChat.OnModeratorsReceived += Client_OnModeratorsReceived;
                TwitchIO.TwitchChat.OnNewSubscriber += Client_OnNewSubscriber;
                //TwitchIO.TwitchChat.OnNoPermissionError += Client_OnNoPermissionError;
                //TwitchIO.TwitchChat.OnNowHosting += Client_OnNowHosting;
                //TwitchIO.TwitchChat.OnRaidedChannelIsMatureAudience += Client_OnRaidedChannelIsMatureAudience;
                TwitchIO.TwitchChat.OnRaidNotification += Client_OnRaidNotification;
                TwitchIO.TwitchChat.OnReSubscriber += Client_OnReSubscriber;
                TwitchIO.TwitchChat.OnRitualNewChatter += Client_OnRitualNewChatter;
                //TwitchIO.TwitchChat.OnSelfRaidError += Client_OnSelfRaidError;
                //TwitchIO.TwitchChat.OnSendReceiveData += Client_OnSendReceiveData;
                //TwitchIO.TwitchChat.OnUnaccountedFor += Client_OnUnaccountedFor;
                TwitchIO.TwitchChat.OnUserBanned += Client_OnUserBanned;
                TwitchIO.TwitchChat.OnUserJoined += Client_OnUserJoined;
                TwitchIO.TwitchChat.OnUserLeft += Client_OnUserLeft;
                //TwitchIO.TwitchChat.OnUserStateChanged += Client_OnUserStateChanged;
                TwitchIO.TwitchChat.OnUserTimedout += Client_OnUserTimedout;
                //TwitchIO.TwitchChat.OnVIPsReceived += Client_OnVIPsReceived;
                //TwitchIO.TwitchChat.OnWhisperCommandReceived += Client_OnWhisperCommandReceived;
                //TwitchIO.TwitchChat.OnWhisperReceived += Client_OnWhisperReceived;
                //TwitchIO.TwitchChat.OnWhisperSent += Client_OnWhisperSent;
                //TwitchIO.TwitchChat.OnWhisperThrottled += Client_OnWhisperThrottled;

                TwitchIO.HandlersAdded = true;
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
        }

        #region Stream On, Off, Updated
        /// <summary>
        /// Event called when the stream is detected to be offline.
        /// </summary>
        /// <param name="sender">The calling object.</param>
        /// <param name="e">Contains the offline arguments.</param>
        private void LiveStreamMonitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Stats.StreamOffline(DateTime.Now);
        }

        /// <summary>
        /// Event called when the stream is detected to be updated.
        /// </summary>
        /// <param name="sender">The calling object.</param>
        /// <param name="e">Contains the update arguments.</param>
        private void LiveStreamMonitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
            Stats.Category = e.Stream.GameName;
        }

        /// <summary>
        /// Event called when the stream is detected to be online.
        /// </summary>
        /// <param name="sender">The calling object.</param>
        /// <param name="e">Contains the online arguments.</param>
        private void LiveStreamMonitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            try
            {
                if (e.Channel != TwitchBots.TwitchChannelName)
                {
                    SendMultiLiveMsg(e);
                }
                else
                {
                    bool Started = Stats.StreamOnline(e.Stream.StartedAt);
                    Stats.Category = e.Stream.GameName;

                    if (Started)
                    {
                        bool MultiLive = DataManage.CheckMultiStreams(e.Stream.StartedAt);

                        if ((OptionFlags.PostMultiLive && MultiLive) || !MultiLive)
                        {
                            // get message, set a default if otherwise deleted/unavailable
                            string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Live);
                            msg ??= "@everyone, #user is now live streaming #category - #title! Come join and say hi at: #url";

                            // keys for exchanging codes for representative names
                            Dictionary<string, string> dictionary = VariableParser.BuildDictionary( new Tuple<string, string>[]
                            {
                                new("user", e.Stream.UserName),
                                new("category", e.Stream.GameName),
                                new("title", e.Stream.Title),
                                new("url", "https://www.twitch.tv/" + e.Stream.UserName)
                            });

                            foreach (Uri u in DataManage.GetWebhooks(WebhooksKind.Live))
                            {
                                DiscordWebhook.SendLiveMessage(u, VariableParser.ParseReplace(msg, dictionary)).Wait();
                                Stats.AddDiscord();
                            }
                        }
                    }
                }

            }
            catch { }
        }

        #endregion Stream On, Off, Updated

        #region Followers
        private void FollowerService_OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
            bool FollowEnabled = (bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.Follow);

            string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Follow);
            msg ??= "Thanks #user for the follow!";

            while (DataManage.UpdatingFollowers) { } // spin until the 'add followers when bot starts - this.ProcessFollows()' is finished

            foreach (Follow f in e.NewFollowers.Where(f => DataManage.AddFollower(f.FromUserName, f.FollowedAt)))
            {
                if (OptionFlags.ManageFollowers)
                {
                    if (FollowEnabled)
                    {
                        Send(msg.Replace("#user", "@" + f.FromUserName));
                    }

                    Stats.AddFollow();
                    Stats.AddAutoEvents();
                }

                if (OptionFlags.TwitchFollowerFollowBack)
                {
                    TwitchFollower.FollowBack(f.FromUserName);
                }
            }
        }

        #endregion Followers

        #region Subscriptions
        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.Subscribe))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Subscribe);
                msg ??= "Thanks #user for the subscribing!";


                Send(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<string, string>[] {
                new( "user", e.Subscriber.DisplayName ),
                new( "submonths", FormatData.Plurality(e.Subscriber.MsgParamCumulativeMonths, "total month", "total months") ),
                new( "subplan", e.Subscriber.SubscriptionPlan.ToString() ),
                new( "subplanname", e.Subscriber.SubscriptionPlanName )
                })));
            }

            Stats.AddSub();
            Stats.AddAutoEvents();
        }

        private void Client_OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.Resubscribe))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Resubscribe);
                msg ??= "Thanks #user for re-subscribing!";

                Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<string, string>[] {
                new( "user", e.ReSubscriber.DisplayName ),
                new( "months", FormatData.Plurality(e.ReSubscriber.Months, "total month", "total months") ),
                new( "submonths", FormatData.Plurality(e.ReSubscriber.MsgParamCumulativeMonths, "month total", "months total")),
                new( "subplan", e.ReSubscriber.SubscriptionPlan.ToString()),
                new( "subplanname", e.ReSubscriber.SubscriptionPlanName )
                });

                // add the streak element if user wants their sub streak displayed
                if (e.ReSubscriber.MsgParamShouldShareStreak)
                {
                    VariableParser.AddData(ref dictionary, new Tuple<string, string>[] { new("streak", e.ReSubscriber.MsgParamStreakMonths) });
                }

                Send(VariableParser.ParseReplace(msg, dictionary));
            }

            Stats.AddSub();
            Stats.AddAutoEvents();
        }

        private void Client_OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.GiftSub))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.GiftSub);
                msg ??= "Thanks #user for gifting a #subplan subscription to #receiveuser!";


                Send(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<string, string>[] {
                    new("user", e.GiftedSubscription.DisplayName),
                    new("months", FormatData.Plurality(e.GiftedSubscription.MsgParamMonths, "month", "months")),
                    new("receiveuser", e.GiftedSubscription.MsgParamRecipientUserName ),
                    new("subplan", e.GiftedSubscription.MsgParamSubPlan.ToString() ),
                    new("subplanname", e.GiftedSubscription.MsgParamSubPlanName)
                })));
            }

            Stats.AddGiftSubs();
            Stats.AddAutoEvents();
        }

        private void Client_OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
        {
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.CommunitySubs))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.CommunitySubs);
                msg ??= "Thanks #user for giving #count to the community!";

                Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<string, string>[] {
                    new("user", e.GiftedSubscription.DisplayName),
                    new("count", FormatData.Plurality(e.GiftedSubscription.MsgParamSenderCount, e.GiftedSubscription.MsgParamSubPlan + " subscription", e.GiftedSubscription.MsgParamSubPlan + " subscriptions")),
                    new("subplan", e.GiftedSubscription.MsgParamSubPlan.ToString())
                });

                Send(VariableParser.ParseReplace(msg, dictionary));
            }

            Stats.AddGiftSubs(e.GiftedSubscription.MsgParamMassGiftCount);
            Stats.AddAutoEvents();
        }

        #endregion Subscriptions

        #region Hosting
        private void Client_OnBeingHosted(object sender, OnBeingHostedArgs e)
        {
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.BeingHosted))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.BeingHosted);
                msg ??= "Thanks #user for #autohost this channel!";


                Send(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<string, string>[]
                {
                    new("user", e.BeingHostedNotification.HostedByChannel ),
                    new("autohost", e.BeingHostedNotification.IsAutoHosted ? "auto-hosting" : "hosting"),
                    new("viewers", FormatData.Plurality(e.BeingHostedNotification.Viewers, "viewer", "viewers"))
                })));
            }

            Stats.AddHosted();
            Stats.AddAutoEvents();
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

        private void Client_OnNowHosting(object sender, OnNowHostingArgs e)
        {
        }
        #endregion Hosting

        #region Raid events
        private void Client_OnRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            if ((bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.Raid))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Raid);
                msg ??= "Thanks #user for bringing #viewers and raiding the channel!";

                Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<string, string>[] {
                new("user", e.RaidNotification.DisplayName ),
                new("viewers", FormatData.Plurality(e.RaidNotification.MsgParamViewerCount, "viewer", "viewers"))
                });

                Send(VariableParser.ParseReplace(msg, dictionary));
            }
            Stats.AddRaids();
            Stats.AddAutoEvents();

            if (OptionFlags.TwitchRaidFollowBack)
            {
                TwitchFollower.FollowBack(e.RaidNotification.DisplayName);
            }

            if (OptionFlags.TwitchRaidShoutOut)
            {
                Stats.UserJoined(e.RaidNotification.DisplayName, DateTime.Now);
                bool output = ProcessCommands.CheckShout(e.RaidNotification.DisplayName, out string response, false);
                if (output)
                {
                    Send(response);
                }
            }
        }

        private void Client_OnSelfRaidError(object sender, EventArgs e)
        {
        }

        private void Client_OnRaidedChannelIsMatureAudience(object sender, EventArgs e)
        {
        }

        #endregion Raid events

        #region Msg IO
        private void Client_OnWhisperThrottled(object sender, OnWhisperThrottledEventArgs e)
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

        private void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            Stats.AddCommands();
            AddChat(e.Command.ChatMessage.DisplayName);

            try
            {
                string response = ProcessCommands.ParseCommand(e.Command.CommandText, e.Command.ArgumentsAsList, e.Command.ChatMessage);
                if (response != "")
                {
                    Send(response);
                }
            }
            catch (InvalidOperationException InvalidOp)
            {
                Send(InvalidOp.Message);
            }
            catch (NullReferenceException NullRef)
            {
                Send(NullRef.Message);
            }
            catch { }

        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
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
            if (e.ChatMessage.Bits > 0 && (bool)DataManage.GetRowData(DataRetrieve.EventEnabled, ChannelEventActions.Bits))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Bits);
                msg ??= "Thanks #user for giving #bits!";

                Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<string, string>[]
                {
                    new("user", e.ChatMessage.DisplayName),
                    new( "bits", FormatData.Plurality(e.ChatMessage.Bits, "bit", "bits") )
                });

                Send(VariableParser.ParseReplace(msg, dictionary));
                Stats.AddBits(e.ChatMessage.Bits);
                Stats.AddAutoEvents();
            }

            AddChat(e.ChatMessage.DisplayName);
        }

        private void Client_OnMessageThrottled(object sender, OnMessageThrottledEventArgs e)
        {
        }

        private void Client_OnMessageSent(object sender, OnMessageSentArgs e)
        {
        }

        private void Client_OnMessageCleared(object sender, OnMessageClearedArgs e)
        {
        }

        private void Client_OnSendReceiveData(object sender, OnSendReceiveDataArgs e)
        {
        }

        private void Client_OnRitualNewChatter(object sender, OnRitualNewChatterArgs e)
        {
            AddChat(e.RitualNewChatter.DisplayName);
        }

        #region Chat changes
        private void Client_OnChatColorChanged(object sender, OnChatColorChangedArgs e)
        {
        }

        private void Client_OnChatCleared(object sender, OnChatClearedArgs e)
        {
        }

        #endregion

        #endregion Msg IO

        #region User state changes

        #region badge changes
        private void Client_OnVIPsReceived(object sender, OnVIPsReceivedArgs e)
        {
        }

        private void Client_OnModeratorsReceived(object sender, OnModeratorsReceivedArgs e)
        {
        }

        #endregion

        #region Moderators

        private void Client_OnModeratorJoined(object sender, OnModeratorJoinedArgs e)
        {
            Stats.ModJoined(e.Username);
        }

        private void Client_OnModeratorLeft(object sender, OnModeratorLeftArgs e)
        {
        }
        #endregion

        private void Client_OnUserJoined(object sender, OnUserJoinedArgs e)
        {
            if (Stats.UserJoined(e.Username, DateTime.Now) && OptionFlags.FirstUserJoinedMsg)
            {
                RegisterJoinedUser(e.Username);
            }
        }

        private void AddChat(string Username)
        {
            if (Stats.UserChat(Username) && OptionFlags.FirstUserChatMsg)
            {
                RegisterJoinedUser(Username);
            }
        }

        private void RegisterJoinedUser(string Username)
        {
            // TODO: update welcome message for new user, returning viewer, and returning follower!
            if (((Username != TwitchBots.TwitchChannelName) || (Username == TwitchBots.TwitchChannelName && OptionFlags.MsgWelcomeStreamer)) && (OptionFlags.FirstUserJoinedMsg || OptionFlags.FirstUserChatMsg))
            {
                string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.UserJoined);
                msg ??= "Thanks #user for stopping by the channel!";

                Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<string, string>[]
                {
                    new( "user", Username )
                });

                Send(VariableParser.ParseReplace(msg, dictionary));
            }

            if (OptionFlags.AutoShout)
            {
                bool output = ProcessCommands.CheckShout(Username, out string response);
                if (output)
                {
                    Send(response);
                }
            }
        }

        private void Client_OnUserLeft(object sender, OnUserLeftArgs e) => Stats.UserLeft(e.Username, DateTime.Now);

        private void Client_OnUserBanned(object sender, OnUserBannedArgs e) => Stats.AddUserBanned();

        private void Client_OnUserStateChanged(object sender, OnUserStateChangedArgs e)
        {
        }

        private void Client_OnUserTimedout(object sender, OnUserTimedoutArgs e) => Stats.AddUserTimedOut();

        private void Client_OnExistingUsersDetected(object sender, OnExistingUsersDetectedArgs e)
        {
            foreach (var user in from string user in e.Users
                                 where Stats.UserJoined(user, DateTime.Now)
                                 select user)
            {
                RegisterJoinedUser(user);
            }
        }

        #endregion

        #region Connection and Channel Info

        #region Channel Join Leave
        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            if (IOModule.ShowConnectionMsg)
            {
                Version version = Assembly.GetEntryAssembly().GetName().Version;
                string s = "Twine Chatbot by WrithemTwine, version " + string.Format(CultureInfo.CurrentCulture, "{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision) + ", is now connected!";

                Send(s);
            }
        }

        private void Client_OnLeftChannel(object sender, OnLeftChannelArgs e)
        {
        }

        #endregion

        private void Client_OnChannelStateChanged(object sender, OnChannelStateChangedArgs e)
        {
        }

        #region Error checking
        private void Client_OnNoPermissionError(object sender, EventArgs e)
        {
        }

        private void Client_OnError(object sender, OnErrorEventArgs e)
        {
        }

        private void Client_OnIncorrectLogin(object sender, OnIncorrectLoginArgs e)
        {
        }

        private void Client_OnFailureToReceiveJoinConfirmation(object sender, OnFailureToReceiveJoinConfirmationArgs e)
        {
        }

        private void Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
        }

        private void Client_OnUnaccountedFor(object sender, OnUnaccountedForArgs e)
        {
        }

        #endregion

        #endregion

    }
}
