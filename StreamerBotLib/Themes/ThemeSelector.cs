using StreamerBotLib.Static;

using System.Reflection;

namespace StreamerBotLib.Themes
{
    /// <summary>
    /// Parses the OptionFlags properties for Theme settings, and determines which one the user selected.
    /// </summary>
    public static class ThemeSelector
    {
        private record class ThemeProperties
        {
            public string ThemeName { get; set; }
            public bool ThemeSelected { get; set; }
        }

        private static ThemeProperties[] ThemeList => (from PropertyInfo PropName in typeof(OptionFlags).GetProperties()
                                                       where PropName.Name.StartsWith("Theme")
                                                       select new ThemeProperties() { ThemeName = PropName.Name, ThemeSelected = (bool)PropName.GetValue(null) }).ToArray();

        /// <summary>
        /// Based on <see cref="OptionFlags"/> Theme Properties, determines which theme the user selected and returns the proper theme file name.
        /// </summary>
        /// <returns>A pack URI formatted resource file path of the user selected theme.</returns>
        public static string GetCurrentTheme()
        {
            ThemeProperties theme = (from ThemeProperties prop in ThemeList
                                     where prop.ThemeSelected == true
                                     select prop).FirstOrDefault();

            return $"pack://application:,,,/StreamerBotLib;component/Themes/{theme.ThemeName.Replace(OptionFlags.PrefixForThemes, "")}{OptionFlags.PrefixForThemes}.xaml";
        }
    }
}
