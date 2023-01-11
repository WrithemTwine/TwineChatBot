using System;
using System.Collections.Generic;

namespace StreamerBotLib.Events
{
    public class StreamerOnExistingUserDetectedArgs : EventArgs
    {
        public List<Models.LiveUser> Users { get; set; }
    }
}
