namespace StreamerBotLib.Models.Events
{
    internal class NotifyAdStartedEventArgs(TimeSpan adDuration) : BotPlatformEventArgs
    {
        public TimeSpan AdDuration { get; } = adDuration;
    }
}
