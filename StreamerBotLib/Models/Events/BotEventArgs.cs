namespace StreamerBotLib.Models.Events
{
    using StreamerBotLib.Models.Enums;

    public class BotEventArgs
    {
        public BotEvents MethodName { get; set; }
        public EventArgs e { get; set; }
    }
}
