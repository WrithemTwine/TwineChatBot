using System;

namespace ChatBot_Net5.Events
{
    public class OnFoundNewFollowerEventArgs : EventArgs
    {
        public string FromUserName;
        public DateTime FollowedAt;
    }
}
