using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(ModActionType), nameof(ModActionName))]
    [Index(nameof(ModActionType), nameof(ModActionName))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class ModeratorApprove(bool isEnabled,
                                  ModActionType modActionType,
                                  string modActionName,
                                  ModPerformType modPerformType,
                                  string modPerformAction)
#else
    public class ModeratorApprove(bool isEnabled = false,
                                  ModActionType modActionType = default,
                                  string modActionName = null,
                                  ModPerformType modPerformType = default,
                                  string modPerformAction = null)
#endif
 : EntityBase
    {
        public bool IsEnabled { get; set; } = isEnabled;
        public ModActionType ModActionType { get; set; } = modActionType;
        public string ModActionName { get; set; } = modActionName;
        public ModPerformType ModPerformType { get; set; } = modPerformType;
        public string ModPerformAction { get; set; } = modPerformAction;

    }
}
