using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.Diagnostics;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
    [Index(nameof(StatusChangeDate), nameof(UserId), nameof(FollowedDate), IsDescending = [true, false, true])]
    [DebuggerDisplay("UserId={UserId}, UserName={UserName}, IsFollower={IsFollower}")]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class Followers(
                           bool isFollower,
                           DateTime followedDate,
                           DateTime statusChangeDate,
                           string category,
                           DateTime addDate,
                           string userId,
                           string userName,
                           Platform platform)
#else
    public class OldFollowUsers(
                           bool isFollower = false,
                           DateTime followedDate = default,
                           DateTime statusChangeDate = default,
                           string category = null,
                           DateTime addDate = default,
                           string userId = null,
                           string userName = null,
                           Platform platform = Platform.Default)
#endif
    {
        public OldFollowUsers(Followers followers, string userName, DateTime statusChangeDate) : this()
        {
            Platform = followers.Platform;
            UserId = followers.UserId;
            UserName = userName;
            IsFollower = followers.IsFollower;
            FollowedDate = followers.FollowedDate;
            StatusChangeDate = statusChangeDate;
            Category = followers.Category;
            AddDate = followers.AddDate;
        }

        public string UserId { get; set; } = userId;
        public Platform Platform { get; set; } = platform;

        public string UserName { get; set; } = userName;
        public bool IsFollower { get; set; } = isFollower;
        public DateTime FollowedDate { get; set; } = followedDate;
        public DateTime StatusChangeDate { get; set; } = statusChangeDate;
        public string Category { get; set; } = category;
        public DateTime AddDate { get; set; } = addDate;

        //public Followers? Followers { get; set; }
    }
}
