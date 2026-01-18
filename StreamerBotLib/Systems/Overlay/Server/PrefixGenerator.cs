using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Control;
using StreamerBotLib.Systems.Overlay.Enums;
using StreamerBotLib.Systems.Overlay.Models;
using StreamerBotLib.Systems.Overlay.Static;

namespace StreamerBotLib.Systems.Overlay.Server
{
    public static class PrefixGenerator
    {
        private static string ServerAddress(int Port)
        {
            return $"http://localhost:{Port}/";
        }

        private static List<string> Prefixes { get; set; } = [];
        private static List<OverlayPage> Links { get; set; } = [];

        public static int LinkCount { get { return Links.Count; } }

        public static List<string> GetPrefixes()
        {
            Prefixes.Clear();
            Links.Clear();

            string ActionServerAddress = ServerAddress(OptionFlags.MediaOverlayMediaActionPort);

            // add base action server address - specifically for image & video alerts
            Prefixes.Add(ActionServerAddress);

            if (OptionFlags.MediaOverlayUseSameStyle)
            {
                Links.Add(new() { OverlayType = "All", OverlayHyperText = $"{ActionServerAddress}{PublicConstants.OverlayPageName}" });
            }
            else
            {
                foreach (string A in Enum.GetNames<OverlayTypes>())
                {
                    if (A != "None")
                    {
                        Prefixes.Add($"{ActionServerAddress}{A}/");
                        Links.Add(new() { OverlayType = A, OverlayHyperText = $"{ActionServerAddress}{A}/{PublicConstants.OverlayPageName}" });
                    }
                }
            }

            Links.Add(new() { OverlayType = "Overlay Video", OverlayHyperText = $"{ActionServerAddress}{PublicConstants.OverlayVideoName}" });
            Links.Add(new() { OverlayType = "Overlay Image", OverlayHyperText = $"{ActionServerAddress}{PublicConstants.OverlayImageName}" });

            string TickerServerAddress = ServerAddress(OptionFlags.MediaOverlayMediaTickerPort);
            Prefixes.Add($"{TickerServerAddress}ticker/");
            if (OptionFlags.MediaOverlayTickerMulti)
            {
                Links.Add(new() { OverlayType = "All Tickers", OverlayHyperText = $"{TickerServerAddress}ticker/{PublicConstants.TickerPageName}" });
            }
            else
            {
                foreach (SelectedTickerItem T in TickerFormatter.selectedTickerItems)
                {
                    Links.Add(new() { OverlayType = T.OverlayTickerItem, OverlayHyperText = $"{TickerServerAddress}ticker/{T.OverlayTickerItem}.html" });
                }
            }

            return Prefixes;
        }

        public static List<OverlayPage> GetLinks()
        {
            // ensure the links are current
            _ = GetPrefixes();

            return Links;
        }
    }
}
