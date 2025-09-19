using Microsoft.EntityFrameworkCore;

using System.Diagnostics;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Number))]
    [Index(nameof(Number))]
    [DebuggerDisplay("Number={Number}, Quote={Quote}")]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class Quotes(int number, string quote)
#else
    public class Quotes(int number = 0, string quote = null)
#endif
     : EntityBase
    {

        public int Number { get; set; } = number;
        public string Quote { get; set; } = quote;
    }
}
