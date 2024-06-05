namespace StreamerBotLib.Events
{
    public class StreamerOnUserJoinedArgs : EventArgs
    {
        public Models.LiveUser LiveUser { get; set; }
    }
}
