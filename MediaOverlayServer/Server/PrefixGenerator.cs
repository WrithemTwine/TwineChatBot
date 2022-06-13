using MediaOverlayServer.Enums;
using MediaOverlayServer.Models;
using MediaOverlayServer.Static;

using System;
using System.Collections.Generic;

namespace MediaOverlayServer.Server
{
    internal static class PrefixGenerator
    {
        private static string ServerAddress { get; } = $"http://localhost:{OptionFlags.MediaOverlayPort}/";

        private static List<string> Prefixes { get; set; } = new();
        private static List<OverlayPage> Links { get; set; } = new();

        internal static List<string> GetPrefixes()
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

        internal static List<OverlayPage> GetLinks()
        {
            return Links;
        }
    }
}
