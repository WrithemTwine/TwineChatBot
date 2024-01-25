using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(Id), nameof(CreatedAt), nameof(GameId))]
    public class Clips(uint id = 0,
                       DateTime createdAt = default,
                       string title = null,
                       string gameId = null,
                       string language = null,
                       decimal duration = 0,
                       string url = null)
    {
        public uint Id { get; set; } = id;
        public DateTime CreatedAt { get; set; } = createdAt;
        public string Title { get; set; } = title;
        public string GameId { get; set; } = gameId;
        public string Language { get; set; } = language;
        public decimal Duration { get; set; } = duration;
        public string Url { get; set; } = url;

        public CategoryList? CategoryList { get; set; }
    }
}
