using System.Globalization;
using System.Windows.Data;

namespace StreamerBotLib.Models.Converters
{
    public class DayOfWeekToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DayOfWeek d)
                return culture.DateTimeFormat.GetDayName(d); // "Monday" / "lunes" / etc.
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
