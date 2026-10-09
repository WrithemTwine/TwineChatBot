using StreamerBotLib.Static;

using System.ComponentModel;
using System.Globalization;

namespace StreamerBotLib.Models.Schedule
{       
    // add platforms for each built platform-schedule
    public enum SchedulePlatform { Twitch };
    
    public class ScheduleBase : IEquatable<ScheduleBase>
    {
        public SchedulePlatform Platform;
        protected const string Delimiter = "|data|";
        public static Dictionary<SchedulePlatform, List<ScheduleConfig>> PlatformSchedule { get; } = [];

        private string CurrData = "";

        public ScheduleBase()
        {
            int x = Enum.GetNames<SchedulePlatform>().Length;
            int savecount = OptionFlags.ScheduleData.Count;

            if (savecount < x)
            { // fill up the storage list with empty strings to match the number of platforms
                for (int i = savecount; i < x; i++)
                {
                    OptionFlags.ScheduleData.Add("");
                }
            }
        }

        public void Save()
        {
            string current = GetCurrDataString();

            if (current != CurrData)
            { // only save if the current content is different
                OptionFlags.ScheduleData[(int)Platform] = current;
                CurrData = current;
                OptionFlags.SaveSettings();
            }
        }

        private string GetCurrDataString()
        {
            return string.Join(Delimiter, PlatformSchedule[Platform].Select(s => s.ToString()));
        }

        protected void PrepareLoadData(SchedulePlatform platform)
        {
            Platform = platform;
            var culture = CultureInfo.CurrentCulture;
            var firstDay = culture.DateTimeFormat.FirstDayOfWeek;
            string srcdata = OptionFlags.ScheduleData[(int)platform];
            PlatformSchedule[platform] = [];

            if (!string.IsNullOrEmpty(srcdata))
            {
                PlatformSchedule[platform].Clear();
                PlatformSchedule[platform].AddRange(from s in srcdata.Split(Delimiter)
                                                    select ScheduleConfig.FromString(s, Platform));
            }
            else
            {
                PlatformSchedule[platform].AddRange(Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var dayOfWeek = (DayOfWeek)(((int)firstDay + i) % 7);
                    return new ScheduleConfig(dayOfWeek, "", "", Platform);
                }));
            }

            CurrData = GetCurrDataString();
        }

        public ScheduleConfig GetCurrDayConfig(DateTime currTime)
        {
            Save();
            return PlatformSchedule[Platform].FirstOrDefault(s => s.Day == currTime.DayOfWeek);
        }

        public void ResetDay()
        {
            foreach (var sched in PlatformSchedule[Platform])
            {
                sched.SentDay = false;
            }
        }

        public bool Equals(ScheduleBase other)
        {
            return this.Platform == other.Platform;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ScheduleBase);
        }

        public override int GetHashCode()
        {
            return Platform.GetHashCode();
        }
    }

    /// <summary>
    /// Specifies the configuration for a schedule per day. Not all fields may be used depnding on the platform.
    /// </summary>
    /// <param name="sentDay"></param>
    /// <param name="day"></param>
    /// <param name="title"></param>
    /// <param name="categoryName"></param>
    public class ScheduleConfig(DayOfWeek day, string title, string categoryName, SchedulePlatform platform = SchedulePlatform.Twitch) : INotifyPropertyChanged
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
        public SchedulePlatform Platform { get; set; } = platform;

        public override string ToString()
        {
            return string.Join("|", [(int)Day, Title, CategoryName]);
        }

        public static ScheduleConfig FromString(string data, SchedulePlatform platform)
        {
            string[] source = data.Split("|");
            return new ScheduleConfig((DayOfWeek)int.Parse(source[0]), source[1], source[2], platform);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }
    }
}
