using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(Id), nameof(ModActionType), nameof(ModActionName))]
    public class ModeratorApprove(uint id = 0, bool isEnabled = false, ModActionType modActionType = default, string modActionName = null, ModPerformType modPerformType = default, string modPerformAction = null)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; } = id;
        public bool IsEnabled { get; set; } = isEnabled;
        public ModActionType ModActionType { get; set; } = modActionType;
        public string ModActionName { get; set; } = modActionName;
        public ModPerformType ModPerformType { get; set; } = modPerformType;
        public string ModPerformAction { get; set; } = modPerformAction;

    }
}
