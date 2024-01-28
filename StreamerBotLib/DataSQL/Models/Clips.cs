using Microsoft.EntityFrameworkCore;

using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(ClipId), nameof(CategoryId))]
    [Index(nameof(ClipId), nameof(CreatedAt), nameof(CategoryId))]
    public class Clips(int clipId = 0,
                       DateTime createdAt = default,
                       string title = null,
                       string categoryId = null,
                       string language = null,
                       decimal duration = 0,
                       string url = null) : EntityBase
    {
        public int ClipId { get; set; } = clipId;
        public DateTime CreatedAt { get; set; } = createdAt;
        public string Title { get; set; } = title;
        public string CategoryId { get; set; } = categoryId;
        public string Language { get; set; } = language;
        public decimal Duration { get; set; } = duration;
        public string Url { get; set; } = url;

        [AllowNull]
        public CategoryList? CategoryList { get; set; } = null;
    }
}
