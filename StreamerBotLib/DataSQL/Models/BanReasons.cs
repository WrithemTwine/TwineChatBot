using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(MsgType), nameof(BanReason))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class BanReasons(
                            int id,
                            MsgTypes msgType,
                            Enums.BanReasons banReason)
#else
    public class BanReasons(
                            int id = 0,
                            MsgTypes msgType = default,
                            Enums.BanReasons banReason = default)
#endif
    {
        public int Id { get; set; } = id;
        public MsgTypes MsgType { get; set; } = msgType;
        public Enums.BanReasons BanReason { get; set; } = banReason;
    }
}
