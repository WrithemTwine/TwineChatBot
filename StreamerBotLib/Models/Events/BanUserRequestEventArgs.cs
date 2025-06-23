
namespace StreamerBotLib.Models.Events
{
    using StreamerBotLib.Models;
    using StreamerBotLib.Models.Enums;

    public class BanUserRequestEventArgs : EventArgs
    {
        public LiveUser User { get; set; }
        public int Duration { get; set; }
        public BanReasons BanReason { get; set; }
    }
}
