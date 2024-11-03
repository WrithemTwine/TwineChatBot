using StreamerBotLib.Enums;

namespace StreamerBotLib.Events
{
    public class BotEventArgs
    {
        public BotEvents MethodName { get; set; }
        public EventArgs e { get; set; }
    }
}
