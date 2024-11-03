using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(Id), nameof(ChannelRaided))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class OutRaidData(int id,
                             string channelRaided,
                             DateTime raidDate)
#else
    public class OutRaidData(int id = 0,
                             string channelRaided = null,
                             DateTime raidDate = default)
#endif
 : EntityBase
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public string ChannelRaided { get; set; } = channelRaided;
        public DateTime RaidDate { get; set; } = raidDate;
    }
}
