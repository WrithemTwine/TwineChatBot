using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Windows.Data;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(MsgType), nameof(ViewerTypes), nameof(ModAction))]
    public class BanRules(int id = 0,
                          ViewerTypes viewerTypes = default,
                          MsgTypes msgType = default,
                          ModActions modAction = default,
                          int timeoutSeconds = 0)
    {
        public int Id { get; set; } = id;
        public ViewerTypes ViewerTypes { get; set; } = viewerTypes;
        [ForeignKey(nameof(BanReasons))]
        public MsgTypes MsgType { get; set; } = msgType;
        public ModActions ModAction { get; set; } = modAction;
        public int TimeoutSeconds { get; set; } = timeoutSeconds;
    }
}
