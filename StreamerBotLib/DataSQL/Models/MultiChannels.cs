using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models.Enums;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [DebuggerDisplay("UserId={UserId}, UserName={UserName}, MultiLiveStreams.Count={MultiLiveStreams.Count}, MultiSummaryLiveStreams={MultiSummaryLiveStreams}")]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class MultiChannels(string userId,
                               string userName,
                               Platform platform)
#else
    public class MultiChannels(string userId = null,
                               string userName = null,
                               Platform platform = Platform.Default)
#endif
 : UserBase(userId, platform), IEqualityComparer<MultiChannels>
    {
        public string UserName { get; set; } = userName;

        public ICollection<MultiLiveStreams> MultiLiveStreams { get; } = [];

#pragma warning disable CS8632 // MultiSummaryLiveStreams is an EFC navigation property
        public MultiSummaryLiveStreams? MultiSummaryLiveStreams { get; set; }
#pragma warning restore CS8632

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
