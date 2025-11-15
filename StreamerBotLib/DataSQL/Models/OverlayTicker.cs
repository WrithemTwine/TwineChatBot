using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Systems.Overlay.Enums;

using System.Diagnostics;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(TickerName))]
    [Index(nameof(TickerName))]
    [DebuggerDisplay("TickerName={TickerName}, UserName={UserName}")]
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
