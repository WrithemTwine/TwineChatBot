namespace StreamerBotLib.Events
{
    public class UpTimeCommandEventArgs : EventArgs
    {
        public string User { get; set; }
        public string Message { get; set; }
    }
}
