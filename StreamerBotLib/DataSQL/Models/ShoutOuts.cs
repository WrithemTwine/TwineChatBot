using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class ShoutOuts(uint id = 0, string userId = null, string userName = null, Platform platform = Platform.Default, uint value = 0) : UserBase(id, userId, userName, platform)
    {
        public uint Value { get; set; } = value;
    }
}
