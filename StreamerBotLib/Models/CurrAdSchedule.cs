using System.Diagnostics;

using TwitchLib.Api.Helix.Models.Channels.GetAdSchedule;

namespace StreamerBotLib.Models
{
    [DebuggerDisplay("NextAdAt={NextAdAt}, LastAdAt={LastAdAt}, SnoozeCount={SnoozeCount}")]
    internal class CurrAdSchedule(int snoozeCount, string snoozeRefreshAt, string nextAdAt, int duration, string lastAdAt, int prerollFreeTime)
    {
        public CurrAdSchedule(AdSchedule adSchedule) : this(adSchedule.SnoozeCount, adSchedule.SnoozeRefreshAt, adSchedule.NextAdAt, adSchedule.Duration, adSchedule.LastAdAt, adSchedule.PrerollFreeTime)
        { }

        public int SnoozeCount { get; } = snoozeCount;

        public DateTime SnoozeRefreshAt { get; } = DateTimeOffset.FromUnixTimeSeconds(long.Parse(snoozeRefreshAt)).DateTime.ToLocalTime();

        public DateTime NextAdAt { get; } = DateTimeOffset.FromUnixTimeSeconds(long.Parse(nextAdAt)).DateTime.ToLocalTime();

        public TimeSpan Duration { get; } = TimeSpan.FromSeconds(duration);

        public DateTime LastAdAt { get; } = DateTimeOffset.FromUnixTimeSeconds(long.Parse(lastAdAt)).DateTime.ToLocalTime();

        public int PrerollFreeTime { get; } = prerollFreeTime;

        public DateTime GetAdEnd { get => NextAdAt.Add(Duration); }

        public new string ToString()
        {
            return $"NextAdAt: {NextAdAt}, Duration: {Duration}, SnoozeCount: {SnoozeCount}, SnoozeRefreshAt: {SnoozeRefreshAt}, LastAdAt: {LastAdAt}, PrerollFreeTime: {PrerollFreeTime}, GetAdEnd: {GetAdEnd}";
        }
    }
}
