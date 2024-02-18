using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(MsgType), nameof(BanReason))]
    public class BanReasons(
                            int id = 0,
                            MsgTypes msgType = default,
                            Enums.BanReasons banReason = default)
    {
        public int Id { get; set; } = id;
        public MsgTypes MsgType { get; set; } = msgType;
        public Enums.BanReasons BanReason { get; set; } = banReason;

    }
}
