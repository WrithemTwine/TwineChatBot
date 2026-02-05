namespace TestStreamerBot.TestAds
{
    internal class NotifyAdSoonEventArgs(int secondsUntilAd, TimeSpan duration) : EventArgs
    {
        public int SecondsUntilAd { get; set; } = secondsUntilAd;
        public TimeSpan AdDuration { get; set; } = duration;
    }
}
