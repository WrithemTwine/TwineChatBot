using TwitchLib.Api.Helix.Models.Channels.GetAdSchedule;

namespace StreamerBotLib.Models
{
    internal class CurrAdSchedule(int snoozeCount, string snoozeRefreshAt, string nextAdAt, int duration, string lastAdAt, int prerollFreeTime)
    {
        public CurrAdSchedule(AdSchedule adSchedule) : this(adSchedule.SnoozeCount, adSchedule.SnoozeRefreshAt, adSchedule.NextAdAt, adSchedule.Duration, adSchedule.LastAdAt, adSchedule.PrerollFreeTime)
        { }

        public int SnoozeCount { get; } = snoozeCount;

        public DateTime SnoozeRefreshAt { get; } = DateTime.Parse(snoozeRefreshAt);

        public DateTime NextAdAt { get; } = DateTime.Parse(nextAdAt);

        public TimeSpan Duration { get; } = TimeSpan.FromSeconds(duration);

        public DateTime LastAdAt { get; } = DateTime.Parse(lastAdAt);

        public int PrerollFreeTime { get; } = prerollFreeTime;

        public DateTime GetAdEnd { get => NextAdAt.Add(Duration); }
    }
}
