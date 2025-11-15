using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub
{
    public class NewChannelFollowEventArgs(ChannelFollow channel) : EventArgs
    {
        public ChannelFollow Channel { get; set; } = channel;
    }
}
