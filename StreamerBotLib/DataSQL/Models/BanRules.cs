using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(MsgType))]
    [Index(nameof(MsgType), nameof(ViewerTypes), nameof(ModAction))]
    public class BanRules(int id = 0,
                          ViewerTypes viewerTypes = default,
                          MsgTypes msgType = default,
                          ModActions modAction = default,
                          int timeoutSeconds = 0)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public ViewerTypes ViewerTypes { get; set; } = viewerTypes;
        public MsgTypes MsgType { get; set; } = msgType;
        public ModActions ModAction { get; set; } = modAction;
        public int TimeoutSeconds { get; set; } = timeoutSeconds;

        public BanReasons BanReasons { get; set; }
    }
}
