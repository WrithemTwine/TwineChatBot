using StreamerBotLib.Overlay.Enums;

using System.Collections.Generic;

namespace StreamerBotLib.Overlay.Static
{
    public static class PublicConstants
    {
        public static string PipeName { get; } = "MediaOverlayPipe";
        public static string AssemblyName { get; } = "MediaOverlayServer.exe";
        public static string OverlayPageName { get; } = "index.html";
        public static string OverlayStyle { get; } = "overlaystyle.css";
        public static string BaseOverlayPath { get; } = "Overlay";
        public static string BaseTickerPath { get; } = "ticker";

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
    }
}
