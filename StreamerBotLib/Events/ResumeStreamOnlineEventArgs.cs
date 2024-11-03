using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace StreamerBotLib.Events
{
    public class ResumeStreamOnlineEventArgs(Stream stream) : EventArgs
    {
        public Stream Stream { get; set; } = stream;
    }
}
