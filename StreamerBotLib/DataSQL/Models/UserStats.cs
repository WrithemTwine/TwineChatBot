using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class UserStats(uint id = 0,
                           string userId = null,
                           string userName = null,
                           Platform platform = Platform.Default,
                           TimeSpan watchTime = default,
                           uint channelChat = 0,
                           uint callCommands = 0,
                           uint rewardRedeems = 0,
                           uint clipsCreated = 0) : UserBase(id, userId, userName, platform)
    {
        public TimeSpan WatchTime { get; set; } = watchTime;
        public uint ChannelChat { get; set; } = channelChat;
        public uint CallCommands { get; set; } = callCommands;
        public uint RewardRedeems { get; set; } = rewardRedeems;
        public uint ClipsCreated { get; set; } = clipsCreated;
    }
}
