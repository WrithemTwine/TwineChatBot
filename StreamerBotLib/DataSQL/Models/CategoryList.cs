using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Static;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(CategoryId), nameof(Category))]
    [Index(nameof(CategoryId), nameof(Category), IsUnique = true)]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class CategoryList(string categoryId,
                              string category,
                              int streamCount)
#else
    public class CategoryList(string categoryId = null,
                              string category = null,
                              int streamCount = 0)
#endif
        : EntityBase
    {

        public string CategoryId { get; set; } = categoryId;

        /// <summary>
        /// Formatted with escape characters
        /// </summary>
        public string Category { get; set; } = FormatData.AddEscapeFormat(category);

        public int StreamCount { get; set; } = streamCount;

        public GameDeadCounter GameDeadCounter { get; set; }

        public ICollection<Followers> Followers { get; } = [];

    }
}
