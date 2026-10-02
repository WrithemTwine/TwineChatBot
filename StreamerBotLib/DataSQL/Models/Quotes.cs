using Microsoft.EntityFrameworkCore;

using System.Diagnostics;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Number))]
    [Index(nameof(Number))]
    [DebuggerDisplay("Number={Number}, Quote={Quote}")]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class Quotes(int number, string quote, string categoryName, string quoteDate)
#else
    public class Quotes(int number = 0, string quote = null, string categoryName = null, DateTime quoteDate = default)
#endif
     : EntityBase
    {

        public int Number { get; set; } = number;
        public string CategoryName { get; set; } = categoryName;
        public DateTime QuoteDate { get; set; } = quoteDate;
        public string Quote { get; set; } = quote;

        public override string ToString()
        {
            return $"{Number}: {QuoteDate:d t} {CategoryName} - {Quote}";
        }
    }
}
