using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
    public class UserStats(int id = 0,
                           string userId = null,
                           string userName = null,
                           Platform platform = Platform.Default,
                           TimeSpan watchTime = default,
                           int channelChat = 0,
                           int callCommands = 0,
                           int rewardRedeems = 0,
                           int clipsCreated = 0) : UserBase(id, userId, userName, platform)
    {
        public TimeSpan WatchTime { get; set; } = watchTime;
        public int ChannelChat { get; set; } = channelChat;
        public int CallCommands { get; set; } = callCommands;
        public int RewardRedeems { get; set; } = rewardRedeems;
        public int ClipsCreated { get; set; } = clipsCreated;

        public Users Users { get; set; }

        public static UserStats operator +(UserStats userStats, UserStats otherStats)
        {
            userStats.WatchTime += otherStats.WatchTime;
            userStats.ChannelChat += otherStats.ChannelChat;
            userStats.CallCommands += otherStats.CallCommands;
            userStats.RewardRedeems += otherStats.RewardRedeems;
            userStats.ClipsCreated += otherStats.ClipsCreated;

            return userStats;
        }
    }
}
