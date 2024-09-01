using Microsoft.EntityFrameworkCore;

namespace EFEntityEntryTesting.EF
{
    [PrimaryKey(nameof(Number))]
    [Index(nameof(Number))]
    public class Quotes(int number = 0, string quote = null)
    {
        public int Number { get; set; } = number;
        public string Quote { get; set; } = quote;
    }
}
