using StreamerBotLib.Systems.Overlay.Enums;

using System.IO;

namespace StreamerBotLib.Systems.Overlay.Static
{
    public static class PublicConstants
    {
        public static string PipeName { get; } = "MediaOverlayPipe";
        public static string AssemblyName { get; } = "MediaOverlayServer.exe";
        public static string OverlayPageName { get; } = "index.html";
        public static string OverlayVideoName { get; } = "video.html";
        public static string OverlayImageName { get; } = "image.html";
        public static string OverlayStyle { get; } = "overlaystyle.css";
        public static string BaseOverlayPath { get; } = "Overlay";
        public static string BaseTickerPath { get; } = "ticker";
        public static string TickerPageName { get; } = "ticker.html";
        public static string BaseTickerIconPath { get; } = "tickericons";
        public static string TickerIconsStyle { get; } = "tickericons.css";

        public static string OverlayAllActions { get; } = "All Actions";
        public static string OverlayAllTickers { get; } = "All Tickers";

        public static string TickerFile(string FileName) => $"{FileName}.css";
        internal static string TickerFile(TickerStyle tickerStyle)
        {
            return TickerFile(TickerStyleMap[tickerStyle]);
        }
        internal static Dictionary<TickerStyle, string> TickerStyleMap = new()
        {
            {TickerStyle.Single, BaseTickerPath },
            {TickerStyle.MultiStatic, "static" },
            {TickerStyle.MultiRotate, "rotate" },
            {TickerStyle.MultiMarquee, "marquee" }
        };

        public readonly static Dictionary<string, string> DefaultTickerIcons = (from S in Enum.GetNames<OverlayTickerItem>()
                                                                        select new Tuple<string, string>(
                                                                            S, 
                                                                            $"{BaseTickerIconPath}/Default{S}.png")).ToDictionary(k => k.Item1, e => e.Item2);

    }
}
