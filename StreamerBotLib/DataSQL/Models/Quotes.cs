using Microsoft.EntityFrameworkCore;

namespace StreamerBotLib.DataSQL.Models
{
    [Index(nameof(Number))]
    public class Quotes(ushort number = 0, string quote = null)
    {
        public ushort Number { get; set; } = number;
        public string Quote { get; set; } = quote;
    }
}
