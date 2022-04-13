using StreamerBotLib.Enums;

namespace StreamerBotLib.Models
{
    public class BanReason
    {
        public MsgTypes MsgType { get; set; }
        public BanReasons Reason { get; set; }
    }
}
