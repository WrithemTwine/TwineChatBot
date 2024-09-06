using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform), nameof(LiveDate))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class MultiLiveStreams(DateTime liveDate,
                                 string userId,
                                 string userName,
                                 Platform platform
                                 )
#else
    public class MultiLiveStreams(DateTime liveDate = default,
                                 string userId = null,
                                 string userName = null,
                                 Platform platform = Platform.Default
                                 )
#endif
 : UserBase(userId, userName, platform), IEqualityComparer<MultiLiveStreams>
    {
        public DateTime LiveDate { get; set; } = liveDate;

        public MultiChannels MultiChannels { get; set; }

        public bool Equals(MultiLiveStreams x, MultiLiveStreams y)
        {
            return (x.UserId == y.UserId && x.UserName == y.UserName && x.LiveDate == y.LiveDate);
        }

        public int GetHashCode([DisallowNull] MultiLiveStreams obj)
        {
            return (obj.UserId + obj.UserName + obj.LiveDate).GetHashCode();
        }
    }
}
