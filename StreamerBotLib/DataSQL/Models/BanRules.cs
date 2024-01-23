using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(ViewerTypes), nameof(ModAction))]
    public class BanRules(uint id = 0, ViewerTypes viewerTypes = default, MsgTypes msgTypes = default, ModActions modAction = default)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; } = id;
        public ViewerTypes ViewerTypes { get; set; } = viewerTypes;
        public MsgTypes MsgTypes { get; set; } = msgTypes;
        public ModActions ModAction { get; set; } = modAction;
        public ushort TimeoutSeconds { get; set; }
    }
}
