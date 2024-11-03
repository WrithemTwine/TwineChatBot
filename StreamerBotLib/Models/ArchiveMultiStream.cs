using StreamerBotLib.Enums;

namespace StreamerBotLib.Models
{
    public record ArchiveMultiStream
    {
        public string UserId { get; set; }
        public Platform Platform { get; set; }
        public int StreamCount { get; set; }
        public DateTime ThroughDate { get; set; }

    }
}
