﻿using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(MsgType))]
    public class LearnMsgs(int id = 0,
                           MsgTypes msgType = default,
                           string teachingMsg = null) : EntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public MsgTypes MsgType { get; set; } = msgType;
        public string TeachingMsg { get; set; } = teachingMsg;
    }
}