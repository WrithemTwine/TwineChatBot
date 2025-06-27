using EFEntityEntryTesting.Enums;

using Microsoft.EntityFrameworkCore;

using System.Diagnostics;

namespace EFEntityEntryTesting.EF
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [DebuggerDisplay("UserId={UserId}, WatchTime={WatchTime}")]
    public class UserStats(
                           TimeSpan watchTime = default,
                           int channelChat = 0,
                           int callCommands = 0,
                           int rewardRedeems = 0,
                           int clipsCreated = 0,
                           string userId = null,
                           Platform platform = Platform.Default)
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
