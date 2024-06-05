namespace StreamerBotLib.Models
{
    public record ArchiveMultiStream
    {
        public string Name { get; set; }
        public int StreamCount { get; set; }
        public DateTime ThroughDate { get; set; }

    }
}
