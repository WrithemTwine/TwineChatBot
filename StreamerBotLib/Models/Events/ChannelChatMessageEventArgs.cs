using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace StreamerBotLib.Models.Events
{
    public class ChannelChatMessageEventArgs(ChannelChatMessage channelChatMessage) : EventArgs
    {
        public ChannelChatMessage ChannelChatMessage { get; set; } = channelChatMessage;
    }
}
