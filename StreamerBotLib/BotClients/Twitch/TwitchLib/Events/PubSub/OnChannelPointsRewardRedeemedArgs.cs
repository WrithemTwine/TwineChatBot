using System;

using TwitchLib.PubSub.Models.Responses.Messages.Redemption;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.PubSub
{
    public class OnChannelPointsRewardRedeemedArgs : EventArgs
    {
        /// <summary>
        /// The ID of the channel that this event fired from.
        /// </summary>
        public string ChannelId { get; set; }
        /// <summary>
        /// Details about the reward that was redeemed
        /// </summary>
        public RewardRedeemed RewardRedeemed { get; set; }
    }
}
