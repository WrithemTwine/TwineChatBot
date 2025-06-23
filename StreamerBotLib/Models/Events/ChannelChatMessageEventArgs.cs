
namespace StreamerBotLib.Models.Events
{
    using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

    public class ChannelChatMessageEventArgs(ChannelChatMessage channelChatMessage) : EventArgs
    {
        public ChannelChatMessage ChannelChatMessage { get; set; } = channelChatMessage;
    }
}
