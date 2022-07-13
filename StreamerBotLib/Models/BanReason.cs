using StreamerBotLib.Enums;

namespace StreamerBotLib.Models
{
    public record BanReason
    {
        public MsgTypes MsgType { get; set; }
        public BanReasons Reason { get; set; }
    }
}
