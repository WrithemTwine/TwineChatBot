using StreamerBotLib.Models.Enums;

using System.Diagnostics;

namespace StreamerBotLib.Models
{
    [DebuggerDisplay("UserId={UserId}, Platform={Platform}, StreamCount={StreamCount}, ThroughDate={ThroughDate}")]
    public record ArchiveMultiStream
    {
        public string UserId { get; set; }
        public Platform Platform { get; set; }
        public int StreamCount { get; set; }
        public DateTime ThroughDate { get; set; }

        public override string ToString()
        {
            return $"UserId: {UserId}, Platform: {Platform}, StreamCount: {StreamCount}, ThroughDate: {ThroughDate}";
        }
    }
}
