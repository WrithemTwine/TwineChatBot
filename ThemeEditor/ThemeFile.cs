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
                                    where s.Contains("Theme.xaml")
                                    select Path.GetFileNameWithoutExtension(s)
                                select t);
        }

        public static string ThemeFilePath(string filename)
        {
            return filename == null ? "" : Path.Combine(ThemePath, filename) + ".xaml";
        }

        public List<ColorElement> OpenThemeFile(string themefile)
        {
            List<ColorElement> data = [];
            string path = ThemeFilePath(themefile);
            if (File.Exists(path))
            {
                XElement themedatafile = XElement.Load(path);
                XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                XNamespace xns = "http://schemas.microsoft.com/winfx/2006/xaml";
                data.AddRange(
                    from item in themedatafile.Elements(ns + "Color")
                    let keyAttr = item.Attribute(xns + "Key")
                    where keyAttr != null
                    select new ColorElement(
                        keyAttr.Value,
                        (Color)ColorConverter.ConvertFromString(item.Value)
                    )
                );
            }
            return data;
        }

        public void SaveThemeFile(string themefile, List<ColorElement> data)
        {
            string path = ThemeFilePath(themefile);
            XElement themedatafile = XElement.Load(path);
            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XNamespace xns = "http://schemas.microsoft.com/winfx/2006/xaml";

            foreach (XElement color in themedatafile.Elements(ns + "Color"))
            {
                var key = color.Attribute(xns + "Key")?.Value;
                int idx = data.FindIndex(d => d.Key == key);
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
                return ThemeFile.ThemeFilePath(value as string);
        }
    }
}
