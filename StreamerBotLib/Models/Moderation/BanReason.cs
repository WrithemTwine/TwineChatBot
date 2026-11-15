using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Moderation
{
    public record BanReason
    {
        public MsgTypes MsgType { get; set; }
        public BanReasons Reason { get; set; }
    }
}
