using System;

namespace StreamerBot.Events
{
    public class GetUserIdResponseEventArgs : EventArgs
    {
        public string UserId { get; set; }
    }
}
