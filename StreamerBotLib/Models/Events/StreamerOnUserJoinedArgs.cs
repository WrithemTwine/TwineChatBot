namespace StreamerBotLib.Models.Events
{
    public class StreamerOnUserJoinedArgs : EventArgs
    {
        public LiveUser LiveUser { get; set; }
    }
}
