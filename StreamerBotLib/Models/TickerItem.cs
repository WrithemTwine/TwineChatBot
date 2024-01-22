using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.Models
{
    public class TickerItem(OverlayTickerItem overlayTickerItem = default, string userName = null) : IEquatable<TickerItem>
    {
        public TickerItem() : this(default, null) { }

        public OverlayTickerItem OverlayTickerItem { get; set; } = overlayTickerItem;
        public string UserName { get; set; } = userName;

        public bool Equals(TickerItem other)
        {
            return OverlayTickerItem.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TickerItem);
        }

        public override int GetHashCode()
        {
            return $"{OverlayTickerItem}_UserName".GetHashCode();
        }
    }
}
