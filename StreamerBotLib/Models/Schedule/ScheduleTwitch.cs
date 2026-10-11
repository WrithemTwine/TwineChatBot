namespace StreamerBotLib.Models.Schedule
{
    public class ScheduleTwitch : ScheduleBase
    {
        public static List<ScheduleConfig> TwitchSchedule => PlatformSchedule[SchedulePlatform.Twitch];

        public ScheduleTwitch()
        {
            PrepareLoadData(SchedulePlatform.Twitch);
        }
    }
}
