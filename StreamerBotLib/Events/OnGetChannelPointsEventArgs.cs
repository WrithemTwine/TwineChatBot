namespace StreamerBotLib.Events
{
    public class OnGetChannelPointsEventArgs : EventArgs
    {
        public List<string> ChannelPointNames { get; set; }
    }
}
