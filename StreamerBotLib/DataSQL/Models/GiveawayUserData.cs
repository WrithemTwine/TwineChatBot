using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class GiveawayUserData(DateTime dateTime,
                                  int id = 0,
                                  string userId = null,
                                  string userName = null,
                                  Platform platform = Platform.Default) : UserBase(id, userId, userName, platform)
    {
        public DateTime DateTime { get; set; } = dateTime;
    }
}
