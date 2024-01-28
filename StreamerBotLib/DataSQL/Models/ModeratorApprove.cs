using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(ModActionType), nameof(ModActionName))]
    [Index(nameof(Id), nameof(ModActionType), nameof(ModActionName))]
    public class ModeratorApprove(int id = 0,
                                  bool isEnabled = false,
                                  ModActionType modActionType = default,
                                  string modActionName = null,
                                  ModPerformType modPerformType = default,
                                  string modPerformAction = null) : EntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public bool IsEnabled { get; set; } = isEnabled;
        public ModActionType ModActionType { get; set; } = modActionType;
        public string ModActionName { get; set; } = modActionName;
        public ModPerformType ModPerformType { get; set; } = modPerformType;
        public string ModPerformAction { get; set; } = modPerformAction;

    }
}
