using StreamerBotLib.Overlay.Enums;

using System;

namespace StreamerBotLib.Models
{
    public class TickerItem : IEquatable<TickerItem>
    {
        public OverlayTickerItem OverlayTickerItem { get; set; }
        public string UserName { get; set; }

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
