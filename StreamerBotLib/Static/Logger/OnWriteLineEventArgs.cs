namespace StreamerBotLib.Static.Logger
{
    public class OnWriteLineEventArgs(string message) : EventArgs
    {
        public string Message { get; set; } = message;
    }
}
