
namespace StreamerBotLib.Models.Events
{
    using TwitchLib.Api.Helix.Models.Streams.GetStreams;

    public class ResumeStreamOnlineEventArgs(Stream stream) : EventArgs
    {
        public Stream Stream { get; set; } = stream;
    }
}
