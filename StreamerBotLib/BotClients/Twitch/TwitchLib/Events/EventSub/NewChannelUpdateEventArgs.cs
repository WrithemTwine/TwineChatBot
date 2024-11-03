using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub
{
    public class NewChannelUpdateEventArgs(ChannelUpdate channelUpdate) : EventArgs
    {
        public ChannelUpdate ChannelUpdate { get; set; } = channelUpdate;
    }
}
