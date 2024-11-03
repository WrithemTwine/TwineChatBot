using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace StreamerBotLib.Events
{
    public class ChannelChatMessageEventArgs(ChannelChatMessage channelChatMessage) : EventArgs
    {
        public ChannelChatMessage ChannelChatMessage { get; set; } = channelChatMessage;
    }
}
