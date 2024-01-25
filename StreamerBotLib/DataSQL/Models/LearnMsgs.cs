using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(Id), nameof(MsgType))]
    public class LearnMsgs(uint id = 0, MsgTypes msgType = default, string teachingMsg = null)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; } = id;

        public MsgTypes MsgType { get; set; } = msgType;
        public string TeachingMsg { get; set; } = teachingMsg;
    }
}
