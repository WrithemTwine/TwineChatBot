using StreamerBotLib.Systems.Overlay.Models;

namespace StreamerBotLib.Models.Events
{
    public class OverlayBulkAddEventArgs : EventArgs
    {
        public OverlayBulkAddEventArgs(List<OverlayActionType> items)
        {
            Items = items;
        }
        public List<OverlayActionType> Items { get; set; }
    }
}
