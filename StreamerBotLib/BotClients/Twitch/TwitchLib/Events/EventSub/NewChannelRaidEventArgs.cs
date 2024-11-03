using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub
{
    public class NewChannelRaidEventArgs(ChannelRaid channelRaid) : EventArgs
    {
        public ChannelRaid ChannelRaid { get; set; } = channelRaid;
    }
}
