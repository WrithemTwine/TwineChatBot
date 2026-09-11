namespace StreamerBotLib.Models.Schedule
{
    public class ScheduleTwitch : ScheduleBase
    {
        public List<ScheduleTwitchConfig> TwitchSchedule { get; }

        public ScheduleTwitch()
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var firstDay = culture.DateTimeFormat.FirstDayOfWeek;

            TwitchSchedule = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var dayOfWeek = (DayOfWeek)(((int)firstDay + i) % 7);
                    string dayName = culture.DateTimeFormat.GetDayName(dayOfWeek); // full name

                    return new ScheduleTwitchConfig(dayName, "", "");
                })
                .ToList();
        }
    }

    public class ScheduleTwitchConfig(string day, string title, string categoryName)
    {
        public string Day { get; set; } = day;
        public string Title { get; set; } = title;
        public string CategoryName { get; set; } = categoryName;
    }
}
