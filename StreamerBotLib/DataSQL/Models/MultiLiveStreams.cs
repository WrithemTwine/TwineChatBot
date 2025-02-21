using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform), nameof(LiveDate))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class MultiLiveStreams(DateTime liveDate,
                                 string userId,
                                 string userName,
                                 Platform platform
                                 )
#else
    public class MultiLiveStreams(DateTime liveDate = default,
                                 string userId = null,
                                 Platform platform = Platform.Default
                                 )
#endif
 : UserBase(userId, platform), IEqualityComparer<MultiLiveStreams>
    {
        public DateTime LiveDate { get; set; } = liveDate;

        public MultiChannels MultiChannels { get; set; } = null!;

        public bool Equals(MultiLiveStreams x, MultiLiveStreams y)
        {
            return (x.UserId == y.UserId && x.Platform == y.Platform && x.LiveDate == y.LiveDate);
        }

        public int GetHashCode([DisallowNull] MultiLiveStreams obj)
        {
            return (obj.UserId + obj.Platform + obj.LiveDate).GetHashCode();
        }
    }
}
