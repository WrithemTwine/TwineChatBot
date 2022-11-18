using StreamerBotLibMediaOverlayServer.Enums;
using StreamerBotLibMediaOverlayServer.Models;
using StreamerBotLibMediaOverlayServer.Static;

using System;
using System.Collections.Generic;

namespace StreamerBotLibMediaOverlayServer.Server
{
    public static class PrefixGenerator
    {
        private static string ServerAddress { get; } = $"http://localhost:{OptionFlags.MediaOverlayPort}/";

        private static List<string> Prefixes { get; set; } = new();
        private static List<OverlayPage> Links { get; set; } = new();

        public static List<string> GetPrefixes()
        {
            Prefixes.Clear();
            Links.Clear();

            if (OptionFlags.UseSameOverlayStyle)
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

            return Prefixes;
        }

        public static List<OverlayPage> GetLinks()
        {
            return Links;
        }
    }
}
