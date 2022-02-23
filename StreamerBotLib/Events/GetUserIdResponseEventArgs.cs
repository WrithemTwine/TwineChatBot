using System;

namespace StreamerBotLib.Events
{
    public class GetUserIdResponseEventArgs : EventArgs
    {
        public string UserId { get; set; }
    }
}
