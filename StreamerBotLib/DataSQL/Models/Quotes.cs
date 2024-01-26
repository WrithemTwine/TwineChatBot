using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(Number))]
    public class Quotes(short number = 0, string quote = null)
    {
        public short Number { get; set; } = number;
        public string Quote { get; set; } = quote;
    }
}
