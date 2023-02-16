using StreamerBotLib.Enums;
using StreamerBotLib.Overlay.Control;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Overlay.Static;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;

namespace StreamerBotLib.Overlay.Server
{
    public static class PrefixGenerator
    {
        private static string ServerAddress { get; } = $"http://localhost:{OptionFlags.MediaOverlayMediaPort}/";

        private static List<string> Prefixes { get; set; } = new();
        private static List<OverlayPage> Links { get; set; } = new();

        public static List<string> GetPrefixes()
        {
            Prefixes.Clear();
            Links.Clear();

            if (OptionFlags.MediaOverlayUseSameStyle)
            {
                Prefixes.Add(ServerAddress);
                Links.Add(new() { OverlayType = "All", OverlayHyperText = $"{ServerAddress}{PublicConstants.OverlayPageName}" });
            }
            else
            {
                foreach (string A in Enum.GetNames(typeof(OverlayTypes)))
                {
                    if (A != "None")
                    {
                        Prefixes.Add($"{ServerAddress}{A}/");
                        Links.Add(new() { OverlayType = A, OverlayHyperText = $"{ServerAddress}{A}/{PublicConstants.OverlayPageName}" });
                    }
                }
            }

            Prefixes.Add($"{ServerAddress}ticker/");
            if (OptionFlags.MediaOverlayTickerMulti)
            {
                Links.Add(new() { OverlayType = "All Tickers", OverlayHyperText = $"{ServerAddress}ticker/{PublicConstants.OverlayPageName}" });
            }
            else
            {
                foreach (SelectedTickerItem T in TickerFormatter.selectedTickerItems)
                {
                    Links.Add(new() { OverlayType = T.OverlayTickerItem, OverlayHyperText = $"{ServerAddress}ticker/{T.OverlayTickerItem}.html" });
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
