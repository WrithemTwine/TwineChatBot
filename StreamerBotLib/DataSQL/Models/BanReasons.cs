using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models.Enums;

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
                             StreamerBotLib.Models.Enums.BanReasons banReason = default)
#endif
  : EntityBase
    {
        public int Id { get; set; } = id;
        public MsgTypes MsgType { get; set; } = msgType;
        public StreamerBotLib.Models.Enums.BanReasons BanReason { get; set; } = banReason;
    }
}
