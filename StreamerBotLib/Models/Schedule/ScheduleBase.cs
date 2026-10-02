using StreamerBotLib.Models.Enums;

using System.ComponentModel;
using System.Globalization;

namespace StreamerBotLib.Models.Schedule
{
    public class ScheduleBase : IEquatable<ScheduleBase>
    {
        protected Platform Platform;
        protected const string Delimiter = "|data|";
        public List<ScheduleConfig> PlatformSchedule { get; } = [];

        protected static List<string> _daysOfWeek = [.. Enum.GetNames<DayOfWeek>()];

        public virtual void Save()
        {
        }

        protected string PrepareSaveData()
        {
            return string.Join(Delimiter, (from s in PlatformSchedule
                                           select s.ToString()));
        }

        protected void PrepareLoadData(string srcdata)
        {
            var culture = CultureInfo.CurrentCulture;
            var firstDay = culture.DateTimeFormat.FirstDayOfWeek;

            if (!string.IsNullOrEmpty(srcdata))
            {
                PlatformSchedule.Clear();
                PlatformSchedule.AddRange(from s in srcdata.Split(Delimiter)
                                          select ScheduleConfig.FromString(s, Platform));
            }

            if (PlatformSchedule.Count == 0)
            {
                PlatformSchedule.AddRange(Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var dayOfWeek = (DayOfWeek)(((int)firstDay + i) % 7);
                    return new ScheduleConfig(false, dayOfWeek, "", "", Platform);
                }));
            }
        }

        public ScheduleConfig GetCurrDayConfig(DateTime currTime)
        {
            string day = CultureInfo.CurrentCulture.DateTimeFormat
       .GetDayName(currTime.DayOfWeek);
            return PlatformSchedule.FirstOrDefault(s => s.Day == currTime.DayOfWeek);
        }

        public void ResetDay()
        {
            foreach (var sched in PlatformSchedule)
            {
                sched.SentDay = false;
            }
        }

        public bool Equals(ScheduleBase other)
        {
            return this.Platform == other.Platform;
        }
    }

    /// <summary>
    /// Specifies the configuration for a schedule per day. Not all fields may be used depnding on the platform.
    /// </summary>
    /// <param name="sentDay"></param>
    /// <param name="day"></param>
    /// <param name="title"></param>
    /// <param name="categoryName"></param>
    public class ScheduleConfig(bool sentDay, DayOfWeek day, string title, string categoryName, Platform platform = Platform.Default) : INotifyPropertyChanged
    {
        private bool _sentDay;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SentDay
        {
            get => _sentDay;
            set
            {
                if (_sentDay == value) return;
                _sentDay = value;
                OnPropertyChanged(nameof(SentDay)); // INotifyPropertyChanged
            }
        }
        public DayOfWeek Day { get; set; } = day;
        public string Title { get; set; } = title;
        public string CategoryName { get; set; } = categoryName;
        public Platform Platform { get; set; } = platform;

        public override string ToString()
        {
            return string.Join("|", [(int)Day, Title, CategoryName]);
        }

        public static ScheduleConfig FromString(string data, Platform platform)
        {
            string[] source = data.Split("|");
            return new ScheduleConfig(false, (DayOfWeek)int.Parse(source[0]), source[1], source[2], platform);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }
    }
}
