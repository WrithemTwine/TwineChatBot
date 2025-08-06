using StreamerBotLib.Static;

using System.Globalization;
using System.Windows.Data;

namespace StreamerBotLib.Models
{
    public class StatValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(int))
            {
                return string.Format("{0:N0}", (int)value);
            }
            else if (value.GetType() == typeof(TimeSpan))
            {
                return FormatData.FormatTimes((TimeSpan)value);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
