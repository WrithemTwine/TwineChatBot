using System.Windows.Navigation;

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
        public static string TickerStyle(string FileName) => $"{FileName}.css";
    }
}
