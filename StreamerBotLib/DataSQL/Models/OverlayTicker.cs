using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(TickerName))]
    [Index(nameof(TickerName))]
    public class OverlayTicker(OverlayTickerItem tickerName = default, string userName = null) : EntityBase
    {
        public OverlayTickerItem TickerName { get; set; } = tickerName;
        public string UserName { get; set; } = userName;
    }
}
