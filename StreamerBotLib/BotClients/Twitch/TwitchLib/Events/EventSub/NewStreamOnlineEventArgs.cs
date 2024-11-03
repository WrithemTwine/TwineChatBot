using TwitchLib.EventSub.Core.SubscriptionTypes.Stream;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub
{
    public class NewStreamOnlineEventArgs(StreamOnline streamOnline) : EventArgs
    {
        public StreamOnline StreamOnline { get; set; } = streamOnline;
    }
}
