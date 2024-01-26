using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(ViewerTypes), nameof(ModAction))]
    public class BanRules(int id = 0, ViewerTypes viewerTypes = default, MsgTypes msgTypes = default, ModActions modAction = default)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public ViewerTypes ViewerTypes { get; set; } = viewerTypes;
        public MsgTypes MsgTypes { get; set; } = msgTypes;
        public ModActions ModAction { get; set; } = modAction;
        public short TimeoutSeconds { get; set; }

        public BanReasons BanReasons { get; set; }
    }
}
