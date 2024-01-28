using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    public abstract class BanBase(int id, MsgTypes msgType = default) : EntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; } = id;
        public MsgTypes MsgType { get; set; } = msgType;
    }
}
