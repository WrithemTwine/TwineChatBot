
namespace StreamerBotLib.DataSQL.Models
{
    using Microsoft.EntityFrameworkCore;

    using StreamerBotLib.Systems.Overlay.Enums;

    [PrimaryKey(nameof(TickerName))]
    [Index(nameof(TickerName))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class OverlayTicker(OverlayTickerItem tickerName, string userName)
#else
    public class OverlayTicker(OverlayTickerItem tickerName = default, string userName = null)
#endif
    : EntityBase
    {

        public OverlayTickerItem TickerName { get; set; } = tickerName;
        public string UserName { get; set; } = userName;
    }
}
