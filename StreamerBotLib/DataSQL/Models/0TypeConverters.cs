using System.Globalization;
using System.Windows.Data;

namespace StreamerBotLib.DataSQL.Models
{
    public class TimeoutSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value).ToString("#,0", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool tryconvert = int.TryParse((string)value, out int converted);

            if (tryconvert)
            {
                switch (converted)
                {
                    case > 1209600:
                        converted = 1209600;
                        break;
                    case < 0:
                        converted = 0;
                        break;
                }
                return converted;
            }
            else
            {
                return 0;
            }
        }
    }

    public class IntConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value).ToString("#,0", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool tryconvert = int.TryParse(value.ToString(), out int converted);

            if (tryconvert)
            {
                return converted;
            }
            else
            {
                return 0;
            }
        }
    }
}
