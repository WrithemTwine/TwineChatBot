namespace StreamerBotLib.Models.Events
{
    internal class NotifyAdStartedEventArgs(TimeSpan adDuration) : EventArgs
    {
        public TimeSpan AdDuration { get; } = adDuration;
    }
}
