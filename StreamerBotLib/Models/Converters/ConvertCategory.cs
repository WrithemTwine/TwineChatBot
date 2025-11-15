using StreamerBotLib.Static;

using System.Globalization;
using System.Windows.Data;

namespace StreamerBotLib.Models.Converters
{
    #region Category Conversion

    public class ConvertCategory : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { // source data requires SQL-safe escaping, ' , due to SQL queries using source text and mismatching- ' -in the where clause
            List<string> output = [];

            if (value is string)
            {
                output.AddRange((value as string).Split(","));
            }
            else
            {
                output.AddRange(value as List<string>);
            }

            if (output == null || output.Count == 0)
            {
                output.Add("All");
            }
            else if (output[0] == "")
            {
                output[0] = "All";
            }

            output.RemoveAll(s => s is null);

            if (targetType == typeof(string))
            {
                return FormatData.RemoveEscapeFormat(string.Join(",", output));
            }
            else
            {
                return output.Select(c => FormatData.RemoveEscapeFormat(c)).ToList();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { // source data requires SQL-safe escaping, ' , due to SQL queries using source text and mismatching- ' -in the where clause
            return FormatData.AddEscapeFormat((string)value).Split(",").ToList();
        }
    }

    #endregion
}
