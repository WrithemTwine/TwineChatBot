using System;

namespace StreamerBotLib.Models
{
    public class Follow : IEquatable<Follow>
    {
        public DateTime FollowedAt { get; set; }
        public string FromUserId { get; set; }
        public string FromUserName { get; set; }
        public string ToUserId { get; set; }
        public string ToUserName { get; set; }

        public bool Equals(Follow other)
        {
            return FollowedAt == other.FollowedAt &&
                FromUserId == other.FromUserId &&
                FromUserName == other.FromUserName &&
                ToUserId == other.ToUserId &&
                ToUserName == other.ToUserName;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Follow);
        }
    }


}
