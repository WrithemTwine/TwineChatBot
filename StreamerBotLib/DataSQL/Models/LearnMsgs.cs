using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models.Enums;

using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(MsgType))]
    [DebuggerDisplay("Id={Id}, MsgType={MsgType}, TeachingMsg={TeachingMsg}")]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class LearnMsgs(int id,
                           MsgTypes msgType,
                           string teachingMsg)
#else
    public class LearnMsgs(int id = 0,
                           MsgTypes msgType = default,
                           string teachingMsg = null)
#endif

        : EntityBase
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public MsgTypes MsgType { get; set; } = msgType;
        public string TeachingMsg { get; set; } = teachingMsg;
    }
}
