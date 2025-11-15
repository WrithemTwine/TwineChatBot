using System.Globalization;
using System.Windows.Data;

namespace StreamerBotLib.DataSQL.Models.Converters
{
    public class IntConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return
                 (value.GetType().Name switch
                 {
                     "Int16" => (short)value,
                     "short" => (short)value,
                     "int" => (int)value,
                     "Int32" => (int)value,
                     "Int64" => (long)value,
                     "long" => (long)value,
                     "Int128" => (Int128)value,
                     _ => throw new NotImplementedException(),
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
}
