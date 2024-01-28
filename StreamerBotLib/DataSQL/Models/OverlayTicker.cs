﻿using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Overlay.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(TickerName))]
    [Index(nameof(Id), nameof(TickerName))]
    public class OverlayTicker(int id = 0, OverlayTickerItem tickerName = default, string userName = null) : EntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public OverlayTickerItem TickerName { get; set; } = tickerName;
        public string UserName { get; set; } = userName;
    }
}
