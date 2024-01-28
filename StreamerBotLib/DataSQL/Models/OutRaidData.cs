﻿using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(Id), nameof(ChannelRaided), IsUnique = true)]
    public class OutRaidData(int id = 0, string channelRaided = null, DateTime raidDate = default) : EntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public string ChannelRaided { get; set; } = channelRaided;
        public DateTime RaidDate { get; set; } = raidDate;
    }
}
