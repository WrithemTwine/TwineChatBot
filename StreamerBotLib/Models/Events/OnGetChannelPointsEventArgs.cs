namespace StreamerBotLib.Models.Events
{
    public class OnGetChannelPointsEventArgs : EventArgs
    {
        public List<string> ChannelPointNames { get; set; }
    }
}
