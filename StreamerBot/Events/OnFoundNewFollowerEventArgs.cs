using System;

namespace StreamerBot.Events
{
    public class OnFoundNewFollowerEventArgs : EventArgs
    {
        public string FromUserName;
        public DateTime FollowedAt;
    }
}
