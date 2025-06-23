namespace StreamerBotLib.Models.Events
{
    public class StreamerOnUserLeftArgs : EventArgs
    {
        public LiveUser LiveUser { get; set; }
    }
}
