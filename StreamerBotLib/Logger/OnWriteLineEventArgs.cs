namespace StreamerBotLib.Logger
{
    public class OnWriteLineEventArgs(string message) : EventArgs
    {
        public string Message { get; set; } = message;
    }
}
