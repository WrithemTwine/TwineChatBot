namespace StreamerBotLib.Events
{
    public class PostChannelMessageEventArgs : EventArgs
    {
        public int RepeatMsg { get; set; } = 0;
        public string Msg { get; set; }
    }
}
