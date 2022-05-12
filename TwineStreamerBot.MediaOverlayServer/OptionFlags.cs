
using TwineStreamerBot.MediaOverlayServer.Properties;

namespace TwineStreamerBot.MediaOverlayServer
{
    /// <summary>
    /// Connects Application Settings and key flags to a single object for reference across classes
    /// </summary>
    internal static class OptionFlags
    {
        public static bool ActiveToken { get; set; }

        public static int MediaOverlayPort { get; set; }


        internal static void SetSettings()
        {
            Settings.Default.Save();

            MediaOverlayPort = Settings.Default.MediaOverlayPort;
        }
    }
}
