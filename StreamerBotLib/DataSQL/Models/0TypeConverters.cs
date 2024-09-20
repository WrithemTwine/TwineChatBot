using System.Globalization;
using System.Windows.Controls;
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
            return
                 (value.GetType().Name switch
                 {
                     "Int16" => (short)value,
                     "short" => ((short)value),
                     "int" => ((int)value),
                     "Int32" => ((int)value),
                     "Int64" => (long)value,
                     "long" => ((long)value),
                     "Int128" => (Int128)value,
                 }).ToString("#,0", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(int))
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
            else if (targetType == typeof(short))
            {
                bool tryconvert = short.TryParse(value.ToString(), out short converted);

                if (tryconvert)
                {
                    return converted;
                }
                else
                {
                    return 0;
                }
            }
            else if (targetType == typeof(long))
            {
                bool tryconvert = long.TryParse(value.ToString(), out long converted);

                if (tryconvert)
                {
                    return converted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
    }

    public class CategoryConverter : ValidationRule, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return  (List<string>)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Join(", ",((List<string>)value));
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            List<string> Data = value as List<string>;

            if (Data.Count > 1 && Data.Contains("All"))
            {
                return new(false, "All or other categories, not both.");
            } else
            {
                return new(true, "Successful.");
            }
        }
    }
}
