namespace StreamerBotLib.Models.Events
{
    public class OnStreamRaidResponseEventArgs : BotPlatformEventArgs
    {
        public string ToChannel { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMature { get; set; }
    }
}
