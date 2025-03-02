using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(MsgType), nameof(ViewerTypes), nameof(ModAction))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class BanRules(int id,
                          ViewerTypes viewerTypes,
                          MsgTypes msgType,
                          ModActions modAction,
                          int timeoutSeconds)
#else
    public class BanRules(int id = 0,
                          ViewerTypes viewerTypes = default,
                          MsgTypes msgType = default,
                          ModActions modAction = default,
                          int timeoutSeconds = 0) 
#endif
: EntityBase
    {
        public int Id { get; set; } = id;
        public ViewerTypes ViewerTypes { get; set; } = viewerTypes;
        public MsgTypes MsgType { get; set; } = msgType;
        public ModActions ModAction { get; set; } = modAction;
        public int TimeoutSeconds { get; set; } = timeoutSeconds;
    }
}
