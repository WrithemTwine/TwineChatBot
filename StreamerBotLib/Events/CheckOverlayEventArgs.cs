using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.Events
{
    public class CheckOverlayEventArgs : EventArgs
    {
        public OverlayTypes OverlayType { get; set; }
        public string Action { get; set; }
        public string UserName { get; set; } = "";
        public string UserMsg { get; set; } = "";
        public string ProvidedURL { get; set; } = "";
    }
}
