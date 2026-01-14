using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.Models
{
    public record Clip : IEqualityComparer<Clip>
    {
        public string ClipId { get; set; }
        public DateTime CreatedAt { get; set; }
        public float Duration { get; set; }
        public string GameId { get; set; }
        public string Language { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string EmbedUrl { get; set; }

        public string FromUserId { get; set; }
        public string FromUserName { get; set; }

        public bool Equals(Clip x, Clip y)
        {
            return x.ClipId == y.ClipId;
        }

        public int GetHashCode([DisallowNull] Clip obj)
        {
            return obj.ClipId.GetHashCode();
        }
    }
}
