using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    public class GameDeadCounter(string category = null, int counter = 0)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get { return Id; } }
        public string Category { get; set; } = category;
        public int Counter { get; set; } = counter;

        public CategoryList CategoryList { get; set; }
    }
}
