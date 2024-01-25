using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class MultiChannels(uint id = 0,
                               string userId = null,
                               string userName = null,
                               Platform platform = Platform.Default) : UserBase(id, userId, userName, platform)
    {
    }
}
