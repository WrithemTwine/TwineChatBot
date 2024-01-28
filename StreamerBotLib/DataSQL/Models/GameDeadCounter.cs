using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(CategoryId), nameof(Category))]
    public class GameDeadCounter(int id = 0,
                                 string categoryId = null,
                                 string category = null,
                                 int counter = 0) : EntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public string CategoryId { get; set; } = categoryId;
        public string Category { get; set; } = category;
        public int Counter { get; set; } = counter;

        public CategoryList CategoryList { get; set; }
    }
}
