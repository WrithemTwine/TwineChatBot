using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Static;

using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(CategoryId), nameof(Category))]
    [Index(nameof(CategoryId), nameof(Category), IsUnique = true)]
    public class CategoryList(string categoryId = null,
                              string category = null,
                              int streamCount = 1) : EntityBase
    {
        public string CategoryId { get; set; } = categoryId;
        /// <summary>
        /// Formatted with escape characters
        /// </summary>
        public string Category { get; set; } = FormatData.AddEscapeFormat(category);
        public int StreamCount { get; set; } = streamCount;

        public GameDeadCounter GameDeadCounter { get; set; }
        public ICollection<Clips>? Clips { get; } = [];
        public ICollection<Followers>? Followers { get; } = [];
        public ICollection<InRaidData>? InRaidData { get; } = [];
        
    }
}
