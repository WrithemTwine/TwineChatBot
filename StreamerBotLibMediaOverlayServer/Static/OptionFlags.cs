using StreamerBotLibMediaOverlayServer.Properties;

namespace StreamerBotLibMediaOverlayServer.Static
{
    /// <summary>
    /// Connects Application Settings and key flags to a single object for reference across classes
    /// </summary>
    public static class OptionFlags
    {
        public static bool ActiveToken { get; set; }

        public static int MediaOverlayPort { get; set; }

        public static bool LogExceptions { get; set; }

        public static bool UseSameOverlayStyle { get; set; }

        public static bool AutoStart { get; set; }

        public static void SetSettings()
        {
            Settings.Default.Save();

            MediaOverlayPort = Settings.Default.MediaOverlayPort;
            LogExceptions = Settings.Default.LogExceptions;
            UseSameOverlayStyle = Settings.Default.UseSameOverlayStyle;
            AutoStart = Settings.Default.AutoStart;
        }
    }
}
