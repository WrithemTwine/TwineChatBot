using System.Diagnostics;

namespace StreamerBot.Models
{
    [DebuggerDisplay("Parameter={Parameter}, Value={Value}")]
    public class Command
    {
        public string Parameter { get; set; }
        public string Value { get; set; }
    }
}
