using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub
{
    public class NewChannelCheerEventArgs(ChannelCheer channelCheer) : EventArgs
    {
        public ChannelCheer ChannelCheer { get; set; } = channelCheer;
    }
}
