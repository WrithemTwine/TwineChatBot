using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class CustomWelcome(uint id = 0,
                               string userId = null,
                               string userName = null,
                               Platform platform = Platform.Default,
                               string message = null) : UserBase(id, userId, userName, platform)
    {
        public string Message { get; set; } = message;

        public Users? Users { get; set; }
    }
}
