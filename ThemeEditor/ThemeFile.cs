using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Linq;

namespace ThemeEditor
{
    internal class ThemeFile
    {
        private static readonly string ThemePath = @"C:\Source\ChatBotApp\StreamerBotLib\Themes";

        public List<string> ThemeFiles { get; set; }

        public ThemeFile()
        {
            ThemeFiles = [];
            ThemeFiles.AddRange(from string t in
                                    from s in Directory.GetFiles(ThemePath)
                                    where s.Contains(".xaml") && s.Contains("Theme")
                                    select s
                                select t);
        }

        public static string ThemeFilePath(string filename)
        {
            return Path.Combine(ThemePath, filename) + ".xaml";
        }

        public List<ColorElement> OpenThemeFile(string themefile)
        {
            List<ColorElement> data = [];
            string path = ThemeFilePath(themefile);
            if (File.Exists(path))
            {
                XElement themedatafile = XElement.Load(path);
                data.AddRange(from item in themedatafile.Elements("Color")
                              select new ColorElement()
                              {
                                  Key = item.Attribute("x:Key").Value,
                                  Color = (Color)ColorConverter.ConvertFromString(item.Value)
                              });
            }
            return data;
        }

        public void SaveThemeFile(string themefile, List<ColorElement> data)
        {
            string path = ThemeFilePath(themefile);
            XElement themedatafile = XElement.Load(path);

            foreach(XElement color in themedatafile.Elements("Color"))
            {
                int idx = data.FindIndex(d => d.Key == color.Attribute("x:Key").Value);
                if(idx != -1)
                {
                    Color curr = (data[idx]).Color;
                    color.Value = $"#{curr.R:X2}{curr.G:X2}{curr.B:X2}";
                }
            }
            themedatafile.Save(path);
        }
    }

    internal class ThemeFileConverter : IValueConverter
    {
        private const string ThemePath = @"C:\Source\ChatBotApp\StreamerBotLib\Themes";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Path.GetFileNameWithoutExtension(value as string ?? "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Path.Combine(ThemePath, value as string ?? "", ".xaml");
        }
    }
}
