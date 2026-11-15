using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub;
using StreamerBotLib.Models.Data;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Models.Users;
using StreamerBotLib.Static;

using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace StreamerBotLib.BotIOController
{
    public partial class BotController
    {
        #region Twitch Bot Events

        /// <summary>
        /// Interface method to ask TwitchFollowBot to query Twitch for the followers of the channel, which then posts the followers back into the system through the TwitchPostNewFollowers event method.
        /// </summary>
        public void TwitchStartUpdateAllFollowers()
        {
            TwitchBots.GetAllFollowers();
        }

        internal void TwitchPostNewFollowers(NewChannelFollowEventArgs Follower)
        {
            HandleBotEventNewFollowers(ConvertFollowers(Follower.Channel, Platform.Twitch));
        }

        /// <summary>
        /// Method to indicate the end of a bulk followers update operation, which is used to trigger any processes waiting for the bulk update to finish, such as comparing the new followers list to the old followers list to find new followers and lost followers.
        /// </summary>
        public void TwitchStopBulkFollowers()
        {
            HandleBotEventStopBulkFollowers();
        }

        /// <summary>
        /// Convert from Twitch Follower objects to generic "Models.Follow" objects.
        /// </summary>
        /// <param name="follows">The Twitch follows list to convert.</param>
        /// <returns>The follower list converted to the generic "Models.Follow" list.</returns>
        private static Follow ConvertFollowers(ChannelFollow follows, Platform Source)
        {
            return new Models.Users.Follow(

                follows.FollowedAt.DateTime.ToLocalTime(),
                follows.UserId,
                follows.UserName,
                Source,
                null // gets re-assigned in SystemController where Category is tracked
            );
        }

        /// <summary>
        /// Convert from Twitch Follower objects to generic "Models.Follow" objects.
        /// </summary>
        /// <param name="follows">The Twitch follows list to convert.</param>
        /// <returns>The follower list converted to the generic "Models.Follow" list.</returns>
        private static List<Follow> ConvertFollowers(List<ChannelFollower> follows, Platform Source)
        {
            return follows.ConvertAll((f) =>
            {
                return new Models.Users.Follow(

                    DateTime.Parse(f.FollowedAt).ToLocalTime(),
                    f.UserId,
                    f.UserName,
                    Source,
                    null // gets re-assigned in SystemController where Category is tracked
                );
            });
        }

        /// <summary>
        /// Interface method to indicate the Twitch EventSub bot has started, which can be used to trigger any processes waiting for the bot to start before performing certain operations that require the bot to be running, such as subscribing to Twitch events or starting to monitor Twitch channels for updates.
        /// </summary>
        /// <param name="args">The event arguments for the bot start event.</param>
        public void TwitchBotEventSubStarted(EventArgs args = null)
        {
            HandleChatBotStarted(Bots.TwitchEventSubBot, args);
        }

        /// <summary>
        /// Interface method to indicate the Twitch EventSub bot is stopping, which can be used to trigger any processes that need to wait for the bot to stop before performing certain operations, such as cleaning up resources or preparing to exit the application.
        /// </summary>
        /// <param name="args">The event arguments for the bot stopping event.</param>
        public void TwitchBotEventSubStopping(EventArgs args = null)
        {
            HandleChatBotStopping(Bots.TwitchEventSubBot, args);
        }

        /// <summary>
        /// Interface method to indicate the Twitch EventSub bot has stopped, which can be used to trigger any processes that need to wait for the bot to stop before performing certain operations, such as cleaning up resources or preparing to exit the application.
        /// </summary>
        /// <param name="args">The event arguments for the bot stopped event.</param>
        public void TwitchBotEventSubStopped(EventArgs args = null)
        {
            HandleChatBotStopped(Bots.TwitchEventSubBot, args);
        }

        /// <summary>
        /// Interface method to indicate the Twitch Follow bot has started and began bulk follower updates, which can be used to trigger any processes waiting for the bot to start before performing certain operations that require the bot to be running, such as subscribing to Twitch events or starting to monitor Twitch channels for updates.
        /// </summary>
        public void TwitchStartBulkFollowers()
        {
            HandleBotEventStartBulkFollowers();
        }

        /// <summary>
        /// Interface method to post a bulk list of followers from Twitch to the system, which is used to update the system with the current list of followers from Twitch after a bulk update operation. This can be used to compare the new followers list to the old followers list to find new followers and lost followers, and then trigger any events or notifications based on those changes in the followers list.
        /// </summary>
        /// <param name="Follower">The event arguments for the new followers detected event.</param>
        public void TwitchBulkPostFollowers(OnNewFollowersDetectedArgs Follower)
        {
            HandleBotEventBulkPostFollowers(ConvertFollowers(Follower.NewFollowers, Platform.Twitch));
        }

        /// <summary>
        /// Interface method to post new clips from Twitch to the system, which is used to update the system with new clips that have been created on Twitch. This can be used to trigger any events or notifications based on new clips being created, such as posting a message in the channel about the new clip or sending a notification to a webhook about the new clip. The ClipFoundEventArgs includes a list of clips and a flag indicating whether all clips were retrieved, which can be used to determine how to process the new clips in the system.
        /// </summary>
        /// <param name="clips">The event arguments for the clip found event.</param>
        public void TwitchClipSvcOnClipFound(ClipFoundEventArgs clips)
        {
            HandleBotEventPostNewClip(clips.AllClips, ConvertClips(clips.ClipList));
        }

        /// <summary>
        /// Convert from Twitch Clip objects to generic "Models.Clip" objects.
        /// </summary>
        /// <param name="clips">The Twitch Clip objects to convert.</param>
        /// <returns>A list of generic "Models.Clip" objects.</returns>
        public static List<Clip> ConvertClips(List<TwitchLib.Api.Helix.Models.Clips.GetClips.Clip> clips)
        {
            return clips.ConvertAll((SrcClip) =>
            {
                return new Models.Data.Clip()
                {
                    ClipId = SrcClip.Id,
                    CreatedAt = DateTime.Parse(SrcClip.CreatedAt).ToLocalTime(),
                    Duration = SrcClip.Duration,
                    GameId = SrcClip.GameId,
                    Language = SrcClip.Language,
                    Title = SrcClip.Title,
                    Url = SrcClip.Url,
                    EmbedUrl = SrcClip.EmbedUrl,
                    FromUserId = SrcClip.CreatorId,
                    FromUserName = SrcClip.CreatorName
                };
            });
        }

        /// <summary>
        /// Interface method to post new clips from Twitch to the system, which is used to update the system with new clips that have been created on Twitch. This can be used to trigger any events or notifications based on new clips being created, such as posting a message in the channel about the new clip or sending a notification to a webhook about the new clip. The OnNewClipsDetectedArgs includes a list of clips and a flag indicating whether all clips were retrieved, which can be used to determine how to process the new clips in the system.
        /// </summary>
        /// <param name="clips">The event arguments for the new clips detected event.</param>
        public void TwitchPostNewClip(OnNewClipsDetectedArgs clips)
        {
            HandleBotEventPostNewClip(clips.AllClips, ConvertClips(clips.Clips));
        }

        /// <summary>
        /// Send notification messages based on a monitored channel stream went live.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchMultiStreamOnline(OnStreamOnlineArgs e)
        {
            HandleMultiLiveOnStreamOnline(new(e.Stream.UserName, Platform.Twitch, e.Stream.UserId), e.Stream.Title,
                e.Stream.StartedAt.ToLocalTime(), e.Stream.GameName);
        }

        /// <summary>
        /// Send notification messages based on the Twitch stream went live, which includes the stream title, the stream start time, and the stream category. This is used to trigger any events or notifications based on the stream going live, such as posting a message in the channel about the stream going live or sending a notification to a webhook about the stream going live. The NewStreamOnlineEventArgs includes the stream information such as the broadcaster username, title, start time, and game/category information, which can be used to provide detailed information about the stream in the notifications.    
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchStreamOnline(NewStreamOnlineEventArgs e)
        {
            Stream CurrStream = TwitchBots.CurrStream;

            if (CurrStream != null)
            {
                HandleOnStreamOnline(
                    e.StreamOnline.BroadcasterUserName,
                    CurrStream.Title,
                    CurrStream.StartedAt.ToLocalTime(),
                    new(CurrStream.GameId, CurrStream.GameName)
                    );
            } // else; should not happen, but if it does, what should we do here?
        }

        /// <summary>
        /// Send notification messages based on the Twitch stream went live, which includes the stream title, the stream start time, and the stream category. This is used to trigger any events or notifications based on the stream going live, such as posting a message in the channel about the stream going live or sending a notification to a webhook about the stream going live. The ResumeStreamOnlineEventArgs includes the stream information such as the broadcaster username, title, start time, and game/category information, which can be used to provide detailed information about the stream in the notifications. This event is specifically for when the bot detects that the stream is already online when it starts up or when it resumes monitoring after a disconnect, so it can send notifications about the stream that is currently online without waiting for a new stream online event to trigger.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchResumeStreamOnline(ResumeStreamOnlineEventArgs e)
        {
            HandleOnStreamOnline(
                e.Stream.UserName,
                e.Stream.Title,
                e.Stream.StartedAt.ToLocalTime(),
                new(e.Stream.GameId, e.Stream.GameName)
                );
        }

        /// <summary>
        /// Send notification messages based on a monitored channel stream updated their stream category.
        /// </summary>
        /// <param name="e">The event arguments for the stream update event.</param>
        internal void TwitchStreamUpdate(NewChannelUpdateEventArgs e)
        {
            HandleOnStreamUpdate(new(e.ChannelUpdate.CategoryId, e.ChannelUpdate.CategoryName));
        }

        /// <summary>
        /// Send notification messages based on a Twitch stream updated their stream category, which includes the new category name and category Id. This is used to trigger any events or notifications based on the stream updating its category, such as posting a message in the channel about the new category or sending a notification to a webhook about the category change. The FindChannelCategoryEventArgs includes the new category information such as the category name and category Id, which can be used to provide detailed information about the new category in the notifications. This event is specifically for when the bot detects a category update for the stream, so it can send notifications about the new category when it changes.
        /// </summary>
        /// <param name="e">The event arguments for the category update event.</param>
        public void TwitchCategoryUpdate(FindChannelCategoryEventArgs e)
        {
            HandleOnStreamUpdate(new(e.GameId, e.GameName));
        }

        /// <summary>
        /// Send notification message from finding a viewer category.
        /// </summary>
        /// <param name="e">The event arguments for the found viewer category event.</param>
        public void TwitchFoundViewerCategory(FindChannelCategoryEventArgs e)
        {
            HandleFoundViewerCategory(new(e.GameId, e.GameName));
        }

        /// <summary>
        /// Send notification messages based on a Twitch stream went offline, which is used to trigger any events or notifications based on the stream going offline, such as posting a message in the channel about the stream going offline or sending a notification to a webhook about the stream going offline. The NewStreamOfflineEventArgs includes the stream information such as the broadcaster username and user Id, which can be used to provide detailed information about the stream that went offline in the notifications. This event is specifically for when the bot detects that the stream has gone offline, so it can send notifications about the stream going offline when it happens.
        /// </summary>
        /// <param name="e">The event arguments for the stream offline event.</param>
        internal void TwitchStreamOffline(NewStreamOfflineEventArgs e)
        {
            HandleOnStreamOffline(Platform.Twitch);
        }

        /// <summary>
        /// Send notification message for a new subscriber.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchNewSubscriber(NewChannelSubscribeEventArgs e)
        {
            HandleNewSubscriber(
                new LiveUser(e.ChannelSubscribe.UserName, Platform.Twitch, e.ChannelSubscribe.UserId),
                "1",
                e.ChannelSubscribe.Tier.Replace("0", ""),
                e.ChannelSubscribe.Tier.Replace("0", ""));
        }

        /// <summary>
        /// Send notification message for a resubscriber.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchReSubscriber(NewChannelSubscriptionMessageEventArgs e)
        {
            HandleReSubscriber(
                new(e.ChannelSubscriptionMessage.UserName, Platform.Twitch, e.ChannelSubscriptionMessage.UserId),
                e.ChannelSubscriptionMessage.DurationMonths,
                e.ChannelSubscriptionMessage.CumulativeMonths.ToString(),
                e.ChannelSubscriptionMessage.Tier.Replace("0", ""),
                e.ChannelSubscriptionMessage.Tier.Replace("0", ""),
                e.ChannelSubscriptionMessage.StreakMonths != null,
                e.ChannelSubscriptionMessage.StreakMonths.ToString());
        }

        /// <summary>
        /// Send notification message for a gifted subscription, which includes the username of the person who gifted the subscription (or anonymous if the gift was anonymous), the number of months gifted, and the subscription tier. This is used to trigger any events or notifications based on a gifted subscription, such as posting a message in the channel about the gifted subscription or sending a notification to a webhook about the gifted subscription. The NewChannelSubscribeEventArgs includes the information about the gifted subscription such as the gifter username, the number of months gifted, and the subscription tier, which can be used to provide detailed information about the gifted subscription in the notifications. This event is specifically for when the bot detects a new gifted subscription, so it can send notifications about the gifted subscription when it happens.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchGiftSubscription(NewChannelSubscribeEventArgs e)
        {
            HandleGiftSubscription(
                new(null, Platform.Twitch, null),
                "1",
                e.ChannelSubscribe.UserName,
                e.ChannelSubscribe.Tier.Replace("0", ""),
                e.ChannelSubscribe.Tier.Replace("0", ""));
        }

        /// <summary>
        /// Send notification message for a community gifted subscription, which includes the username of the person who gifted the subscription (or anonymous if the gift was anonymous), the total number of subscriptions gifted in the community gift, and the subscription tier. This is used to trigger any events or notifications based on a community gifted subscription, such as posting a message in the channel about the community gifted subscription or sending a notification to a webhook about the community gifted subscription. The NewChannelSubscriptionGiftEventArgs includes the information about the community gifted subscription such as the gifter username, the total number of subscriptions gifted, and the subscription tier, which can be used to provide detailed information about the community gifted subscription in the notifications. This event is specifically for when the bot detects a new community gifted subscription, so it can send notifications about the community gifted subscription when it happens.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        internal void TwitchCommunitySubscription(NewChannelSubscriptionGiftEventArgs e)
        {
            HandleCommunitySubscription(
                e.ChannelSubscriptionGift.IsAnonymous ? new(null, Platform.Twitch, null) : new(e.ChannelSubscriptionGift.UserName, Platform.Twitch, e.ChannelSubscriptionGift.UserId),
                e.ChannelSubscriptionGift.Total,
                e.ChannelSubscriptionGift.Tier.Replace("0", ""));
        }

        /// <summary>
        /// Send notification message for the current users in the Twitch channel when the bot starts up and detects existing users in the channel. This is used to trigger any events or notifications based on the existing users in the channel when the bot starts up, such as posting a message in the channel about the current viewers or sending a notification to a webhook about the current viewers. The StreamerOnExistingUserDetectedArgs includes a list of the current users in the channel, which can be used to provide detailed information about the current viewers in the notifications. This event is specifically for when the bot starts up and detects existing users in the channel, so it can send notifications about the current viewers at that time.
        /// </summary>
        /// <param name="e">The event arguments for the stream online event.</param>
        public void TwitchCurrentUsers(StreamerOnExistingUserDetectedArgs e)
        {
            HandleUserJoined(e.Users);
        }

        //public void TwitchOnUserJoined(StreamerOnUserJoinedArgs e)
        //{
        //    HandleUserJoined([e.LiveUser]);
        //}

        /// <summary>
        /// Send notification message for a user leaving the Twitch channel, which includes the username and user Id of the user who left. This is used to trigger any events or notifications based on a user leaving the channel, such as posting a message in the channel about the user leaving or sending a notification to a webhook about the user leaving. The StreamerOnUserLeftArgs includes the information about the user who left such as their username and user Id, which can be used to provide detailed information about the user who left in the notifications. This event is specifically for when the bot detects a user leaving the channel, so it can send notifications about the user leaving when it happens.
        /// </summary>
        /// <param name="e">The event arguments for the user left event.</param>
        public void TwitchOnUserLeft(StreamerOnUserLeftArgs e)
        {
            HandleUserLeft(e.LiveUser);
        }

        //public void TwitchOnUserTimedout(OnUserTimedoutArgs e = null)
        //{
        //    HandleUserTimedOut(e);
        //}

        //public void TwitchOnUserBanned(OnUserBannedArgs e = null)
        //{
        //    HandleUserBanned(new(e.UserBan.TargetUserId, Platform.Twitch, e.UserBan.Username));
        //}

        /// <summary>
        /// Send notification message for a Twitch chat message received, which includes the username and user Id of the user who sent the message, the channel the message was sent in, whether the user is a broadcaster, moderator, subscriber, etc., the content of the message, and any bits included in the message. This is used to trigger any events or notifications based on a chat message being received, such as posting a message in the channel about the received message or sending a notification to a webhook about the received message. The ChannelChatMessageEventArgs includes all the information about the chat message and the user who sent it, which can be used to provide detailed information about the chat message in the notifications. This event is specifically for when the bot detects a new chat message in the channel, so it can send notifications about the received chat message when it happens.
        /// </summary>
        /// <param name="e">The event arguments for the chat message received event.</param>
        public void TwitchMessageReceived(ChannelChatMessageEventArgs e)
        {
            LogWriter.DebugLog("TwitchMessageReceived", DebugLogTypes.BotController, $"Received message {e.ChannelChatMessage.Message.Text} from {e.ChannelChatMessage.ChatterUserName} in {e.ChannelChatMessage.BroadcasterUserName} channel.");

            HandleMessageReceived(
                new(
                channel: e.ChannelChatMessage.BroadcasterUserName,
                userId: e.ChannelChatMessage.ChatterUserId,
                displayName: e.ChannelChatMessage.ChatterUserName,
                isBroadcaster: e.ChannelChatMessage.IsBroadcaster,
                isModerator: e.ChannelChatMessage.IsModerator,
                isStaff: e.ChannelChatMessage.IsStaff,
                isSubscriber: e.ChannelChatMessage.IsSubscriber,
                isVip: e.ChannelChatMessage.IsVip,
                message: e.ChannelChatMessage.Message.Text,
                bits: e.ChannelChatMessage.Cheer?.Bits ?? 0,
                userType: ViewerTypes.Viewer, platform: Platform.Twitch));
        }

        /// <summary>
        /// When a Twitch raid is incoming, the Twitch bot captures the data about the raid and sends it to the system to handle the incoming raid data, which may include posting messages to the channel about the raid, and posting notifications to any webhooks about the raid.
        /// </summary>
        /// <param name="e">The raid event data</param>
        public void TwitchIncomingRaid(OnIncomingRaidArgs e)
        {
            HandleIncomingRaidData(e.LiveUser, e.RaidTime, e.ViewerCount, e.Category);
        }

        /// <summary>
        /// When a Twitch raid is outgoing, the Twitch bot captures the data about the raid and sends it to the system to handle the outgoing raid data, which may include posting messages to the channel about the raid, and posting notifications to any webhooks about the raid.
        /// </summary>
        /// <param name="e">The raid event data</param>
        public void TwitchOutgoingRaid(OnStreamRaidResponseEventArgs e)
        {
            LogWriter.DebugLog("TwitchOutgoingRaid", DebugLogTypes.BotController, "");

            HandleOutgoingRaidData(e.ToChannel, e.CreatedAt, Platform.Twitch);
        }

        /// <summary>
        /// When a Twitch chat message is received, the Twitch bot captures the data about the message and sends it to the system to handle the incoming message data, which may include checking if the message is a command, and if so, executing the command and posting any responses to the channel, and posting notifications to any webhooks about the command.
        /// </summary>
        /// <param name="e">The chat message event data</param>
        public void TwitchChatCommandReceived(ChannelChatMessageEventArgs e)
        {
            string commandtext = "";
            List<string> cmdarglist = [];
            bool foundcommand = false;

            foreach (string f in e.ChannelChatMessage.Message.Text.Split(' '))
            {
                if (f.StartsWith('!') && !foundcommand)
                {
                    foundcommand = true;
                    commandtext = f[1..].ToLower();
                }
                else if (foundcommand)
                {
                    cmdarglist.Add(f);
                }
            }

            LogWriter.DebugLog("TwitchMessageReceived", DebugLogTypes.BotController, $"Received message {e.ChannelChatMessage.Message.Text} from {e.ChannelChatMessage.ChatterUserName} in {e.ChannelChatMessage.BroadcasterUserName} channel.");

            HandleChatCommandReceived(
                new(
                    commandtext, cmdarglist,
                    e.ChannelChatMessage.BroadcasterUserName,
                    e.ChannelChatMessage.ChatterUserId,
                    e.ChannelChatMessage.ChatterUserName,
                    e.ChannelChatMessage.IsBroadcaster,
                    false, false,
                    e.ChannelChatMessage.IsModerator,
                    false, false,
                    e.ChannelChatMessage.IsStaff,
                    e.ChannelChatMessage.IsSubscriber,
                    false,
                    e.ChannelChatMessage.IsVip,
                    e.ChannelChatMessage.Message.Text,
                    bits: 0,
                    userType: ViewerTypes.Viewer,
                    platform: Platform.Twitch));
        }

        /// <summary>
        /// When a Twitch chat command is received, the Twitch bot captures the data about the command and sends it to the system to handle the incoming command data, which may include executing the command and posting any responses to the channel, and posting notifications to any webhooks about the command.
        /// </summary>
        /// <param name="e">The chat command event data</param>
        public void TwitchBotCommandCall(SendBotCommandEventArgs e)
        {
            HandleChatCommandReceived(e.CmdMessage);
        }

        /// <summary>
        /// When a Twitch channel point reward is redeemed, the Twitch bot captures the data about the redemption and sends it to the system to handle the incoming redemption data, which may include checking if the reward title matches any configured rewards in the system, and if so, executing any actions associated with that reward and posting any responses to the channel, and posting notifications to any webhooks about the reward redemption.
        /// </summary>
        /// <param name="e">The channel point reward redemption event data</param>
        internal void TwitchChannelPointsRewardRedeemed(NewChannelCustomRewardRedemptionEventArgs e)
        {
            // currently only need the invoking user DisplayName and the reward title, for determining the reward is used for the giveaway.
            // much more data exists in the resulting data output

            LogWriter.DebugLog("TwitchChannelPointsRewardRedeemed", DebugLogTypes.TwitchBots, $"Received Twitch Channel Point Reward {e.ChannelPointsCustomRewardRedemption.Reward.Title}. Now processing.");

            HandleCustomReward(
                new(e.ChannelPointsCustomRewardRedemption.UserName, Platform.Twitch, e.ChannelPointsCustomRewardRedemption.UserId),
                e.ChannelPointsCustomRewardRedemption.Reward.Title,
                e.ChannelPointsCustomRewardRedemption.UserInput
                );
        }

        /// <summary>
        /// When a Twitch channel cheer is received, the Twitch bot captures the data about the cheer and sends it to the system to handle the incoming cheer data, which may include checking if the cheer message contains any keywords that match with any configured rewards in the system, and if so, executing any actions associated with that reward and posting any responses to the channel, and posting notifications to any webhooks about the cheer.
        /// </summary>
        /// <param name="channelCheer">The channel cheer event data</param>
        internal void TwitchChannelCheered(NewChannelCheerEventArgs channelCheer)
        {
            HandleChannelCheer(new(channelCheer.ChannelCheer.UserName, Platform.Twitch, channelCheer.ChannelCheer.UserId), channelCheer.ChannelCheer.Bits);
        }

        #endregion



    }
}
