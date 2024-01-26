﻿using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(MsgType), nameof(BanReason))]
    public class BanReasons(int id,
                            MsgTypes msgType = default,
                            Enums.BanReasons banReason = default) : BanBase(id, msgType)
    {
        public Enums.BanReasons BanReason { get; set; } = banReason;

        public BanRules BanRules { get; set; }
    }
}
