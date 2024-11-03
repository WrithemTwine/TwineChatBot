using TwitchLib.EventSub.Core.SubscriptionTypes.Stream;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub
{
    public class NewStreamOfflineEventArgs(StreamOffline streamOffline) : EventArgs
    {
        public StreamOffline StreamOffline { get; set; } = streamOffline;
    }
}
