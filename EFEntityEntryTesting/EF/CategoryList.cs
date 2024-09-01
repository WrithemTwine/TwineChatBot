using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace EFEntityEntryTesting.EF
{
    [PrimaryKey(nameof(CategoryId), nameof(Category))]
    [Index(nameof(CategoryId), nameof(Category), IsUnique = true)]
    public class CategoryList(string categoryId = null,
                              string category = null,
                              int streamCount = 0)
    {
        [Column(Order = 1)]
        public string CategoryId { get; set; } = categoryId;

        /// <summary>
        /// Formatted with escape characters
        /// </summary>
        [Column(Order = 2)]
        public string Category { get; set; } = category;

        [Column(Order = 3)]
        public int StreamCount { get; set; } = streamCount;

    }
}
