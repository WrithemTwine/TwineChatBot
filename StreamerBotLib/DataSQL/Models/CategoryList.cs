using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Static;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(CategoryId), nameof(Category), IsUnique = true)]
    public class CategoryList(int id = 0,
                              string categoryId = null,
                              string category = null,
                              int streamCount = 1)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public string CategoryId { get; set; } = categoryId;
        /// <summary>
        /// Formatted with escape characters
        /// </summary>
        public string Category { get; set; } = FormatData.AddEscapeFormat(category);
        public int StreamCount { get; set; } = streamCount;

        public GameDeadCounter GameDeadCounter { get; set; }
        public Clips? Clips { get; set; }
        public Followers? Followers { get; set; }
        public InRaidData? InRaidData { get; set; }
    }
}
