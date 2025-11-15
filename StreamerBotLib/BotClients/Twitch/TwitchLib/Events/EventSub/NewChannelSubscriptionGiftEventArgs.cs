using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub
{
    public class NewChannelSubscriptionGiftEventArgs(ChannelSubscriptionGift channelSubscriptionGift) : EventArgs
    {
        public ChannelSubscriptionGift ChannelSubscriptionGift { get; set; } = channelSubscriptionGift;
    }
}
