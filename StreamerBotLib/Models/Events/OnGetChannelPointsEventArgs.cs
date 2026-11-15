namespace StreamerBotLib.Models.Events
{
    public class OnGetChannelPointsEventArgs : BotPlatformEventArgs
    {
        public List<string> ChannelPointNames { get; set; }
    }
}
