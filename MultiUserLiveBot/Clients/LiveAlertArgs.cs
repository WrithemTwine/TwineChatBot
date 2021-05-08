using System;

using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace MultiUserLiveBot.Clients
{
    public class LiveAlertArgs : EventArgs
    {
        public Stream ChannelStream;
    }
}
