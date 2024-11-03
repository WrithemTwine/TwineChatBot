using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(CategoryId), nameof(Category))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class GameDeadCounter(string categoryId,
                                 string category,
                                 int counter)
#else
    public class GameDeadCounter(string categoryId = null,
                                 string category = null,
                                 int counter = 0)
#endif
        : EntityBase
    {
        public string CategoryId { get; set; } = categoryId;
        public string Category { get; set; } = category;
        public int Counter { get; set; } = counter;

        public CategoryList CategoryList { get; set; }
    }
}
