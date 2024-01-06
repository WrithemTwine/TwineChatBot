namespace StreamerBotLib.Events
{
    public class StreamerOnUserLeftArgs : EventArgs
    {
        public Models.LiveUser LiveUser { get; set; }
    }
}
