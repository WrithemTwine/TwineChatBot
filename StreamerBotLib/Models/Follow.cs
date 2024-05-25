using StreamerBotLib.Enums;

namespace StreamerBotLib.Models
{
    public record Follow
    {
        public Follow(DateTime followedAt, string fromUserId, string fromUserName, Platform platform, string category)
        {
            FollowedAt = followedAt;
            FromUserId = fromUserId;
            FromUserName = fromUserName;
            FromUser = new(fromUserName, platform, fromUserId);
            Category = category;
        }

        public DateTime FollowedAt { get; set; }
        public string FromUserId { get; set; }
        public string FromUserName { get; set; }
        public LiveUser FromUser { get; set; }
        public string Category { get; set; }
    }


}
