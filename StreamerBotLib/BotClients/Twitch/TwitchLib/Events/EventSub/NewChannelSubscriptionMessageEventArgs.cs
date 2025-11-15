using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub
{
    public class NewChannelSubscriptionMessageEventArgs(ChannelSubscriptionMessage channelSubscriptionMessage) : EventArgs
    {
        public ChannelSubscriptionMessage ChannelSubscriptionMessage { get; set; } = channelSubscriptionMessage;
    }
}
