using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub
{
    public class NewChannelRaidEventArgs(ChannelRaid channelRaid, DateTime raidTime) : EventArgs
    {
        public ChannelRaid ChannelRaid { get; set; } = channelRaid;
        public DateTime RaidTime { get; set; } = raidTime;
    }
}
