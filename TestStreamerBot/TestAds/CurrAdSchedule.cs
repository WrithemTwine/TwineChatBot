using System.Diagnostics;

namespace TestStreamerBot.TestAds
{
    [DebuggerDisplay("NextAdAt={NextAdAt}, LastAdAt={LastAdAt}, SnoozeCount={SnoozeCount}")]
    internal class CurrAdSchedule(int snoozeCount, DateTime snoozeRefreshAt, DateTime nextAdAt, int duration, DateTime lastAdAt, int prerollFreeTime)
    {

        public int SnoozeCount { get; set; } = snoozeCount;

        public DateTime SnoozeRefreshAt { get; } = snoozeRefreshAt.ToLocalTime();

        public DateTime NextAdAt { get; set; } = nextAdAt.ToLocalTime();

        public TimeSpan Duration { get; } = TimeSpan.FromSeconds(duration);

        public DateTime LastAdAt { get; } = lastAdAt.ToLocalTime();

        public int PrerollFreeTime { get; } = prerollFreeTime;

        public DateTime GetAdEnd { get => NextAdAt.Add(Duration); }
    }
}
