using Microsoft.EntityFrameworkCore;

using System.Diagnostics;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(ClipId), nameof(CategoryId))]
    [Index(nameof(ClipId), nameof(CreatedAt), nameof(CategoryId))]
    [DebuggerDisplay("ClipId={ClipId}, CreatedAt={CreatedAt}, CategoryId={CategoryId}")]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class Clips(string clipId,
                       DateTime createdAt,
                       string title,
                       string categoryId,
                       string language,
                       float duration,
                       string url)
#else
    public class Clips(string clipId = "0",
                       DateTime createdAt = default,
                       string title = null,
                       string categoryId = null,
                       string language = null,
                       float duration = 0,
                       string url = null)
#endif
        : EntityBase
    {

        public string ClipId { get; set; } = clipId;
        public DateTime CreatedAt { get; set; } = createdAt;
        public string Title { get; set; } = title;
        public string CategoryId { get; set; } = categoryId;
        public string Language { get; set; } = language;
        public float Duration { get; set; } = duration;
        public string Url { get; set; } = url;

        public CategoryList CategoryList { get; set; } = null!;
    }
}
