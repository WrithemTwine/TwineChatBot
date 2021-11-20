using System;

namespace StreamerBot.Models
{
    public class Follow
    {
        public DateTime FollowedAt { get; set; }
        public string FromUserId { get; set; }
        public string FromUserName { get; set; }
        public string ToUserId { get; set; }
        public string ToUserName { get; set; }
    }
}
