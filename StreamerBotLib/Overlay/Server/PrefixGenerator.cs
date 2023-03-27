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
        private static string ServerAddress(int Port)
        {
            return $"http://localhost:{Port}/";
        }

        private static List<string> Prefixes { get; set; } = new();
        private static List<OverlayPage> Links { get; set; } = new();

        public static List<string> GetPrefixes()
        {
            Prefixes.Clear();
            Links.Clear();

            string ActionServerAddress = ServerAddress(OptionFlags.MediaOverlayMediaActionPort);
            if (OptionFlags.MediaOverlayUseSameStyle)
            {
                Prefixes.Add(ActionServerAddress);
                Links.Add(new() { OverlayType = "All", OverlayHyperText = $"{ActionServerAddress}{PublicConstants.OverlayPageName}" });
            }
            else
            {
                foreach (string A in Enum.GetNames(typeof(OverlayTypes)))
                {
                    if (A != "None")
                    {
                        Prefixes.Add($"{ActionServerAddress}{A}/");
                        Links.Add(new() { OverlayType = A, OverlayHyperText = $"{ActionServerAddress}{A}/{PublicConstants.OverlayPageName}" });
                    }
                }
            }

            string TickerServerAddress = ServerAddress(OptionFlags.MediaOverlayMediaTickerPort);
            Prefixes.Add($"{TickerServerAddress}ticker/");
            if (OptionFlags.MediaOverlayTickerMulti)
            {
                Links.Add(new() { OverlayType = "All Tickers", OverlayHyperText = $"{TickerServerAddress}ticker/{PublicConstants.OverlayPageName}" });
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
