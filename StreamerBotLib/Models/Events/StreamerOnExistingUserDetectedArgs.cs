namespace StreamerBotLib.Models.Events
{
    public class StreamerOnExistingUserDetectedArgs : EventArgs
    {
        public List<LiveUser> Users { get; set; }
    }
}
