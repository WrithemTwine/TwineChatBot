using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]

#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class UserStats(
                           TimeSpan watchTime,
                           int channelChat,
                           int callCommands,
                           int rewardRedeems,
                           int clipsCreated,
                           string userId,
                           string userName,
                           Platform platform)
#else
    public class UserStats(
                           TimeSpan watchTime = default,
                           int channelChat = 0,
                           int callCommands = 0,
                           int rewardRedeems = 0,
                           int clipsCreated = 0,
                           string userId = null,
                           Platform platform = Platform.Default)
#endif
     : UserBase(userId, platform)
    {
        public TimeSpan WatchTime { get; set; } = watchTime == default ? new(0, 0, 0) : watchTime;
        public int ChannelChat { get; set; } = channelChat;
        public int CallCommands { get; set; } = callCommands;
        public int RewardRedeems { get; set; } = rewardRedeems;
        public int ClipsCreated { get; set; } = clipsCreated;

        public Users User { get; set; } = null!;

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
