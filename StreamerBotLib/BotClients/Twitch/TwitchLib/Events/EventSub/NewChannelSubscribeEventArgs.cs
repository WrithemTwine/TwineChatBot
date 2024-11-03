using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub
{
    public class NewChannelSubscribeEventArgs(ChannelSubscribe channelSubscribe) : EventArgs
    {
        public ChannelSubscribe ChannelSubscribe { get; set; } = channelSubscribe;
    }
}
