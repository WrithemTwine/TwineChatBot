using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace StreamerBotLib.DataSQL.Models.Converters
{
    public class CategoryConverter : ValidationRule, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Join(", ", (List<string>)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new List<string>(((string)value).Split(", "));
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            List<string> Data = value as List<string>;

            if (Data.Count > 1 && Data.Contains("All"))
            {
                return new(false, "All or other categories, not both.");
            }
            else
            {
                return new(true, "Successful.");
            }
        }
    }
}
