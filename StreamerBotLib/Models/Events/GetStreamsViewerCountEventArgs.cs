namespace StreamerBotLib.Models.Events
{
    public class GetStreamsEventArgs : BotPlatformEventArgs
    {
        public int ViewerCount { get; set; }
    }
}
