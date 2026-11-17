using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.BotManagement
{
    public class BotAutoStart(Bots botName, bool autoStart)
    {
        public Bots BotName { get; set; } = botName;

        public bool AutoStart { get; set; } = autoStart;
    }
}
