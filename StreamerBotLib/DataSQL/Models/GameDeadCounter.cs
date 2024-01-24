using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    public class GameDeadCounter(string category = null, uint counter = 0)
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get { return Id; } }
        public string Category { get; set; } = category;
        public uint Counter { get; set; } = counter;

        public CategoryList CategoryList { get; set; }
    }
}
