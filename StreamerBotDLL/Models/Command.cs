using System.Diagnostics;

namespace StreamerBotLib.Models
{
    [DebuggerDisplay("Parameter={Parameter}, Value={Value}")]
    public class Command
    {
        public string Parameter { get; set; }
        public string Value { get; set; }
    }
}
