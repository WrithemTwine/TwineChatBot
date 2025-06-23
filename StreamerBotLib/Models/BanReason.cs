
namespace StreamerBotLib.Models
{
    using StreamerBotLib.Models.Enums;

    public record BanReason
    {
        public MsgTypes MsgType { get; set; }
        public BanReasons Reason { get; set; }
    }
}
