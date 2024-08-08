﻿using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(ModActionType), nameof(ModActionName))]
    [Index(nameof(ModActionType), nameof(ModActionName))]
    public class ModeratorApprove(bool isEnabled = false,
                                  ModActionType modActionType = default,
                                  string modActionName = null,
                                  ModPerformType modPerformType = default,
                                  string modPerformAction = null) : EntityBase
    {
        public bool IsEnabled { get; set; } = isEnabled;
        public ModActionType ModActionType { get; set; } = modActionType;
        public string ModActionName { get; set; } = modActionName;
        public ModPerformType ModPerformType { get; set; } = modPerformType;
        public string ModPerformAction { get; set; } = modPerformAction;

    }
}