using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Number))]
    [Index(nameof(Number))]
    public class Quotes(int number = 0, string quote = null) : EntityBase
    {
        public int Number { get; set; } = number;
        public string Quote { get; set; } = quote;
    }
}
