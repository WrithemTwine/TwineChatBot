namespace StreamerBotLib.Events
{
    public class OnGetChannelGameNameEventArgs : EventArgs
    {
        public string GameName { get; set; }
        public string GameId { get; set; }
    }
}
