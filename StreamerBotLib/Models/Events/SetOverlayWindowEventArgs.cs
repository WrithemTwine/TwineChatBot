using StreamerBotLib.Overlay;

namespace StreamerBotLib.Models.Events
{
    public class SetOverlayWindowEventArgs
    {
        public Action<MediaOverlayPage> SetOverlay;
    }
}
