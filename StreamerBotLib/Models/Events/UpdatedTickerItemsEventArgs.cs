namespace StreamerBotLib.Models.Events
{
    public class UpdatedTickerItemsEventArgs : EventArgs
    {
        public List<TickerItem> TickerItems { get; set; }
    }
}
