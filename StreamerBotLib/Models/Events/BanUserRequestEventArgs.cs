using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Users;

namespace StreamerBotLib.Models.Events
{
    public class BanUserRequestEventArgs : BotPlatformEventArgs
    {
        public LiveUser User { get; set; }
        public int Duration { get; set; }
        public BanReasons BanReason { get; set; }
    }
}
