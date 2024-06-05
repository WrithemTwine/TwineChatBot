using StreamerBotLib.Models;

namespace StreamerBotLib.Events
{
    public class UpdatedTickerItemsEventArgs : EventArgs
    {
        public List<TickerItem> TickerItems { get; set; }
    }
}
