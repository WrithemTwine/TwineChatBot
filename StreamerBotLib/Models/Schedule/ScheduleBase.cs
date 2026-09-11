namespace StreamerBotLib.Models.Schedule
{
    public class ScheduleBase
    {
        protected static List<string> _daysOfWeek = [.. Enum.GetNames<DayOfWeek>()];
    }
}
