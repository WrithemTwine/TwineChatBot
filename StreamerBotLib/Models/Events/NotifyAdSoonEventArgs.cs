namespace StreamerBotLib.Models.Events
{
    internal class NotifyAdSoonEventArgs(int secondsUntilAd, TimeSpan duration) : BotPlatformEventArgs
    {
        public int SecondsUntilAd { get; set; } = secondsUntilAd;
        public TimeSpan AdDuration { get; set; } = duration;
    }
}
