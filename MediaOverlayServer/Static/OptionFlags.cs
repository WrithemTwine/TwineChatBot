using MediaOverlayServer.Properties;

namespace MediaOverlayServer.Static
{
    /// <summary>
    /// Connects Application Settings and key flags to a single object for reference across classes
    /// </summary>
    internal static class OptionFlags
    {
        public static bool ActiveToken { get; set; }

        public static int MediaOverlayPort { get; set; }

        public static bool LogExceptions { get; set; }

        public static bool UseSameOverlayPage { get; set; }

        internal static void SetSettings()
        {
            Settings.Default.Save();

            MediaOverlayPort = Settings.Default.MediaOverlayPort;
            LogExceptions = Settings.Default.LogExceptions;
            UseSameOverlayPage = Settings.Default.UseSameOverlayPage;
        }
    }
}
