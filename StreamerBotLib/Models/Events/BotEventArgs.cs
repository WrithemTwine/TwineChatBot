using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Events
{
    public class BotEventArgs
    {
        public BotEvents MethodName { get; set; }
        public EventArgs e { get; set; }
    }
}
