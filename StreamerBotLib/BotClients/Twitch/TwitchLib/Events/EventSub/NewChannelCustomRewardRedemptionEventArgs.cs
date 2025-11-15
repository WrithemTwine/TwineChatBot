using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub
{
    public class NewChannelCustomRewardRedemptionEventArgs(ChannelPointsCustomRewardRedemption channelPointsCustomRewardRedemption) : EventArgs
    {
        public ChannelPointsCustomRewardRedemption ChannelPointsCustomRewardRedemption { get; set; } = channelPointsCustomRewardRedemption;
    }
}
