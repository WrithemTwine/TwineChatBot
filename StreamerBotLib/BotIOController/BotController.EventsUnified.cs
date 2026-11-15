using StreamerBotLib.BotClients;
using StreamerBotLib.Models.Data;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Models.Users;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;
using StreamerBotLib.Systems.Overlay.Enums;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StreamerBotLib.BotIOController
{
    public partial class BotController
    {
        #region Handle Bot Events

        #region Followers

        /// <summary>
        /// When new followers are detected from Twitch, the Twitch bot captures the data about the new followers and sends it to the system to handle the incoming followers data, which may include updating the followers list in the system, posting messages to the channel about the new followers, and posting notifications to any webhooks about the new followers. This method is specifically for handling new followers detected from Twitch, and is called with the follower data such as the follower's username, user Id, and follow time.
        /// </summary>
        /// <param name="follow">The new follower data.</param>
        public void HandleBotEventNewFollowers(Follow follow)
        {
            DataBot.AddNewFollowers([follow]);
        }

        /// <summary>
        /// When a bulk followers update operation starts, this method is called to indicate the start of the bulk update, which can be used to trigger any processes that need to wait for the bulk update to start before performing certain operations, such as preparing to receive the bulk followers data or resetting any temporary data structures used for processing the bulk update. This method is specifically for handling the start of a bulk followers update operation, and is called before the system starts receiving the bulk followers data from Twitch.
        /// </summary>
        public void HandleBotEventStartBulkFollowers()
        {
            OnBulkFollowerStarted?.Invoke(this, new());
            DataBot.StartBulkFollowers();
        }

        /// <summary>
        /// When a bulk followers update operation posts a list of followers, this method is called to handle the incoming bulk followers data, which may include updating the followers list in the system with the new bulk data, posting messages to the channel about any changes in followers based on the new bulk data, and posting notifications to any webhooks about the changes in followers. This method is specifically for handling the incoming bulk followers data from Twitch, and is called with a list of follower data such as the follower's username, user Id, and follow time for each follower in the bulk update. The system can then compare this new bulk followers list to the old followers list to find new followers and lost followers, and trigger any events or notifications based on those changes in the followers list.
        /// </summary>
        /// <param name="follows">The list of followers to update.</param>
        public void HandleBotEventBulkPostFollowers(List<Follow> follows)
        {
            DataBot.UpdateFollowers(follows);
        }

        /// <summary>
        /// When a bulk followers update operation stops, this method is called to indicate the end of the bulk update, which can be used to trigger any processes that need to wait for the bulk update to stop before performing certain operations, such as finalizing the processing of the bulk followers data or cleaning up any temporary data structures used for processing the bulk update. This method is specifically for handling the end of a bulk followers update operation, and is called after the system has finished receiving and processing the bulk followers data from Twitch.
        /// </summary>
        public void HandleBotEventStopBulkFollowers()
        {
            DataBot.StopBulkFollowers();
        }

        #endregion

        #region Clips

        /// <summary>
        /// When new clips are detected from Twitch, the Twitch bot captures the data about the new clips and sends it to the system to handle the incoming clips data, which may include updating the clips list in the system, posting messages to the channel about the new clips, and posting notifications to any webhooks about the new clips. This method is specifically for handling new clips detected from Twitch, and is called with the clip data such as the clip's title, URL, creator username, and creation time.
        /// </summary>
        /// <param name="AllClips">Whether the current call is for all clips.</param>
        /// <param name="clips">The list of clips to update.</param>
        public void HandleBotEventPostNewClip(bool AllClips, List<Clip> clips)
        {
            DataBot.ClipHelper(AllClips, clips);
        }

        /// <summary>
        /// When a request is made to get the clips for a Twitch channel, this method is called to handle the request for getting the channel clips, which may include retrieving the clips data from Twitch for the specified channel, and then posting the retrieved clips data back to the system through the provided callback function. This method is specifically for handling requests to get Twitch channel clips, and is called with the channel name and a callback function that should be called with the list of clips once they are retrieved from Twitch. The system can then use this clips data to update any relevant information in the system, post messages about the clips in the channel, or send notifications to any webhooks about the new clips.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments containing the channel name and callback function.</param>
        public void HandleGetUserClips(object sender, GetChannelClipsEventArgs e)
        {
            if (e.Platform == Platform.Twitch)
            {
                TwitchBots.GetUserChannelClips(e.ChannelName, e.CallBackResult);
            }
        }

        #endregion

        #region LiveStream

        private void HandleMultiLiveOnStreamOnline(LiveUser User, string Title, DateTime StartedAt, string Category)
        {
            DateTime CurrTime = StartedAt.ToLocalTime();

            // true posted new event, false did not post

            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                DataBot.PostMultiStreamDate(User, CurrTime, (PostedLive) => PostMultiLiveStreamDateCallBack(User, Title, Category, CurrTime, PostedLive));

                if (User.Platform == Platform.Twitch)
                {
                    if (OptionFlags.TwitchMultiLiveBrowseChannel)
                    {
                        string URL = Resources.TwitchHomepage + User.UserName;

                        Process startBrowser = new();
                        startBrowser.StartInfo.UseShellExecute = true;
                        startBrowser.StartInfo.FileName = $"\"{URL}\"";
                        _ = startBrowser.Start();
                    }
                }
            });
        }

        private static void PostMultiLiveStreamDateCallBack(LiveUser User, string Title, string Category, DateTime CurrTime, bool PostedLive)
        {
            if (PostedLive)
            {
                DataBot.CheckMultiLiveStreamDate(User.UserId, User.Platform, CurrTime, (MultiLive) => MultiStreamDateCallback(User, Title, Category, MultiLive));
            }
        }

        private static void MultiStreamDateCallback(LiveUser User, string Title, string Category, bool MultiLive)
        {
            if ((OptionFlags.PostMultiLive && MultiLive) || !MultiLive)
            {
                // get message, set a default if otherwise deleted/unavailable
                string msg = OptionFlags.MsgLive ?? "@everyone, #user is now live streaming #category - #title! Come join and say hi at: #url";

                // keys for exchanging codes for representative names
                Dictionary<string, string> dictionary = new()
                        {
                            { "#user", User.UserName },
                            { "#category", Category },
                            { "#title", Title },
                            { "#url", User.UserName }
                        };

                DataBot.PostMultiLiveLog(VariableParser.ParseReplace(msg, dictionary));
                DataBot.GetMultiWebHooks((Webhooks) => PostMultiWebHooksCallback(User, msg, dictionary, Webhooks));
            }
        }

        private static void PostMultiWebHooksCallback(LiveUser User, string msg, Dictionary<string, string> dictionary, IEnumerable<Tuple<WebhooksSource, Uri>> Webhooks)
        {
            foreach (Tuple<WebhooksSource, Uri> u in Webhooks)
            {
                if (u.Item1 == WebhooksSource.Discord)
                {
                    DiscordWebhook.SendMessage(u.Item2,
                        VariableParser.ParseReplace(msg, dictionary),
                        VariableParser.BuildPlatformUrl(User.UserName, User.Platform));
                }
            }
        }

        /// <summary>
        /// When a Twitch stream goes live, the Twitch bot captures the data about the stream and sends it to the system to handle the incoming stream data, which may include posting messages to the channel about the stream, and posting notifications to any webhooks about the stream. This method is specifically for handling multi-channel streaming, which means this method is called when any monitored channel goes live, and the system checks if it's a multi-channel stream and posts messages accordingly.
        /// </summary>
        /// <param name="e">The multi-channel stream summarize event data</param>
        public void MultiChannelSummarize(MultiLiveSummarizeEventArgs e)
        {
            DataBot.MultiSummarize(e);
        }

        private void ManageOnlineStream(Platform platform)
        {
            PlatformOnlineStatus[platform] = true;

            OnStreamOnline?.Invoke(this, new());
        }

        private void ManageOfflineStream(Platform platform)
        {
            PlatformOnlineStatus[platform] = false;

            if (!PlatformOnlineStatus.ContainsValue(true))
            {
                OnStreamOffline?.Invoke(this, new());
            }
        }

        /// <summary>
        /// When a stream goes online, this method is called to handle the stream online event, which includes managing the stream online status for the platform, managing the bots' stream status based on the new stream online status, posting messages to the channel about the stream going live, and posting notifications to any webhooks about the stream going live. This method is specifically for handling a single channel stream going online, and is called with the stream data such as channel name, title, start time, and category.
        /// </summary>
        /// <param name="ChannelName">The name of the channel going live</param>
        /// <param name="Title">The title of the stream</param>
        /// <param name="StartedAt">The start time of the stream</param>
        /// <param name="Category">The category of the stream</param>
        /// <param name="platform">The platform of the stream</param>
        /// <param name="Debug">A flag indicating whether to run in debug mode</param>
        public void HandleOnStreamOnline(string ChannelName, string Title, DateTime StartedAt, CategoryData Category, Platform platform = Platform.Twitch, bool Debug = false)
        {
            try
            {
                ManageOnlineStream(platform);

                ManageBotsStreamStatusChanged(true);

                OnStreamCategoryChanged?.Invoke(this, new() { GameId = Category.CategoryId, GameName = Category.CategoryName });

                DataBot.StreamOnline(StartedAt, Category, (isOnline) => CallbackHandleOnStreamOnline(isOnline, ChannelName, Title, StartedAt, Category, platform, Debug)); // a callback to finish the stream online process
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "HandleOnStreamOnline");
            }
        }

        private void CallbackHandleOnStreamOnline(bool Started, string ChannelName, string Title, DateTime StartedAt, CategoryData Category, Platform platform = Platform.Twitch, bool Debug = false)
        {
            try
            {
                if (Started)
                {
                    bool MultiLive = ActionSystem.CheckStreamDate(StartedAt); // since this call is within another callback through DataBot, we don't need to use DataBot

                    if ((OptionFlags.PostMultiLive && MultiLive) || !MultiLive)
                    {
                        // get message, set a default if otherwise deleted/unavailable
                        string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out bool Enabled, out _);

                        // keys for exchanging codes for representative names
                        Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                        {
                                new(MsgVars.user, ChannelName),
                                new(MsgVars.category, Category.CategoryName),
                                new(MsgVars.title, Title),
                                new(MsgVars.url, ChannelName)
                        });

                        string TempMsg = VariableParser.ParseReplace(msg, dictionary);

                        if (Enabled && !Debug)
                        {
                            DataBot.GetDiscordWebhooks(WebhooksKind.Live, (Webhooks) => PostToDiscord(ChannelName, platform, TempMsg, Webhooks));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "HandleOnStreamOnline");
            }
        }

        private void PostToDiscord(string ChannelName, Platform platform, string TempMsg, IEnumerable<Tuple<bool, Uri>> Webhooks)
        {
            foreach (Tuple<bool, Uri> u in Webhooks)
            {
                DiscordWebhook.SendMessage(u.Item2, VariableParser.ParseReplace(TempMsg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                                                {
                                                                        new(MsgVars.everyone, u.Item1 ? "@everyone" : "")
                                                }
                                            )
                                        ),
                                        VariableParser.BuildPlatformUrl(ChannelName, platform)
                                    );
                DataBot.UpdatedStat(StreamStatType.Discord);
            }
        }

        /// <summary>
        /// When a stream category is found for the viewer, this method is called to handle the found viewer category event, which includes posting messages to the channel about the viewer's category, and posting notifications to any webhooks about the viewer's category. This method is specifically for handling when a viewer's category is found, which may be used for features such as showing the viewer's current category in chat or on stream.
        /// </summary>
        /// <param name="categoryData">Describes the current category data for the viewer.</param>
        public void HandleFoundViewerCategory(CategoryData categoryData)
        {
            DataBot.PostViewerCategory(categoryData);
        }

        /// <summary>
        /// When a stream category is updated, this method is called to handle the stream update event, which includes updating the current category data in the system, posting messages to the channel about the stream's new category, and posting notifications to any webhooks about the stream's new category. This method is specifically for handling when a stream's category is updated while the stream is live, which may be used for features such as showing the stream's current category in chat or on stream, and notifying viewers of the category change.
        /// </summary>
        /// <param name="categoryData">Describes the updated category data for the stream.</param>
        public void HandleOnStreamUpdate(CategoryData categoryData)
        {
            DataBot.SetCategory(categoryData);

            if (OptionFlags.IsStreamOnline)
            {
                DataBot.PostCategoryStream(categoryData);
            }

            OnStreamCategoryChanged?.Invoke(this, new() { GameId = categoryData.CategoryId, GameName = categoryData.CategoryName });
        }

        /// <summary>
        /// When a stream goes offline, this method is called to handle the stream offline event, which includes managing the stream offline status for the platform, managing the bots' stream status based on the new stream offline status, posting messages to the channel about the stream going offline, and posting notifications to any webhooks about the stream going offline. This method is specifically for handling a single channel stream going offline, and is called with the stream data such as hosted channel (if a raid occurred) and raid time (if a raid occurred). The method also checks if the stream was previously online before posting any messages or notifications about the stream going offline.
        /// </summary>
        /// <param name="platform">The platform for which the stream is going offline.</param>
        /// <param name="HostedChannel">The channel that hosted the stream (if a raid occurred).</param>
        /// <param name="RaidTime">The time when the raid occurred (if a raid occurred).</param>
        public void HandleOnStreamOffline(Platform platform, string HostedChannel = null, DateTime? RaidTime = null)
        {
            LogWriter.DebugLog("HandleOnStreamOffline", DebugLogTypes.BotController, "Received a livestream offline status update.");

            ManageOfflineStream(platform);

            if (OptionFlags.IsStreamOnline)
            {
                LogWriter.DebugLog("HandleOnStreamOffline", DebugLogTypes.BotController, "Start notifying about the offline stream.");

                ManageBotsStreamStatusChanged(false);

                DateTime currTime = RaidTime?.ToLocalTime() ?? DateTime.Now.ToLocalTime();
                DataBot.StreamOffline(currTime);

                if (RaidTime != null)
                {
                    LogWriter.DebugLog("HandleOnStreamOffline", DebugLogTypes.BotController, $"A raid occurred to channel: {HostedChannel}.");
                }
                else
                {
                    LogWriter.DebugLog("HandleOnStreamOffline", DebugLogTypes.BotController, $"No raid occurred, stream offline.");
                }

                DataBot.PostOutgoingRaid(HostedChannel ?? "No Raid", currTime);
                //OptionFlags.TwitchOutRaidStarted = false;
            }
        }

        /// <summary>
        /// Call to manage other bots when a monitored stream is detected to be online.
        /// </summary>
        /// <param name="Start">True to start services for stream online, False to stop services for stream offline.</param>
        public static void ManageBotsStreamStatusChanged(bool Start)
        {
            LogWriter.DebugLog("ManageBotsStreamStatusChanged", DebugLogTypes.BotController, $"Starting any stopped bots or " +
                $"stopping any started bots, based on the current active livestream={Start} status.");

            // loop the bots and send message to start or stop based on stream online or offline status
            foreach (IBotTypes bots in BotsList)
            {
                bots.ManageStreamOnlineOfflineStatus(Start);
            }
        }

        #endregion

        #region Chat Bot

        /// <summary>
        /// When a chat bot starts, this method is called to handle the chat bot started event, which includes adding the started chat bot to the list of started chat bots, starting the message sending thread if it's not already started, posting a welcome message to the channel if the option is enabled, and notifying the data bot about the bot start. This method is specifically for handling when a chat bot starts, and may be called multiple times if multiple chat bots are used in the system. The method also checks if it's the first chat bot starting to manage the message sending thread accordingly.
        /// </summary>
        /// <param name="Source">The chat bot that has started.</param>
        /// <param name="args">Event arguments for the chat bot started event.</param>
        public void HandleChatBotStarted(Bots Source, EventArgs args)
        {
            lock (StartedChatBots)
            {
                StartedChatBots.UniqueAdd(Source);
            }

            if (StartedChatBots.Count == 1 && args == null)
            {
                SendThread = ThreadManager.CreateThread(
                                                        "HandleChatBotStarted",
                                                        BeginProcMsgs,
                                                        Priority: ThreadExitPriority.Normal);
                SendThread.Start();

                if (OptionFlags.MsgBotConnection)
                { // only show if user spcified they want the welcome message sent to chat
                    Send(Platform.Default, LocalizedMsgSystem.GetTwineBotAuthorInfo());
                }
                DataBot.NotifyBotStart();
            }
        }

        /// <summary>
        /// When a chat bot is stopping, this method is called to handle the chat bot stopping event, which includes removing the stopping chat bot from the list of started chat bots, stopping the message sending thread if it's no longer needed, and notifying the data bot about the bot stop. This method is specifically for handling when a chat bot is stopping, and may be called multiple times if multiple chat bots are used in the system.
        /// </summary>
        /// <param name="Source">The chat bot that is stopping.</param>
        /// <param name="args">Event arguments for the chat bot stopping event.</param>
        public void HandleChatBotStopping(Bots Source, EventArgs args)
        {
            lock (StartedChatBots)
            {
                StartedChatBots.RemoveAll((s) => s == Source);
            }

            if (StartedChatBots.Count == 0 && args == null)
            {
                DataBot.NotifyBotStop();
            }
            ChatBotStopping = true;
        }

        /// <summary>
        /// When a chat bot has stopped, this method is called to handle the chat bot stopped event, which includes checking if the stopped chat bot was the last one in the list of started chat bots, and if so, stopping the message sending thread and resetting the chat bot stopping status. This method is specifically for handling when a chat bot has fully stopped, and may be called multiple times if multiple chat bots are used in the system. The method also checks if it's the last chat bot stopping to manage the message sending thread accordingly.
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="args"></param>
        public void HandleChatBotStopped(Bots Source, EventArgs args)
        {
            if (Source == Bots.TwitchEventSubBot && args == null)
            {
                ChatBotStopping = false;
            }
        }

        /// <summary>
        /// When a new subscriber event is received, this method is called to handle the new subscriber event, which includes parsing the subscriber data, building the message to be sent to the channel and webhooks, posting messages to the channel about the new subscriber, and posting notifications to any webhooks about the new subscriber. This method is specifically for handling when a new subscriber is detected by the Twitch bot, and may be called multiple times if multiple subscribers are detected. The method also updates the subscription statistics in the data bot and checks for any overlay events related to the new subscriber.
        /// </summary>
        /// <param name="User">The user who subscribed.</param>
        /// <param name="Months">The number of months the user has subscribed.</param>
        /// <param name="Subscription">The type of subscription.</param>
        /// <param name="SubscriptionName">The name of the subscription.</param>
        public void HandleNewSubscriber(LiveUser User, string Months, string Subscription, string SubscriptionName)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out bool Enabled, out short Multi);

            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                new( MsgVars.user, User.UserName ),
                new( MsgVars.submonths, FormatData.Plurality(Months, MsgVars.Pluralmonth, Prefix: LocalizedMsgSystem.GetVar(MsgVars.Total)) ),
                new( MsgVars.subplan, Subscription ),
                new( MsgVars.subplanname, SubscriptionName )
                });
            string ParsedMsg = VariableParser.ParseReplace(msg, dictionary);
            string HTMLParsedMsg = VariableParser.ParseReplace(msg, dictionary, true);

            if (Enabled)
            {
                DataBot.GetEventAnnounce(ChannelEventActions.Subscribe, (result) => Send(User.Platform, ParsedMsg, result, Multi));
            }

            DataBot.UpdatedStat(StreamStatType.Sub, StreamStatType.AutoEvents);

            DataBot.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Subscribe, User, UserMsg: HTMLParsedMsg);
            DataBot.AddNewOverlayTickerItem(OverlayTickerItem.LastSubscriber, User.UserName);
        }

        /// <summary>
        /// When a re-subscriber event is received, this method is called to handle the re-subscriber event, which includes parsing the re-subscriber data, building the message to be sent to the channel and webhooks, posting messages to the channel about the re-subscriber, and posting notifications to any webhooks about the re-subscriber. This method is specifically for handling when a re-subscriber is detected by the Twitch bot, and may be called multiple times if multiple re-subscribers are detected. The method also updates the subscription statistics in the data bot and checks for any overlay events related to the re-subscriber. Additionally, if the user has chosen to share their subscription streak, it will include that information in the message.
        /// </summary>
        /// <param name="User">The user who re-subscribed.</param>
        /// <param name="Months">The number of months the user has re-subscribed.</param>
        /// <param name="TotalMonths">The total number of months the user has subscribed.</param>
        /// <param name="Subscription">The type of subscription.</param>
        /// <param name="SubscriptionName">The name of the subscription.</param>
        /// <param name="ShareStreak">Indicates whether the user wants to share their subscription streak.</param>
        /// <param name="StreakMonths">The number of months in the user's subscription streak.</param>
        public void HandleReSubscriber(LiveUser User, int Months, string TotalMonths, string Subscription, string SubscriptionName, bool ShareStreak, string StreakMonths)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out bool Enabled, out short Multi);
            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                new( MsgVars.user, User.UserName ),
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

            string ParsedMsg = VariableParser.ParseReplace(msg, dictionary);
            string HTMLParsedMsg = VariableParser.ParseReplace(msg, dictionary, true);
            if (Enabled)
            {
                DataBot.GetEventAnnounce(ChannelEventActions.Resubscribe, (result) => Send(User.Platform, ParsedMsg, result, Multi));
            }
            DataBot.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Resubscribe, User, UserMsg: HTMLParsedMsg);

            DataBot.UpdatedStat(StreamStatType.Sub, StreamStatType.AutoEvents);
            DataBot.AddNewOverlayTickerItem(OverlayTickerItem.LastSubscriber, User.UserName);
        }

        /// <summary>
        /// When a gift subscription event is received, this method is called to handle the gift subscription event, which includes parsing the gift subscription data, building the message to be sent to the channel and webhooks, posting messages to the channel about the gift subscription, and posting notifications to any webhooks about the gift subscription. This method is specifically for handling when a gift subscription is detected by the Twitch bot, and may be called multiple times if multiple gift subscriptions are detected. The method also updates the gift subscription statistics in the data bot and checks for any overlay events related to the gift subscription.
        /// </summary>
        /// <param name="User">The user who gifted the subscription.</param>
        /// <param name="Months">The number of months the gift subscription covers.</param>
        /// <param name="RecipientUserName">The username of the recipient of the gift subscription.</param>
        /// <param name="Subscription">The type of subscription.</param>
        /// <param name="SubscriptionName">The name of the subscription.</param>
        public void HandleGiftSubscription(LiveUser User, string Months, string RecipientUserName, string Subscription, string SubscriptionName)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.GiftSub, out bool Enabled, out short Multi);
            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                    new(MsgVars.user, User.UserName ?? "anonymous"),
                    new(MsgVars.months, FormatData.Plurality(Months, MsgVars.Pluralmonth)),
                    new(MsgVars.receiveuser, RecipientUserName ?? "" ),
                    new(MsgVars.subplan, Subscription ?? "" ),
                    new(MsgVars.subplanname, SubscriptionName ?? "")
                });

            string ParsedMsg = VariableParser.ParseReplace(msg, dictionary);
            string HTMLParsedMsg = VariableParser.ParseReplace(msg, dictionary, true);
            if (Enabled)
            {
                DataBot.GetEventAnnounce(ChannelEventActions.GiftSub, (result) => Send(User.Platform, ParsedMsg, result, Multi));
            }
            DataBot.UpdatedStat(StreamStatType.GiftSubs, StreamStatType.AutoEvents);
            DataBot.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.GiftSub, User, UserMsg: HTMLParsedMsg);
            DataBot.AddNewOverlayTickerItem(OverlayTickerItem.LastGiftSub, User.UserName);
            //            SystemsController.AddNewOverlayTickerItem(OverlayTickerItem.LastSubscriber, RecipientUserName);
        }

        /// <summary>
        /// When a community subscription event is received, this method is called to handle the community subscription event, which includes parsing the community subscription data, building the message to be sent to the channel and webhooks, posting messages to the channel about the community subscription, and posting notifications to any webhooks about the community subscription. This method is specifically for handling when a community subscription is detected by the Twitch bot, and may be called multiple times if multiple community subscriptions are detected. The method also updates the gift subscription statistics in the data bot and checks for any overlay events related to the community subscription.
        /// </summary>
        /// <param name="User">The user who subscribed.</param>
        /// <param name="SubCount">The number of months the community subscription covers.</param>
        /// <param name="Subscription">The type of subscription.</param>
        public void HandleCommunitySubscription(LiveUser User, int SubCount, string Subscription)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.CommunitySubs, out bool Enabled, out short Multi);
            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                    new(MsgVars.user, User.UserName ?? "anonymous"),
                    new(MsgVars.count, FormatData.Plurality(SubCount, MsgVars.Pluralsub, Subscription)),
                    new(MsgVars.subplan, Subscription)
                });

            string ParsedMsg = VariableParser.ParseReplace(msg, dictionary);
            string HTMLParsedMsg = VariableParser.ParseReplace(msg, dictionary, true);
            if (Enabled)
            {
                DataBot.GetEventAnnounce(ChannelEventActions.CommunitySubs, (result) => Send(User.Platform, ParsedMsg, result, Multi));
            }

            DataBot.UpdatedStat(StreamStatType.GiftSubs, SubCount);
            DataBot.UpdatedStat(StreamStatType.AutoEvents);

            DataBot.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.CommunitySubs, User, UserMsg: HTMLParsedMsg);
            DataBot.AddNewOverlayTickerItem(OverlayTickerItem.LastGiftSub, User.UserName ?? "anonymous");
        }

        /// <summary>
        /// When a channel cheer event is received, this method is called to handle the channel cheer event, which includes parsing the cheer data, building the message to be sent to the channel and webhooks, posting messages to the channel about the cheer, and posting notifications to any webhooks about the cheer. This method is specifically for handling when a channel cheer is detected by the Twitch bot, and may be called multiple times if multiple cheers are detected. The method also updates the cheer statistics in the data bot and checks for any overlay events related to the cheer.
        /// </summary>
        /// <param name="user">The user who cheered.</param>
        /// <param name="Bits">The number of bits cheered.</param>
        public void HandleChannelCheer(LiveUser user, int Bits)
        {
            DataBot.UserCheered(user, Bits);
        }

        /// <summary>
        /// When a channel is hosted, this method is called to handle the being hosted event, which includes parsing the hosting data, building the message to be sent to the channel and webhooks, posting messages to the channel about being hosted, and posting notifications to any webhooks about being hosted. This method is specifically for handling when a channel is hosted by another channel, and may be called multiple times if multiple hosting events are detected. The method also updates the hosting statistics in the data bot and checks for any overlay events related to being hosted.
        /// </summary>
        /// <param name="User">The user who is being hosted.</param>
        /// <param name="HostedByChannel">The channel that is hosting.</param>
        /// <param name="IsAutoHosted">Indicates if the hosting is auto-hosted.</param>
        /// <param name="Viewers">The number of viewers watching the host channel.</param>
        public void HandleBeingHosted(LiveUser User, string HostedByChannel, bool IsAutoHosted, int Viewers)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BeingHosted, out bool Enabled, out short Multi);
            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                                            {
                    new(MsgVars.user, HostedByChannel ),
                    new(MsgVars.autohost, LocalizedMsgSystem.DetermineHost(IsAutoHosted) ),
                    new(MsgVars.viewers, FormatData.Plurality(Viewers, MsgVars.Pluralviewers
                     ))
                                            });
            string ParsedMsg = VariableParser.ParseReplace(msg, dictionary);
            string HTMLParsedMsg = VariableParser.ParseReplace(msg, dictionary, true);
            if (Enabled)
            {
                DataBot.GetEventAnnounce(ChannelEventActions.BeingHosted, (result) => Send(User.Platform, ParsedMsg, result, Multi));
            }

            DataBot.UpdatedStat(StreamStatType.Hosted, StreamStatType.AutoEvents);
            DataBot.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.BeingHosted, User, UserMsg: HTMLParsedMsg);
        }

        /// <summary>
        /// When a user joins the channel, this method is called to handle the user joined event, which includes parsing the user data, building the message to be sent to the channel and webhooks, posting messages to the channel about the user joining, and posting notifications to any webhooks about the user joining. This method is specifically for handling when a user joins the channel, and may be called multiple times if multiple users join. The method also updates the viewer statistics in the data bot and checks for any overlay events related to the user joining.
        /// </summary>
        /// <param name="Users">The list of users who joined.</param>
        public void HandleUserJoined(List<LiveUser> Users)
        {
            DataBot.UserJoined(Users);
        }

        /// <summary>
        /// When a user leaves the channel, this method is called to handle the user left event, which includes parsing the user data, building the message to be sent to the channel and webhooks, posting messages to the channel about the user leaving, and posting notifications to any webhooks about the user leaving. This method is specifically for handling when a user leaves the channel, and may be called multiple times if multiple users leave. The method also updates the viewer statistics in the data bot and checks for any overlay events related to the user leaving.
        /// </summary>
        /// <param name="User">The user who left.</param>
        public void HandleUserLeft(LiveUser User)
        {
            DataBot.UserLeft(User);
        }

        /// <summary>
        /// When a user is banned from the channel, this method is called to handle the user banned event, which includes parsing the user data, building the message to be sent to the channel and webhooks, posting messages to the channel about the user being banned, and posting notifications to any webhooks about the user being banned. This method is specifically for handling when a user is banned from the channel, and may be called multiple times if multiple users are banned. The method also updates the ban statistics in the data bot and checks for any overlay events related to the user being banned. Additionally, it calls the HandleUserLeft method to manage any necessary actions related to the user leaving due to being banned.
        /// </summary>
        /// <param name="User">The user who was banned.</param>
        public void HandleUserBanned(LiveUser User)
        {
            try
            {
                DataBot.UpdatedStat(StreamStatType.UserBanned);
                HandleUserLeft(User);

                DataBot.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.BannedUser, User);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "HandleUserBanned");
            }
        }

        /// <summary>
        /// When a user is added to the channel (e.g. joins the chat), this method is called to handle the add chat event, which includes parsing the user data, building the message to be sent to the channel and webhooks, posting messages to the channel about the user joining, and posting notifications to any webhooks about the user joining. This method is specifically for handling when a user is added to the channel, such as when they join the chat, and may be called multiple times if multiple users are added. The method also updates the viewer statistics in the data bot and checks for any overlay events related to the user being added.
        /// </summary>
        /// <param name="UserName">The name of the user who joined.</param>
        /// <param name="Source">The platform from which the user joined.</param>
        public void HandleAddChat(LiveUser user)
        {
            DataBot.UserJoined([user]);
        }

        /// <summary>
        /// When a message is received in the chat, this method is called to handle the message received event, which includes parsing the message data, building the message to be sent to the channel and webhooks, posting messages to the channel about the received message, and posting notifications to any webhooks about the received message. This method is specifically for handling when a message is received in the chat, and may be called multiple times if multiple messages are received. The method also checks for any overlay events related to the received message.
        /// </summary>
        /// <param name="MsgReceived">The message that was received.</param>
        public void HandleMessageReceived(CmdMessage MsgReceived)
        {
            DataBot.MessageReceived(MsgReceived, MsgReceived.User);
        }

        /// <summary>
        /// When a raid occurs, this method is called to handle the incoming raid data, which includes parsing the raid data, building the message to be sent to the channel and webhooks, posting messages to the channel about the incoming raid, and posting notifications to any webhooks about the incoming raid. This method is specifically for handling when a raid is detected by the Twitch bot, and may be called multiple times if multiple raids are detected. The method also updates the raid statistics in the data bot and checks for any overlay events related to the incoming raid.
        /// </summary>
        /// <param name="User">The user who initiated the raid.</param>
        /// <param name="RaidTime">The time the raid occurred.</param>
        /// <param name="ViewerCount">The number of viewers in the raid.</param>
        /// <param name="Category">The category of the raid.</param>
        public void HandleIncomingRaidData(LiveUser User, DateTime RaidTime, int ViewerCount, CategoryData Category)
        {
            DataBot.PostIncomingRaid(User, RaidTime.ToLocalTime(), ViewerCount, Category);
        }

        /// <summary>
        /// When a raid occurs, this method is called to handle the outgoing raid data, which includes parsing the raid data, building the message to be sent to the channel and webhooks, posting messages to the channel about the outgoing raid, and posting notifications to any webhooks about the outgoing raid. This method is specifically for handling when a raid is detected by the Twitch bot as an outgoing raid (e.g. when the stream goes offline due to a raid), and may be called multiple times if multiple outgoing raids are detected. The method also updates the raid statistics in the data bot and checks for any overlay events related to the outgoing raid.  
        /// </summary>
        /// <param name="ToChannelName">The name of the channel to which the raid is outgoing.</param>
        /// <param name="RaidTime">The time the raid occurred.</param>
        /// <param name="platform">The platform from which the raid was detected.</param>
        public void HandleOutgoingRaidData(string ToChannelName, DateTime RaidTime, Platform platform)
        {
            HandleOnStreamOffline(platform, ToChannelName, RaidTime);
        }

        /// <summary>
        /// When a chat command is received, this method is called to handle the chat command received event, which includes parsing the command data, building the message to be sent to the channel and webhooks, posting messages to the channel about the received command, and posting notifications to any webhooks about the received command. This method is specifically for handling when a chat command is received in the chat, and may be called multiple times if multiple commands are received. The method also checks if the received command matches any giveaway criteria for entering a giveaway, and if so, posts the user to the giveaway. Additionally, it processes the command through the data bot to handle any commands that may be registered in the system.
        /// </summary>
        /// <param name="commandmsg">The chat command message received.</param>
        /// <param name="Source">The platform from which the command was received.</param>
        public void HandleChatCommandReceived(CmdMessage commandmsg)
        {
            if (GiveawayItemType == GiveawayTypes.Command && commandmsg.CommandText == GiveawayItemName)
            {
                HandleGiveawayPostName(commandmsg.User);
            }
            DataBot.ProcessCommand(commandmsg);
        }

        /// <summary>
        /// When a custom reward redemption is received, this method is called to handle the custom reward redemption event, which includes parsing the reward data, building the message to be sent to the channel and webhooks, posting messages to the channel about the custom reward redemption, and posting notifications to any webhooks about the custom reward redemption. This method is specifically for handling when a custom reward redemption is received from Twitch channel points, and may be called multiple times if multiple redemptions are received. The method also checks if the received custom reward redemption matches any giveaway criteria for entering a giveaway, and if so, posts the user to the giveaway. Additionally, it checks if the custom reward redemption requires approval through the data bot, and if so, posts an approval request to the channel and webhooks. Finally, it checks for any overlay events related to the custom reward redemption.
        /// </summary>
        /// <param name="User">The user who redeemed the custom reward.</param>
        /// <param name="RewardTitle">The title of the custom reward redeemed.</param>
        /// <param name="RewardMsg">The message associated with the custom reward redemption.</param>
        public void HandleCustomReward(LiveUser User, string RewardTitle, string RewardMsg)
        {
            if (GiveawayItemType == GiveawayTypes.CustomRewards && RewardTitle == GiveawayItemName)
            {
                HandleGiveawayPostName(User);
            }

            DataBot.GetApprovalRule(ModActionType.ChannelPoints, RewardTitle, (result) => CustRewardDataBotCallback(User, RewardTitle, RewardMsg, result));
        }

        private void CustRewardDataBotCallback(LiveUser User, string RewardTitle, string RewardMsg, Tuple<string, string> approval)
        {
            if (approval != null)
            {
                LogWriter.DebugLog("HandleCustomReward", DebugLogTypes.TwitchBots, $"Custom reward {RewardTitle} requires approval.");

                switch (User.Platform)
                {
                    case Platform.Twitch:
                        DataBot.PostApproval($"{approval.Item2} {User.UserName} {RewardMsg}",
                            new(() =>
                            {
                                TwitchBots.PostInternalCommand(approval.Item2, [User.UserName, RewardMsg], $"!{approval.Item2} {User.UserName} {RewardMsg}");
                            })
                        );

                        TwitchBots.PostInternalCommand(LocalizedMsgSystem.GetVar(DefaultCommand.approve), [], $"!{LocalizedMsgSystem.GetVar(DefaultCommand.approve)}");
                        break;
                }
            }

            if (OptionFlags.MediaOverlayChannelPoints)
            {
                LogWriter.DebugLog("HandleCustomReward", DebugLogTypes.OverlayBot, $"Checking Channel Point Redemption {RewardTitle} for an Overlay request.");

                DataBot.CheckForOverlayEvent(OverlayTypes.ChannelPoints, RewardTitle, User);
            }
        }

        #region Giveaway
        /// <summary>
        /// Begin Giveaway with provided criteria. Registers the criteria for entering a viewer into the giveaway.
        /// </summary>
        /// <param name="giveawayTypes">The type of giveaway - e.g. channel point redemption, or command</param>
        /// <param name="ItemName">The identifier of the giveaway event.</param>
        public void HandleGiveawayBegin(GiveawayTypes giveawayTypes, string ItemName)
        {
            GiveawayItemType = giveawayTypes;
            GiveawayItemName = ItemName;
            GiveawayStarted = true;

            DataBot.BeginGiveaway();
        }

        /// <summary>
        /// Finish up/end the giveaway. Clears out data for next giveaway to start fresh.
        /// </summary>
        public void HandleGiveawayEnd()
        {
            DataBot.EndGiveaway();

            GiveawayItemType = GiveawayTypes.None;
            GiveawayItemName = "";
            GiveawayStarted = false;
        }

        /// <summary>
        /// Post a user to the giveaway.
        /// </summary>
        /// <param name="User">The user entering the giveaway.</param>
        public void HandleGiveawayPostName(LiveUser User)
        {
            DataBot.ManageGiveaway(User);
        }

        /// <summary>
        /// Finish up and award the giveaway winners.
        /// </summary>
        public void HandleGiveawayWinner()
        {
            if (GiveawayStarted)
            {
                HandleGiveawayEnd();
            }
            DataBot.PostGiveawayResult();
        }


        #endregion

        /// <summary>
        /// Initiates repeat timers.
        /// </summary>
        public void ActivateRepeatTimers()
        {
            DataBot.ActivateRepeatTimers();
        }

        /// <summary>
        /// Resets the repeat timer mode, which may be used to manage the state of repeat timers in the system, such as when a stream goes offline or when certain events occur that require resetting the timers. This method is specifically for resetting the repeat timer mode in the data bot, and may be called in various scenarios where managing the state of repeat timers is necessary.
        /// </summary>
        public void ResetRepeatTimerMode()
        {
            DataBot.ResetRepeatTimerMode();
        }

        #endregion

        #region UserBot

        private void Systems_BanUserRequest(object sender, BanUserRequestEventArgs e)
        {
            if (e.User.Platform == Platform.Twitch)
            {
                // TODO: verify users are correctly determined to be banned before banning, added to log
                LogWriter.WriteLog($"Request to ban or timeout user {e.User.UserName} for {e.BanReason} for {e.Duration} seconds.");
                //TwitchBots.BanUserRequest(e.UserName, e.BanReason, e.Duration);
            }
        }

        #endregion

        #region MediaOverlay


        /// <summary>
        /// Sends the initial ticker items to the overlay server bot, which may include information such as the latest subscriber, latest gift subscriber, or other relevant data that should be displayed on the stream overlay when it first loads. This method is specifically for sending the initial ticker items from the data bot to the overlay server bot, and may be called when the stream starts or when the overlay is initialized to ensure that the most up-to-date information is displayed on the overlay.
        /// </summary>
        public void SendInitialTickerItems()
        {
            DataBot.SendInitialTickerItems();
        }

        #endregion

        #region Ad Messages

        /// <summary>
        /// Starts the Twitch ad notification thread, which listens for ad-related events from the Twitch bot and sends notifications to the data bot when ads start, end, or are upcoming. This method is specifically for initiating the ad notification system that allows the data bot to manage any necessary actions related to Twitch ads, such as pausing certain functionalities during ads or displaying ad-related information on the stream overlay. The method may be called when the Twitch bot is initialized or when the stream goes live to ensure that ad notifications are properly set up and functioning throughout the stream.
        /// </summary>
        public void StartTwitchAdNotifications()
        {
            TwitchBots.StartAdNotificationThread();
        }

        private void TwitchBots_NotifyAdEnded(object sender, EventArgs e)
        {
            DataBot.NotifyAdEnd(Platform.Twitch);
        }

        private void TwitchBots_NotifyAdStarted(object sender, NotifyAdStartedEventArgs e)
        {
            DataBot.NotifyAdStart(Platform.Twitch, e.AdDuration);
        }

        private void TwitchBots_NotifyAdSoon(object sender, NotifyAdSoonEventArgs e)
        {
            DataBot.NotifyAdSoon(Platform.Twitch, e.SecondsUntilAd, e.AdDuration);
        }

        #endregion

        #endregion

    }
}
