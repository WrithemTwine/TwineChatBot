using System;
using System.Collections.Generic;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace ChatBot_Net5.Events
{
    public class DownloadedFollowersEventArgs : EventArgs
    {
        public string ChannelName;
        public List<Follow> FollowList;
    }
}
