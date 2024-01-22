using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Static;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(CategoryId), nameof(Category), IsUnique = true)]
    public class CategoryList(uint id = 0, string categoryId = null, string category = null, uint streamCount = 1)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; } = id;
        public string CategoryId { get; set; } = categoryId;
        /// <summary>
        /// Formatted with escape characters
        /// </summary>
        public string Category { get; set; } = FormatData.AddEscapeFormat(category);
        public uint StreamCount { get; set; } = streamCount;

    }
}
