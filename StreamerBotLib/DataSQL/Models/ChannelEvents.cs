using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Name))]
    [Index(nameof(Name), IsUnique = true)]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class ChannelEvents(ChannelEventActions name,
                               short repeatMsg,
                               bool addMe,
                               bool isEnabled,
                               string message,
                               string commands)
#else
    public class ChannelEvents(ChannelEventActions name = default,
                               short repeatMsg = 0,
                               bool addMe = false,
                               bool isEnabled = false,
                               string message = null,
                               string commands = null)
#endif
 : EntityBase
    {
        public ChannelEventActions Name { get; set; } = name;
        public short RepeatMsg { get; set; } = repeatMsg;
        public bool AddMe { get; set; } = addMe;
        public bool IsEnabled { get; set; } = isEnabled;
        public string Message { get; set; } = message;
        public string Commands { get; set; } = commands;
    }
}
