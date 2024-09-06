using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class MultiChannels(string userId,
                               string userName,
                               Platform platform)
#else
    public class MultiChannels(string userId = null,
                               string userName = null,
                               Platform platform = Platform.Default)
#endif
 : UserBase(userId, userName, platform), IEqualityComparer<MultiChannels>
    {
        public ICollection<MultiLiveStreams> MultiLiveStreams { get; } = [];

        public MultiSummaryLiveStreams? MultiSummaryLiveStreams { get; set; }

        public bool Equals(MultiChannels x, MultiChannels y)
        {
            return (x.UserId == y.UserId && x.UserName == y.UserName);
        }

        public int GetHashCode([DisallowNull] MultiChannels obj)
        {
            return (obj.UserId + obj.UserName).GetHashCode();
        }
    }
}
