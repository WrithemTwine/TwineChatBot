namespace TestStreamerBot.TestAds
{
    internal class NotifyAdStartedEventArgs(TimeSpan adDuration) : EventArgs
    {
        public TimeSpan AdDuration { get; } = adDuration;
    }
}
