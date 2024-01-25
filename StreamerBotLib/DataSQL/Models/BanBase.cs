using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    public abstract class BanBase(uint id, MsgTypes msgType = default)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public uint Id { get; set; } = id;
        public MsgTypes MsgType { get; set; } = msgType;
    }
}
