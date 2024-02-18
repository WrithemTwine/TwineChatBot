using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class GiveawayUserData(DateTime dateTime,
                                  string userId = default,
                                  string userName = default,
                                  Platform platform = Platform.Default) : UserBase(userId, userName, platform)
    {
        public DateTime DateTime { get; set; } = dateTime;
    }
}
