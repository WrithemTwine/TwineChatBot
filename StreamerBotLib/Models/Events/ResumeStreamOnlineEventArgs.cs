using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace StreamerBotLib.Models.Events
{
    public class ResumeStreamOnlineEventArgs(Stream stream) : BotPlatformEventArgs
    {
        public Stream Stream { get; set; } = stream;
    }
}
